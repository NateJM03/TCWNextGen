using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using NexradViewer.Models;
using NexradViewer.Utils;

namespace NexradViewer.Services
{
    /// <summary>
    /// Client for accessing NEXRAD radar data from AWS S3
    /// Based on nextgenradar.txt specifications for unidata-nexrad-level2-chunks bucket
    /// </summary>
    public class AwsNexradClient
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ConfigurationService _config;
        
        // AWS bucket information for NEXRAD data
        private const string Level2Bucket = "noaa-nexrad-level2";
        private const string LiveChunksBucket = "unidata-nexrad-level2-chunks";
        private const string Level3Bucket = "noaa-nexrad-level3";
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AwsNexradClient"/> class
        /// </summary>
        public AwsNexradClient(ConfigurationService config = null)
        {
            _config = config ?? new ConfigurationService();
            
            // Create an S3 client without credentials (for public buckets)
            var s3Config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.USEast1, // NOAA buckets are in US East 1
                UseAccelerateEndpoint = false,
                UseDualstackEndpoint = false,
                ForcePathStyle = true
            };
            
            _s3Client = new AmazonS3Client(s3Config);
            Logger.LogInfo($"AwsNexradClient initialized for region {s3Config.RegionEndpoint.SystemName}");
        }
        
        /// <summary>
        /// Gets the latest Level 2 chunks for a station following the exact S3 naming convention:
        /// /<Site>/<Volume_Number>/<YYYYMMDD-HHMMSS-CHUNKNUM-CHUNKTYPE>
        /// </summary>
        public async Task<List<S3Object>> GetLatestLevel2ChunksAsync(string stationId)
        {
            try
            {
                Logger.LogInfo($"=== DETAILED S3 REQUEST DEBUG ===");
                Logger.LogInfo($"Station ID: {stationId}");
                Logger.LogInfo($"Bucket: {LiveChunksBucket}");
                Logger.LogInfo($"Current UTC Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
                
                // Create prefix for this station (e.g., "KFWS/")
                string stationPrefix = $"{stationId}/";
                Logger.LogInfo($"Station Prefix: '{stationPrefix}'");
                
                // List S3 objects in the chunks bucket for this station
                var request = new ListObjectsV2Request
                {
                    BucketName = LiveChunksBucket,
                    Prefix = stationPrefix,
                    MaxKeys = 200 // Get more chunks to ensure we have complete volumes
                };
                
                Logger.LogInfo($"S3 Request Details:");
                Logger.LogInfo($"  BucketName: {request.BucketName}");
                Logger.LogInfo($"  Prefix: '{request.Prefix}'");
                Logger.LogInfo($"  MaxKeys: {request.MaxKeys}");
                
                Logger.LogInfo($"Making S3 ListObjectsV2 request...");
                var response = await _s3Client.ListObjectsV2Async(request);
                
                Logger.LogInfo($"S3 Response received:");
                Logger.LogInfo($"  HttpStatusCode: {response.HttpStatusCode}");
                Logger.LogInfo($"  RequestId: {response.ResponseMetadata?.RequestId}");
                Logger.LogInfo($"  Object Count: {response.S3Objects.Count}");
                Logger.LogInfo($"  IsTruncated: {response.IsTruncated}");
                Logger.LogInfo($"  NextContinuationToken: {response.NextContinuationToken}");
                Logger.LogInfo($"  KeyCount: {response.KeyCount}");
                Logger.LogInfo($"  MaxKeys: {response.MaxKeys}");
                Logger.LogInfo($"  Name (Bucket): {response.Name}");
                Logger.LogInfo($"  Prefix: {response.Prefix}");
                
                if (response.S3Objects.Count == 0)
                {
                    Logger.LogWarning($"=== NO OBJECTS FOUND ===");
                    Logger.LogWarning($"No objects found for station {stationId} in bucket {LiveChunksBucket}");
                    Logger.LogWarning($"This could mean:");
                    Logger.LogWarning($"  1. Station ID '{stationId}' doesn't exist");
                    Logger.LogWarning($"  2. No current data for this station");
                    Logger.LogWarning($"  3. Bucket access issues");
                    Logger.LogWarning($"  4. Network connectivity problems");
                    
                    // Let's try listing without prefix to see if bucket is accessible
                    Logger.LogInfo($"Testing bucket accessibility...");
                    try
                    {
                        var testRequest = new ListObjectsV2Request
                        {
                            BucketName = LiveChunksBucket,
                            MaxKeys = 5 // Just a few to test
                        };
                        var testResponse = await _s3Client.ListObjectsV2Async(testRequest);
                        Logger.LogInfo($"Bucket test result: {testResponse.S3Objects.Count} objects found (any station)");
                        if (testResponse.S3Objects.Count > 0)
                        {
                            Logger.LogInfo($"Sample keys from bucket:");
                            foreach (var obj in testResponse.S3Objects.Take(3))
                            {
                                Logger.LogInfo($"  {obj.Key} (Size: {obj.Size}, Modified: {obj.LastModified})");
                            }
                        }
                    }
                    catch (Exception testEx)
                    {
                        Logger.LogException(testEx, "Bucket accessibility test");
                    }
                    
                    return new List<S3Object>();
                }
                
                Logger.LogInfo($"=== FOUND OBJECTS ===");
                int objectIndex = 0;
                foreach (var obj in response.S3Objects)
                {
                    objectIndex++;
                    Logger.LogInfo($"Object #{objectIndex}:");
                    Logger.LogInfo($"  Key: {obj.Key}");
                    Logger.LogInfo($"  Size: {obj.Size} bytes");
                    Logger.LogInfo($"  LastModified: {obj.LastModified:yyyy-MM-dd HH:mm:ss} UTC");
                    Logger.LogInfo($"  ETag: {obj.ETag}");
                    Logger.LogInfo($"  StorageClass: {obj.StorageClass}");
                }
                
                // Parse and group chunks by volume
                Logger.LogInfo($"=== PARSING CHUNK KEYS ===");
                var volumeGroups = new Dictionary<string, List<S3Object>>();
                int validChunks = 0;
                int invalidChunks = 0;
                
                foreach (var obj in response.S3Objects)
                {
                    Logger.LogInfo($"Parsing key: {obj.Key}");
                    var chunkInfo = ParseChunkKey(obj.Key);
                    if (chunkInfo != null)
                    {
                        validChunks++;
                        string volumeKey = $"{chunkInfo.Site}/{chunkInfo.VolumeNumber}";
                        if (!volumeGroups.ContainsKey(volumeKey))
                        {
                            volumeGroups[volumeKey] = new List<S3Object>();
                            Logger.LogInfo($"  Created new volume group: {volumeKey}");
                        }
                        volumeGroups[volumeKey].Add(obj);
                        
                        Logger.LogInfo($"  Valid chunk: Site={chunkInfo.Site}, Volume={chunkInfo.VolumeNumber}, ChunkNum={chunkInfo.ChunkNumber}, Type={chunkInfo.ChunkType}, Time={chunkInfo.Timestamp:yyyy-MM-dd HH:mm:ss}");
                    }
                    else
                    {
                        invalidChunks++;
                        Logger.LogWarning($"  INVALID chunk key format: {obj.Key}");
                    }
                }
                
                Logger.LogInfo($"=== PARSING SUMMARY ===");
                Logger.LogInfo($"Valid chunks: {validChunks}");
                Logger.LogInfo($"Invalid chunks: {invalidChunks}");
                Logger.LogInfo($"Volume groups: {volumeGroups.Count}");
                
                if (volumeGroups.Count == 0)
                {
                    Logger.LogWarning($"No valid volume groups found for {stationId}");
                    Logger.LogWarning($"All {response.S3Objects.Count} objects had invalid key formats");
                    return new List<S3Object>();
                }
                
                // Log each volume group
                Logger.LogInfo($"=== VOLUME GROUPS ===");
                foreach (var kvp in volumeGroups)
                {
                    var timestamp = GetVolumeTimestamp(kvp.Value);
                    Logger.LogInfo($"Volume: {kvp.Key} - {kvp.Value.Count} chunks, Latest: {timestamp:yyyy-MM-dd HH:mm:ss}");
                }
                
                // Get the most recent volume based on timestamp
                var mostRecentVolume = volumeGroups
                    .OrderByDescending(kvp => GetVolumeTimestamp(kvp.Value))
                    .First();
                
                Logger.LogInfo($"=== SELECTED VOLUME ===");
                Logger.LogInfo($"Selected volume: {mostRecentVolume.Key} with {mostRecentVolume.Value.Count} chunks");
                Logger.LogInfo($"Volume timestamp: {GetVolumeTimestamp(mostRecentVolume.Value):yyyy-MM-dd HH:mm:ss}");
                
                // Sort chunks within the volume by chunk number
                var sortedChunks = mostRecentVolume.Value
                    .OrderBy(obj => GetChunkNumber(obj.Key))
                    .ToList();
                
                Logger.LogInfo($"=== FINAL CHUNK LIST ===");
                for (int i = 0; i < sortedChunks.Count; i++)
                {
                    var chunk = sortedChunks[i];
                    var chunkInfo = ParseChunkKey(chunk.Key);
                    Logger.LogInfo($"Chunk #{i + 1}: {chunk.Key} (Type: {chunkInfo.ChunkType}, Num: {chunkInfo.ChunkNumber})");
                }
                
                Logger.LogInfo($"Returning {sortedChunks.Count} sorted chunks for volume {mostRecentVolume.Key}");
                Logger.LogInfo($"=== END S3 REQUEST DEBUG ===");
                return sortedChunks;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "GetLatestLevel2ChunksAsync");
                Logger.LogError($"Exception details:");
                Logger.LogError($"  Message: {ex.Message}");
                Logger.LogError($"  Type: {ex.GetType().Name}");
                Logger.LogError($"  Source: {ex.Source}");
                if (ex.InnerException != null)
                {
                    Logger.LogError($"  Inner Exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Error getting Level 2 chunks: {ex.Message}");
                return new List<S3Object>();
            }
        }
        
        /// <summary>
        /// Parses the S3 chunk key according to the format:
        /// /<Site>/<Volume_Number>/<YYYYMMDD-HHMMSS-CHUNKNUM-CHUNKTYPE>
        /// </summary>
        public ChunkInfo ParseChunkKey(string key)
        {
            try
            {
                var parts = key.Split('/');
                if (parts.Length != 3)
                {
                    Logger.LogWarning($"Invalid chunk key format: {key} (expected 3 parts, got {parts.Length})");
                    Logger.LogWarning($"Parts: [{string.Join("], [", parts)}]");
                    return null;
                }
                
                string site = parts[0];
                string volumeNumber = parts[1];
                string chunkPart = parts[2];
                
                // Parse the chunk part: YYYYMMDD-HHMMSS-CHUNKNUM-CHUNKTYPE
                var chunkParts = chunkPart.Split('-');
                if (chunkParts.Length != 4)
                {
                    Logger.LogWarning($"Invalid chunk part format: {chunkPart} (expected 4 parts, got {chunkParts.Length})");
                    Logger.LogWarning($"Chunk parts: [{string.Join("], [", chunkParts)}]");
                    return null;
                }
                
                string dateStr = chunkParts[0];
                string timeStr = chunkParts[1];
                string chunkNumStr = chunkParts[2];
                string chunkType = chunkParts[3];
                
                // Parse date and time
                if (!DateTime.TryParseExact($"{dateStr} {timeStr}", "yyyyMMdd HHmmss", 
                    null, System.Globalization.DateTimeStyles.None, out DateTime timestamp))
                {
                    Logger.LogWarning($"Invalid timestamp format: {dateStr}-{timeStr}");
                    return null;
                }
                
                // Parse chunk number
                if (!int.TryParse(chunkNumStr, out int chunkNumber))
                {
                    Logger.LogWarning($"Invalid chunk number: {chunkNumStr}");
                    return null;
                }
                
                // Validate chunk type (S, I, or E)
                if (chunkType != "S" && chunkType != "I" && chunkType != "E")
                {
                    Logger.LogWarning($"Invalid chunk type: {chunkType} (expected S, I, or E)");
                    return null;
                }
                
                return new ChunkInfo
                {
                    Site = site,
                    VolumeNumber = volumeNumber,
                    Timestamp = timestamp,
                    ChunkNumber = chunkNumber,
                    ChunkType = chunkType,
                    FullKey = key
                };
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"ParseChunkKey for key: {key}");
                return null;
            }
        }
        
        /// <summary>
        /// Gets the timestamp of the most recent chunk in a volume
        /// </summary>
        private DateTime GetVolumeTimestamp(List<S3Object> volumeChunks)
        {
            return volumeChunks.Max(obj => obj.LastModified);
        }
        
        /// <summary>
        /// Gets the chunk number from a chunk key
        /// </summary>
        private int GetChunkNumber(string key)
        {
            var chunkInfo = ParseChunkKey(key);
            return chunkInfo?.ChunkNumber ?? int.MaxValue;
        }
        
        /// <summary>
        /// Gets the list of all active NEXRAD stations
        /// </summary>
        public async Task<List<string>> GetAllStations()
        {
            try
            {
                Logger.LogInfo($"=== GETTING ALL STATIONS ===");
                Logger.LogInfo($"Bucket: {LiveChunksBucket}");
                
                // List all prefixes (station IDs) in the level 2 chunks bucket
                var request = new ListObjectsV2Request
                {
                    BucketName = LiveChunksBucket,
                    Delimiter = "/",
                    MaxKeys = 1000
                };
                
                Logger.LogInfo($"Making S3 request to list station prefixes...");
                var response = await _s3Client.ListObjectsV2Async(request);
                
                Logger.LogInfo($"Station prefixes response:");
                Logger.LogInfo($"  HttpStatusCode: {response.HttpStatusCode}");
                Logger.LogInfo($"  CommonPrefixes Count: {response.CommonPrefixes.Count}");
                
                if (response.CommonPrefixes.Count == 0)
                {
                    Logger.LogWarning($"No common prefixes found in bucket {LiveChunksBucket}");
                    return new List<string>();
                }
                
                Logger.LogInfo($"Found prefixes:");
                foreach (var prefix in response.CommonPrefixes.Take(10))
                {
                    Logger.LogInfo($"  {prefix}");
                }
                if (response.CommonPrefixes.Count > 10)
                {
                    Logger.LogInfo($"  ... and {response.CommonPrefixes.Count - 10} more");
                }
                
                // Extract station IDs from common prefixes
                var stations = response.CommonPrefixes
                    .Select(prefix => prefix.TrimEnd('/'))
                    .Where(stationId => !string.IsNullOrEmpty(stationId) && stationId.Length == 4)
                    .ToList();
                
                Logger.LogInfo($"Valid stations found: {stations.Count}");
                Logger.LogInfo($"First 20 stations: {string.Join(", ", stations.Take(20))}");
                
                return stations;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "GetAllStations");
                Console.WriteLine($"Error getting stations: {ex.Message}");
                return new List<string>();
            }
        }
        
        /// <summary>
        /// Downloads a Level 2 chunk from S3
        /// </summary>
        public async Task<MemoryStream> DownloadLevel2ChunkAsync(S3Object s3Object)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = LiveChunksBucket,
                    Key = s3Object.Key
                };
                
                Logger.LogInfo($"Downloading chunk: {s3Object.Key}");
                using (var response = await _s3Client.GetObjectAsync(request))
                {
                    var memoryStream = new MemoryStream();
                    await response.ResponseStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    Logger.LogInfo($"Downloaded chunk: {s3Object.Key}, size: {memoryStream.Length} bytes");
                    return memoryStream;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"DownloadLevel2ChunkAsync for key: {s3Object.Key}");
                Console.WriteLine($"Error downloading Level 2 chunk: {ex.Message}");
                return new MemoryStream();
            }
        }
        
        /// <summary>
        /// Gets the Level 3 product code for a radar product type
        /// </summary>
        public string GetLevel3ProductCode(Models.RadarProductType productType)
        {
            return Models.MappingTypes.Level3ProductCodes.TryGetValue(productType, out string code) ? code : string.Empty;
        }
        
        /// <summary>
        /// Gets the latest Level 3 product for a station
        /// </summary>
        public async Task<S3Object> GetLatestLevel3ProductAsync(string stationId, string productCode)
        {
            try
            {
                // Get current date in UTC (NEXRAD data is stored by date)
                var utcNow = DateTime.UtcNow;
                string year = utcNow.Year.ToString();
                string month = utcNow.Month.ToString("D2");
                string day = utcNow.Day.ToString("D2");
                
                // Create prefix for today's data for this station and product
                string prefix = $"{year}/{month}/{day}/{stationId}/{productCode}/";
                
                // List S3 objects in the level 3 bucket
                var request = new ListObjectsV2Request
                {
                    BucketName = Level3Bucket,
                    Prefix = prefix,
                    MaxKeys = 10 // Limit to the latest products
                };
                
                var response = await _s3Client.ListObjectsV2Async(request);
                
                // If no products found today, try yesterday
                if (response.S3Objects.Count == 0)
                {
                    var yesterday = utcNow.AddDays(-1);
                    year = yesterday.Year.ToString();
                    month = yesterday.Month.ToString("D2");
                    day = yesterday.Day.ToString("D2");
                    
                    prefix = $"{year}/{month}/{day}/{stationId}/{productCode}/";
                    request.Prefix = prefix;
                    response = await _s3Client.ListObjectsV2Async(request);
                }
                
                // Get the most recent product
                return response.S3Objects
                    .OrderByDescending(obj => obj.LastModified)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting Level 3 product: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Downloads a Level 3 product from S3
        /// </summary>
        public async Task<MemoryStream> DownloadLevel3ProductAsync(S3Object s3Object)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = Level3Bucket,
                    Key = s3Object.Key
                };
                
                using (var response = await _s3Client.GetObjectAsync(request))
                {
                    var memoryStream = new MemoryStream();
                    await response.ResponseStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    return memoryStream;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading Level 3 product: {ex.Message}");
                return new MemoryStream();
            }
        }
        
        /// <summary>
        /// Determines if an S3 object is a start chunk (type S)
        /// </summary>
        public bool IsStartChunk(S3Object s3Object)
        {
            var chunkInfo = ParseChunkKey(s3Object.Key);
            return chunkInfo?.ChunkType == "S";
        }
        
        /// <summary>
        /// Determines if an S3 object is an end chunk (type E)
        /// </summary>
        public bool IsEndChunk(S3Object s3Object)
        {
            var chunkInfo = ParseChunkKey(s3Object.Key);
            return chunkInfo?.ChunkType == "E";
        }
        
        /// <summary>
        /// Determines if an S3 object is an intermediate chunk (type I)
        /// </summary>
        public bool IsIntermediateChunk(S3Object s3Object)
        {
            var chunkInfo = ParseChunkKey(s3Object.Key);
            return chunkInfo?.ChunkType == "I";
        }
    }
    
    /// <summary>
    /// Information extracted from a NEXRAD chunk S3 key
    /// </summary>
    public class ChunkInfo
    {
        public string Site { get; set; }
        public string VolumeNumber { get; set; }
        public DateTime Timestamp { get; set; }
        public int ChunkNumber { get; set; }
        public string ChunkType { get; set; } // S, I, or E
        public string FullKey { get; set; }
    }
}
