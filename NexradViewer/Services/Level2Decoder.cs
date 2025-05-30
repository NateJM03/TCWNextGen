using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SharpCompress.Compressors;
using SharpCompress.Compressors.BZip2;
using NexradViewer.Models;
using NexradViewer.Utils;

namespace NexradViewer.Services
{
    /// <summary>
    /// Decoder for NEXRAD Level 2 data
    /// </summary>
    public class Level2Decoder
    {
        private readonly AwsNexradClient _awsClient;
        private readonly Action<string> _logger;
        
        // Constants for decoding NEXRAD data
        private const int VHR_SIZE = 24;  // Volume Header Record size
        private const int CTM_SIZE = 12;  // Channel Terminal Message header size
        private const int MESSAGE_HEADER_SIZE = 16;  // Standard Message Header size
        
        // Constants for Message Type 31
        private const byte MSG_TYPE_31 = 31;  // Message Type for Level 2 data
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Level2Decoder"/> class
        /// </summary>
        public Level2Decoder(AwsNexradClient awsClient, Action<string> logger = null)
        {
            _awsClient = awsClient;
            _logger = logger ?? (msg => { });
            Logger.LogInfo("Level2Decoder initialized");
        }
        
        /// <summary>
        /// Decodes the latest scan for a station and product
        /// </summary>
        public async Task<RadarData> DecodeLatestScan(string stationId, Models.RadarProductType productType, int tiltIndex = 0)
        {
            Logger.LogInfo($"=== LEVEL2DECODER START ===");
            Logger.LogInfo($"DecodeLatestScan called: StationId={stationId}, Product={productType}, TiltIndex={tiltIndex}");
            Log($"Decoding latest scan for {stationId}, product {productType}, tilt {tiltIndex}");
            
            try
            {
                Logger.LogInfo($"About to call _awsClient.GetLatestLevel2ChunksAsync({stationId})");
                
                // Get the latest chunks for this station
                var chunks = await _awsClient.GetLatestLevel2ChunksAsync(stationId);
                
                Logger.LogInfo($"GetLatestLevel2ChunksAsync returned: {(chunks == null ? "NULL" : $"{chunks.Count} chunks")}");
                
                if (chunks == null || chunks.Count == 0)
                {
                    Logger.LogWarning($"No chunks found for {stationId} - this triggers our detailed S3 logging");
                    Log($"No chunks found for {stationId}");
                    return null;
                }
                
                Logger.LogInfo($"SUCCESS: Got {chunks.Count} chunks for {stationId}");
                Log($"Got {chunks.Count} chunks for {stationId}");
                
                // Find the start chunk (type S) for the latest volume
                var startChunk = chunks.FirstOrDefault(c => _awsClient.IsStartChunk(c));
                
                if (startChunk == null)
                {
                    Logger.LogWarning($"No start chunk found for {stationId}");
                    Log($"No start chunk found for {stationId}");
                    return null;
                }
                
                Logger.LogInfo($"Found start chunk: {startChunk.Key}");
                
                // Extract the volume number from the key
                string volumeNumber = ExtractVolumeNumber(startChunk.Key);
                Logger.LogInfo($"Extracted volume number: {volumeNumber}");
                
                // Filter chunks for this volume
                var volumeChunks = chunks.Where(c => c.Key.Contains($"/{volumeNumber}/")).ToList();
                
                Logger.LogInfo($"Found {volumeChunks.Count} chunks for volume {volumeNumber}");
                Log($"Found {volumeChunks.Count} chunks for volume {volumeNumber}");
                
                // Download and process the start chunk first (contains VHR)
                Logger.LogInfo($"About to download start chunk: {startChunk.Key}");
                using (var startChunkStream = await _awsClient.DownloadLevel2ChunkAsync(startChunk))
                {
                    Logger.LogInfo($"Downloaded start chunk, stream length: {startChunkStream.Length} bytes");
                    
                    // Create a radar data object to hold the decoded data
                    var radarData = new RadarData
                    {
                        StationId = stationId,
                        ProductType = productType,
                        Timestamp = startChunk.LastModified.ToUniversalTime(),
                        Gates = new List<RadarGate>()
                    };
                    
                    Logger.LogInfo($"About to decode start chunk");
                    
                    // Decode the start chunk
                    DecodeChunk(startChunkStream, radarData, productType, tiltIndex, true);
                    
                    Logger.LogInfo($"Start chunk decoded, gates so far: {radarData.Gates.Count}");
                    
                    // Download and process other chunks in the volume
                    foreach (var chunk in volumeChunks.Where(c => c.Key != startChunk.Key))
                    {
                        Logger.LogInfo($"Processing additional chunk: {chunk.Key}");
                        using (var chunkStream = await _awsClient.DownloadLevel2ChunkAsync(chunk))
                        {
                            Logger.LogInfo($"Downloaded chunk {chunk.Key}, stream length: {chunkStream.Length} bytes");
                            
                            // Decode additional chunks
                            DecodeChunk(chunkStream, radarData, productType, tiltIndex, false);
                            
                            Logger.LogInfo($"Chunk decoded, total gates: {radarData.Gates.Count}");
                        }
                    }
                    
                    Logger.LogInfo($"=== LEVEL2DECODER SUCCESS ===");
                    Logger.LogInfo($"Final result: {radarData.Gates.Count} gates for {stationId}");
                    Log($"Decoded {radarData.Gates.Count} gates for {stationId}");
                    return radarData;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Level2Decoder.DecodeLatestScan");
                Logger.LogError($"=== LEVEL2DECODER FAILED ===");
                Logger.LogError($"Exception: {ex.GetType().Name} - {ex.Message}");
                if (ex.InnerException != null)
                {
                    Logger.LogError($"Inner Exception: {ex.InnerException.Message}");
                }
                Logger.LogError($"Stack Trace: {ex.StackTrace}");
                
                Log($"Error decoding Level 2 data: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Extract the volume number from a chunk key
        /// </summary>
        private string ExtractVolumeNumber(string key)
        {
            // Format: <Site>/<Volume_Number>/<YYYYMMDD-HHMMSS-CHUNKNUM-CHUNKTYPE>
            var parts = key.Split('/');
            if (parts.Length >= 2)
            {
                return parts[1]; // Second segment is the volume number
            }
            return string.Empty;
        }
        
        /// <summary>
        /// Decodes a NEXRAD Level 2 chunk
        /// </summary>
        private void DecodeChunk(MemoryStream chunkStream, RadarData radarData, Models.RadarProductType productType, int tiltIndex, bool isStartChunk)
        {
            Logger.LogInfo($"DecodeChunk: isStartChunk={isStartChunk}, streamLength={chunkStream.Length}, productType={productType}");
            
            // Read the Volume Header Record if this is the start chunk
            double stationLat = 0;
            double stationLon = 0;
            double stationAlt = 0;
            
            if (isStartChunk)
            {
                var vhr = new byte[VHR_SIZE];
                chunkStream.Read(vhr, 0, VHR_SIZE);
                
                // Parse VHR for metadata (ICAO, date, etc.) if needed
                string icao = System.Text.Encoding.ASCII.GetString(vhr, 20, 4); // Extract ICAO
                Logger.LogInfo($"VHR parsed: ICAO={icao}");
                Log($"VHR ICAO: {icao}");
            }
            
            int blockCount = 0;
            
            // Process compressed blocks in the chunk
            while (chunkStream.Position < chunkStream.Length)
            {
                blockCount++;
                Logger.LogInfo($"Processing block #{blockCount} at position {chunkStream.Position}");
                
                // Read the LDM Control Word (4 bytes)
                if (chunkStream.Position + 4 > chunkStream.Length)
                {
                    Logger.LogWarning($"Not enough data for control word at position {chunkStream.Position}");
                    break; // Not enough data for control word
                }
                
                byte[] controlWordBytes = new byte[4];
                chunkStream.Read(controlWordBytes, 0, 4);
                
                // Convert to big-endian int32
                Array.Reverse(controlWordBytes); // Convert to little-endian for BitConverter
                int compressedBlockSize = Math.Abs(BitConverter.ToInt32(controlWordBytes, 0));
                
                Logger.LogInfo($"Block #{blockCount}: compressed size = {compressedBlockSize} bytes");
                
                // Safety check for block size
                if (compressedBlockSize <= 0 || compressedBlockSize > 100000000 || 
                    chunkStream.Position + compressedBlockSize > chunkStream.Length)
                {
                    Logger.LogError($"Invalid compressed block size: {compressedBlockSize} at position {chunkStream.Position}");
                    Log($"Invalid compressed block size: {compressedBlockSize}");
                    break;
                }
                
                // Read the compressed block
                byte[] compressedBlock = new byte[compressedBlockSize];
                chunkStream.Read(compressedBlock, 0, compressedBlockSize);
                
                Logger.LogInfo($"Read {compressedBlock.Length} bytes of compressed data");
                
                // Decompress the block
                try
                {
                    using (var compressedStream = new MemoryStream(compressedBlock))
                    using (var decompressor = new BZip2Stream(compressedStream, CompressionMode.Decompress, false))
                    using (var decompressedStream = new MemoryStream())
                    {
                        // Copy decompressed data to a memory stream
                        decompressor.CopyTo(decompressedStream);
                        decompressedStream.Position = 0;
                        
                        Logger.LogInfo($"Decompressed to {decompressedStream.Length} bytes");
                        
                        // Process messages in the decompressed block
                        ProcessMessages(decompressedStream, radarData, productType, tiltIndex, ref stationLat, ref stationLon, ref stationAlt);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, $"Error decompressing block #{blockCount}");
                }
            }
            
            Logger.LogInfo($"DecodeChunk complete: processed {blockCount} blocks, gates={radarData.Gates.Count}");
        }
        
        /// <summary>
        /// Process messages in a decompressed block
        /// </summary>
        private void ProcessMessages(MemoryStream messageStream, RadarData radarData, Models.RadarProductType productType, 
                                   int tiltIndex, ref double stationLat, ref double stationLon, ref double stationAlt)
        {
            Logger.LogInfo($"ProcessMessages: streamLength={messageStream.Length}, productType={productType}, tiltIndex={tiltIndex}");
            
            int messageCount = 0;
            
            // Process each message in the decompressed stream
            while (messageStream.Position < messageStream.Length)
            {
                messageCount++;
                long startPos = messageStream.Position;
                
                Logger.LogInfo($"Processing message #{messageCount} at position {startPos}");
                
                // Skip CTM header (12 bytes)
                messageStream.Position += CTM_SIZE;
                
                // Check if we have enough data left for a message header
                if (messageStream.Position + MESSAGE_HEADER_SIZE > messageStream.Length)
                {
                    Logger.LogWarning($"Not enough data for message header at position {messageStream.Position}");
                    break;
                }
                
                // Read the message header
                byte[] headerBytes = new byte[MESSAGE_HEADER_SIZE];
                messageStream.Read(headerBytes, 0, MESSAGE_HEADER_SIZE);
                
                // Extract message type and size
                int messageSize = (headerBytes[0] << 8) | headerBytes[1]; // Big endian
                byte messageType = headerBytes[3];
                
                Logger.LogInfo($"Message #{messageCount}: type={messageType}, size={messageSize} half-words");
                
                // Calculate total message size in bytes (half-words to bytes)
                int messageSizeBytes = messageSize * 2;
                
                // Check if this is a Message Type 31 (radial data)
                if (messageType == MSG_TYPE_31)
                {
                    Logger.LogInfo($"Found Message Type 31 (radial data), processing...");
                    
                    // Set stream position back to the start of this message + CTM header
                    messageStream.Position = startPos + CTM_SIZE;
                    
                    // Process Message Type 31
                    ProcessMessage31(messageStream, messageSizeBytes, radarData, productType, tiltIndex, ref stationLat, ref stationLon, ref stationAlt);
                }
                else
                {
                    Logger.LogInfo($"Skipping message type {messageType}");
                    // Skip over this message
                    messageStream.Position = startPos + messageSizeBytes;
                }
            }
            
            Logger.LogInfo($"ProcessMessages complete: processed {messageCount} messages");
        }
        
        /// <summary>
        /// Process a Message Type 31 (Digital Radar Data)
        /// </summary>
        private void ProcessMessage31(MemoryStream stream, int messageSize, RadarData radarData, 
                                   Models.RadarProductType productType, int tiltIndex, 
                                   ref double stationLat, ref double stationLon, ref double stationAlt)
        {
            try
            {
                Logger.LogInfo($"ProcessMessage31: messageSize={messageSize}, productType={productType}, tiltIndex={tiltIndex}");
                
                // Skip standard header (already read)
                stream.Position += MESSAGE_HEADER_SIZE;
                
                // Read the Data Header Block specific to this radial
                byte[] dataHeaderBytes = new byte[52]; // Size of data header block
                int bytesRead = stream.Read(dataHeaderBytes, 0, dataHeaderBytes.Length);
                
                if (bytesRead < dataHeaderBytes.Length)
                {
                    Logger.LogWarning($"Incomplete data header (got {bytesRead}, expected {dataHeaderBytes.Length})");
                    Log($"Incomplete data header (got {bytesRead}, expected {dataHeaderBytes.Length})");
                    return;
                }
                
                // Extract key fields from the Data Header Block
                string radarId = System.Text.Encoding.ASCII.GetString(dataHeaderBytes, 0, 4); // ICAO
                int azimuthNumber = (dataHeaderBytes[10] << 8) | dataHeaderBytes[11]; // Azimuth number
                float azimuthAngle = BitConverter.ToSingle(dataHeaderBytes, 12); // Azimuth angle
                byte elevationNumber = dataHeaderBytes[26]; // Elevation number
                float elevationAngle = BitConverter.ToSingle(dataHeaderBytes, 28); // Elevation angle
                
                Logger.LogInfo($"Radial data: ICAO={radarId}, azimuth={azimuthAngle:F1}°, elevation={elevationNumber} ({elevationAngle:F1}°)");
                
                // Only process data for the requested tilt index
                if (tiltIndex != elevationNumber)
                {
                    Logger.LogInfo($"Skipping elevation {elevationNumber} (want {tiltIndex})");
                    return;
                }
                
                // Get the number of data blocks
                int dataBlockCount = (dataHeaderBytes[50] << 8) | dataHeaderBytes[51];
                Logger.LogInfo($"Processing {dataBlockCount} data blocks");
                
                // Iterate through the data blocks
                for (int blockIndex = 0; blockIndex < dataBlockCount; blockIndex++)
                {
                    // Read data block type
                    byte[] blockTypeBytes = new byte[1];
                    stream.Read(blockTypeBytes, 0, 1);
                    char blockType = (char)blockTypeBytes[0];
                    
                    // Read block name (3 bytes)
                    byte[] blockNameBytes = new byte[3];
                    stream.Read(blockNameBytes, 0, 3);
                    string blockName = System.Text.Encoding.ASCII.GetString(blockNameBytes);
                    
                    Logger.LogInfo($"Data block #{blockIndex + 1}: type='{blockType}', name='{blockName}'");
                    
                    // Check for the constant data block (contains station coordinates)
                    if (blockType == 'V' && blockName == "VOL")
                    {
                        Logger.LogInfo($"Found VOL constant data block");
                        byte[] volBlockBytes = new byte[31]; // Size of VOL constant block
                        stream.Read(volBlockBytes, 0, volBlockBytes.Length);
                        
                        // Extract station coordinates
                        stationLat = BitConverter.ToSingle(volBlockBytes, 4);
                        stationLon = BitConverter.ToSingle(volBlockBytes, 8);
                        stationAlt = BitConverter.ToInt16(volBlockBytes, 12);
                        
                        Logger.LogInfo($"Station coordinates: lat={stationLat}, lon={stationLon}, alt={stationAlt}");
                        
                        // Skip to the next block
                        continue;
                    }
                    
                    // Skip 4 reserved bytes
                    stream.Position += 4;
                    
                    // Check if this is the moment data block we're interested in
                    bool isRequestedMoment = false;
                    switch (productType)
                    {
                        case RadarProductType.BaseReflectivity:
                            isRequestedMoment = blockName == "REF";
                            break;
                        case RadarProductType.BaseVelocity:
                            isRequestedMoment = blockName == "VEL";
                            break;
                        case RadarProductType.SpectrumWidth:
                            isRequestedMoment = blockName == "SW ";
                            break;
                        case RadarProductType.CorrelationCoefficient:
                            isRequestedMoment = blockName == "RHO";
                            break;
                        case RadarProductType.DifferentialReflectivity:
                            isRequestedMoment = blockName == "ZDR";
                            break;
                        case RadarProductType.SpecificDifferentialPhase:
                            isRequestedMoment = blockName == "PHI" || blockName == "KDP";
                            break;
                        default:
                            isRequestedMoment = false;
                            break;
                    }
                    
                    Logger.LogInfo($"Block '{blockName}' is requested moment: {isRequestedMoment}");
                    
                    // Read the moment data block header
                    byte[] momentHeaderBytes = new byte[16];
                    stream.Read(momentHeaderBytes, 0, momentHeaderBytes.Length);
                    
                    // Extract the gate information
                    int gateCount = (momentHeaderBytes[0] << 8) | momentHeaderBytes[1];
                    int rangeToFirstGate = (momentHeaderBytes[2] << 8) | momentHeaderBytes[3];
                    int gateSpacing = (momentHeaderBytes[4] << 8) | momentHeaderBytes[5];
                    byte dataSize = momentHeaderBytes[9];
                    
                    // Read scaling parameters
                    float scale = BitConverter.ToSingle(momentHeaderBytes, 10);
                    float offset = BitConverter.ToSingle(momentHeaderBytes, 14);
                    
                    Logger.LogInfo($"Moment data: {gateCount} gates, range={rangeToFirstGate}m, spacing={gateSpacing}m, dataSize={dataSize}, scale={scale}, offset={offset}");
                    
                    // Process the gate data if this is the moment we're interested in
                    if (isRequestedMoment)
                    {
                        Logger.LogInfo($"Processing {gateCount} gates for requested moment '{blockName}'");
                        
                        // Calculate range to first gate in kilometers
                        float rangeToFirstGateKm = rangeToFirstGate / 1000.0f;
                        
                        // Calculate gate spacing in kilometers
                        float gateSpacingKm = gateSpacing / 1000.0f;
                        
                        // Read the gate values
                        byte[] gateValues = new byte[gateCount * (dataSize / 8)];
                        stream.Read(gateValues, 0, gateValues.Length);
                        
                        int validGates = 0;
                        
                        // Process each gate
                        for (int gateIndex = 0; gateIndex < gateCount; gateIndex++)
                        {
                            // Extract gate value
                            int gateValue = 0;
                            if (dataSize == 8)
                            {
                                gateValue = gateValues[gateIndex];
                            }
                            else if (dataSize == 16)
                            {
                                gateValue = (gateValues[gateIndex * 2] << 8) | gateValues[gateIndex * 2 + 1];
                            }
                            
                            // Skip special values (0 = below threshold, 1 = range folded)
                            if (gateValue <= 1)
                            {
                                continue;
                            }
                            
                            // Convert to actual value using the scale and offset
                            float value = (gateValue + offset) / scale;
                            
                            // Calculate range to this gate
                            float rangeKm = rangeToFirstGateKm + (gateIndex * gateSpacingKm);
                            
                            // Convert radar-centric polar coordinates to lat/lon
                            var (lat, lon) = PolarToLatLon(stationLat, stationLon, rangeKm, azimuthAngle);
                            
                            // Create a radar gate
                            var gate = new RadarGate
                            {
                                Latitude = lat,
                                Longitude = lon,
                                Value = value,
                                Azimuth = azimuthAngle,
                                Range = rangeKm,
                                Elevation = elevationAngle
                            };
                            
                            // Add the gate to the radar data
                            radarData.Gates.Add(gate);
                            validGates++;
                        }
                        
                        Logger.LogInfo($"Added {validGates} valid gates from moment '{blockName}'");
                    }
                    else
                    {
                        // Skip this moment's data
                        int bytesToSkip = gateCount * (dataSize / 8);
                        Logger.LogInfo($"Skipping {bytesToSkip} bytes for moment '{blockName}'");
                        stream.Position += bytesToSkip;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "ProcessMessage31");
                Log($"Error processing Message 31: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Convert polar coordinates (range, azimuth) to lat/lon
        /// </summary>
        private (double lat, double lon) PolarToLatLon(double stationLat, double stationLon, double rangeKm, double azimuthDeg)
        {
            const double DegToRad = Math.PI / 180.0;
            const double RadToDeg = 180.0 / Math.PI;
            const double EarthRadiusKm = 6371.0;
            const double EffectiveEarthRadiusFactor = 4.0 / 3.0; // 4/3 Earth radius model for refraction
            
            // Effective Earth radius in kilometers
            double effectiveEarthRadiusKm = EarthRadiusKm * EffectiveEarthRadiusFactor;
            
            // Convert to radians
            double latRad = stationLat * DegToRad;
            double lonRad = stationLon * DegToRad;
            double azRad = azimuthDeg * DegToRad;
            
            // Convert range from km to radians
            double rangeRad = rangeKm / effectiveEarthRadiusKm;
            
            // Calculate new position (great circle navigation formula)
            double newLatRad = Math.Asin(Math.Sin(latRad) * Math.Cos(rangeRad) + 
                                         Math.Cos(latRad) * Math.Sin(rangeRad) * Math.Cos(azRad));
                                         
            double newLonRad = lonRad + Math.Atan2(Math.Sin(azRad) * Math.Sin(rangeRad) * Math.Cos(latRad),
                                                 Math.Cos(rangeRad) - Math.Sin(latRad) * Math.Sin(newLatRad));
            
            // Convert back to degrees
            return (newLatRad * RadToDeg, newLonRad * RadToDeg);
        }
        
        /// <summary>
        /// Logs a message using the logger
        /// </summary>
        private void Log(string message)
        {
            _logger?.Invoke($"[Level2Decoder] {message}");
        }
    }
}
