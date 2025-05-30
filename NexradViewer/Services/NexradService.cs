using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using NexradViewer.Models;
using System.Drawing;
using NexradViewer.Controls;
using NexradViewer.Utils;

namespace NexradViewer.Services
{
    /// <summary>
    /// Service for accessing and processing NEXRAD radar data
    /// </summary>
    public class NexradService
    {
        // Dependencies
        private readonly AwsNexradClient _awsClient;
        private readonly Level2Decoder _level2Decoder;
        private readonly RadarDataProcessor _dataProcessor;
        private readonly Action<string> _logger;
        private readonly WarningsHelper _warningsHelper;
        private readonly ConfigurationService _config;

        // Stations cache
        private List<RadarStation> _stations = new List<RadarStation>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NexradService"/> class
        /// </summary>
        public NexradService(Action<string> logger = null, ConfigurationService config = null)
        {
            _logger = logger ?? (msg => { });
            _config = config ?? new ConfigurationService();
            
            Logger.LogInfo("Initializing NexradService");
            Logger.LogInfo($"Configuration loaded: MaxStationsForComposite={_config.MaxStationsForComposite}");
            
            _awsClient = new AwsNexradClient(_config);
            _dataProcessor = new RadarDataProcessor(this, _logger);
            _level2Decoder = new Level2Decoder(_awsClient, _logger);
            _warningsHelper = new WarningsHelper(_logger);
            
            LoadStations();
            Logger.LogInfo("NexradService initialization completed");
        }

        /// <summary>
        /// Gets all radar stations
        /// </summary>
        public List<RadarStation> GetStations()
        {
            return _stations;
        }

        /// <summary>
        /// Gets a radar station by ID
        /// </summary>
        public RadarStation GetStationById(string stationId)
        {
            return _stations.FirstOrDefault(s => s.Id == stationId);
        }

        /// <summary>
        /// Gets the latest radar scan for a station and product
        /// </summary>
        public async Task<RadarData> GetLatestScanAsync(string stationId, RadarProductType productType, int tiltIndex = 0)
        {
            string productName = productType.ToString();
            Logger.LogInfo($"=== GetLatestScanAsync START ===");
            Logger.LogInfo($"Requested: StationId={stationId}, Product={productName}, TiltIndex={tiltIndex}");
            _logger($"Getting latest scan for {stationId}, product {productName}");
            
            try
            {
                var station = GetStationById(stationId);
                if (station == null)
                {
                    Logger.LogError($"Station {stationId} not found in station list");
                    Logger.LogInfo($"Available stations: {string.Join(", ", _stations.Select(s => s.Id))}");
                    _logger($"Station {stationId} not found");
                    return null;
                }
                
                Logger.LogInfo($"Station found: {station.Name} at {station.Latitude}, {station.Longitude}");
                
                // Try Level 2 decoder for base products
                bool isLevel2 = productType == RadarProductType.BaseReflectivity || 
                               productType == RadarProductType.BaseVelocity || 
                               productType == RadarProductType.SpectrumWidth;
                
                Logger.LogInfo($"Product type analysis: IsLevel2={isLevel2}");
                
                if (isLevel2)
                {
                    Logger.LogInfo($"Attempting Level 2 decoder for {productName}");
                    _logger($"Using Level 2 decoder for {productName}");
                    
                    try
                    {
                        var radarData = await _level2Decoder.DecodeLatestScan(stationId, productType, tiltIndex);
                        Logger.LogInfo($"Level 2 decoder result: {(radarData != null ? $"Success, Gates={radarData.Gates?.Count ?? 0}" : "NULL")}");
                        
                        if (radarData != null && radarData.Gates.Count > 0)
                        {
                            _logger($"Successfully decoded {radarData.Gates.Count} gates for {stationId} using Level 2 decoder");
                            Logger.LogInfo($"=== GetLatestScanAsync END (Level2 Success) ===");
                            return radarData;
                        }
                        
                        Logger.LogWarning("Level 2 decoder returned null or empty data");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException(ex, "Level2Decoder.DecodeLatestScan");
                    }
                }
                
                // Fallback to data processor
                Logger.LogInfo($"Attempting RadarDataProcessor for {stationId}, {productName}");
                _logger($"Falling back to data processor for {stationId}, {productName}");
                
                try
                {
                    var gates = await _dataProcessor.ProcessRadarData(stationId, productType);
                    Logger.LogInfo($"RadarDataProcessor result: {gates?.Count ?? 0} gates");
                    
                    if (gates.Count > 0)
                    {
                        var data = new RadarData
                        {
                            StationId = stationId,
                            ProductType = productType,
                            Timestamp = DateTime.UtcNow,
                            Gates = gates
                        };
                        
                        _logger($"Successfully processed {gates.Count} gates for {stationId} using data processor");
                        Logger.LogInfo($"=== GetLatestScanAsync END (DataProcessor Success) ===");
                        return data;
                    }
                    
                    Logger.LogWarning("RadarDataProcessor returned empty gate list");
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "RadarDataProcessor.ProcessRadarData");
                }
                
                // Generate synthetic data for testing
                Logger.LogInfo($"Attempting synthetic data generation for {stationId}, {productName}");
                _logger($"No data available, generating synthetic data for {stationId}, {productName}");
                
                try
                {
                    var syntheticData = GenerateSyntheticData(stationId, productType);
                    Logger.LogInfo($"Synthetic data result: {(syntheticData != null ? $"Success, Gates={syntheticData.Gates?.Count ?? 0}" : "NULL")}");
                    
                    if (syntheticData != null)
                    {
                        Logger.LogInfo($"=== GetLatestScanAsync END (Synthetic Success) ===");
                        return syntheticData;
                    }
                    else
                    {
                        Logger.LogError("GenerateSyntheticData returned null");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "GenerateSyntheticData");
                }
                
                Logger.LogError("All data sources failed - returning null");
                Logger.LogInfo($"=== GetLatestScanAsync END (All Failed) ===");
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "GetLatestScanAsync - Outer catch");
                _logger($"Error getting scan: {ex.Message}");
                
                try
                {
                    Logger.LogInfo("Attempting synthetic data as fallback from exception");
                    var fallbackData = GenerateSyntheticData(stationId, productType);
                    Logger.LogInfo($"Exception fallback result: {(fallbackData != null ? $"Success, Gates={fallbackData.Gates?.Count ?? 0}" : "NULL")}");
                    return fallbackData;
                }
                catch (Exception synEx)
                {
                    Logger.LogException(synEx, "GenerateSyntheticData - Exception fallback");
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets a nationwide composite radar image by combining data from multiple radar stations
        /// </summary>
        public async Task<RadarData> GetNationwideCompositeAsync(RadarProductType productType = RadarProductType.BaseReflectivity)
        {
            string productName = productType.ToString();
            _logger($"Getting nationwide composite for {productName}");
            
            try
            {
                var activeStations = await _awsClient.GetAllStations();
                _logger($"Found {activeStations.Count} active stations");
                
                if (activeStations.Count == 0)
                {
                    activeStations = _stations.Select(s => s.Id).ToList();
                }
                
                var compositeData = new RadarData
                {
                    ProductType = productType,
                    Timestamp = DateTime.UtcNow,
                    Gates = new List<RadarGate>(),
                    Mode = "COMPOSITE"
                };
                
                int configMaxStations = _config?.MaxStationsForComposite ?? 160;
                int maxStations = Math.Min(activeStations.Count, configMaxStations);
                _logger($"Fetching data for {maxStations} stations for composite");
                
                var coverageTracker = new Dictionary<(int latBucket, int lonBucket), List<RadarGate>>();
                var tasks = new List<Task<RadarData>>();
                
                for (int i = 0; i < maxStations; i++)
                {
                    var stationId = activeStations[i];
                    tasks.Add(GetLatestScanAsync(stationId, productType));
                }
                
                await Task.WhenAll(tasks);
                
                foreach (var task in tasks)
                {
                    var stationData = task.Result;
                    
                    if (stationData != null && stationData.Gates.Count > 0)
                    {
                        _logger($"Processing {stationData.Gates.Count} gates from station {stationData.StationId}");
                        
                        foreach (var gate in stationData.Gates)
                        {
                            int latBucket = (int)(gate.Latitude * 50);
                            int lonBucket = (int)(gate.Longitude * 50);
                            var bucketKey = (latBucket, lonBucket);
                            
                            if (!coverageTracker.TryGetValue(bucketKey, out var bucketGates))
                            {
                                bucketGates = new List<RadarGate>();
                                coverageTracker[bucketKey] = bucketGates;
                            }
                            
                            bucketGates.Add(gate);
                        }
                    }
                }
                
                // Blend overlapping data
                foreach (var bucket in coverageTracker)
                {
                    if (bucket.Value.Count == 1)
                    {
                        compositeData.Gates.Add(bucket.Value[0]);
                    }
                    else if (bucket.Value.Count > 1)
                    {
                        var blendedGate = BlendGates(bucket.Value, productType);
                        compositeData.Gates.Add(blendedGate);
                    }
                }
                
                _logger($"Created composite with {compositeData.Gates.Count} gates");
                return compositeData;
            }
            catch (Exception ex)
            {
                _logger($"Error creating nationwide composite: {ex.Message}");
                return new RadarData
                {
                    ProductType = productType,
                    Timestamp = DateTime.UtcNow,
                    Gates = new List<RadarGate>(),
                    Mode = "ERROR"
                };
            }
        }

        /// <summary>
        /// Gets active weather warnings from the NWS API
        /// </summary>
        public async Task<List<WeatherWarning>> GetActiveWarningsAsync()
        {
            _logger("Getting active warnings");
            
            try
            {
                var warnings = await _warningsHelper.GetActiveWarningsAsync();
                _logger($"Got {warnings.Count} active warnings");
                return warnings;
            }
            catch (Exception ex)
            {
                _logger($"Error getting warnings: {ex.Message}");
                return new List<WeatherWarning>();
            }
        }

        /// <summary>
        /// Get color for a radar value based on the product
        /// </summary>
        public Color GetProductColor(double value, RadarProductType productType)
        {
            switch (productType)
            {
                case RadarProductType.BaseReflectivity:
                case RadarProductType.CompositeReflectivity:
                case RadarProductType.SuperResolutionBaseReflectivity:
                    return GetReflectivityColor(value);
                
                case RadarProductType.BaseVelocity:
                case RadarProductType.StormRelativeMeanVelocity:
                    return GetVelocityColor(value);
                
                default:
                    return GetDefaultColor(value);
            }
        }

        /// <summary>
        /// Creates radar features from the radar data
        /// </summary>
        public FeatureCollection CreateRadarFeatures(RadarData radarData, bool isMainMap)
        {
            if (radarData == null || radarData.Gates == null || radarData.Gates.Count == 0)
            {
                return new FeatureCollection();
            }
            
            var featureCollection = new FeatureCollection();
            foreach (var gate in radarData.Gates)
            {
                var feature = new PointFeature(gate.Latitude, gate.Longitude);
                feature.Properties.Add("Value", gate.Value);
                feature.Properties.Add("ProductType", radarData.ProductType);
                feature.Properties.Add("StationId", radarData.StationId);
                feature.Properties.Add("Color", GetProductColor(gate.Value, radarData.ProductType));
                
                featureCollection.Add(feature);
            }
            
            _logger($"Created {featureCollection.Features.Count} radar features");
            return featureCollection;
        }
        
        /// <summary>
        /// Creates a feature for a radar station
        /// </summary>
        public PointFeature CreateStationFeature(RadarStation station)
        {
            if (station == null)
            {
                return null;
            }
            
            var feature = new PointFeature(station.Latitude, station.Longitude);
            feature.Properties.Add("Id", station.Id);
            feature.Properties.Add("Name", station.Name);
            feature.Properties.Add("Type", "Station");
            feature.Properties.Add("Color", Color.FromArgb(255, 0, 0));
            
            return feature;
        }
        
        /// <summary>
        /// Creates features for weather warnings
        /// </summary>
        public FeatureCollection CreateWarningFeatures(List<WeatherWarning> warnings)
        {
            if (warnings == null || warnings.Count == 0)
            {
                return new FeatureCollection();
            }
            
            var featureCollection = new FeatureCollection();
            foreach (var warning in warnings)
            {
                if (warning.Coordinates != null && warning.Coordinates.Count > 0)
                {
                    foreach (var coordinate in warning.Coordinates)
                    {
                        var feature = new PointFeature(coordinate.Item1, coordinate.Item2);
                        feature.Properties.Add("Type", warning.Type.ToString());
                        feature.Properties.Add("Title", warning.Title);
                        feature.Properties.Add("Description", warning.Description);
                        feature.Properties.Add("Color", GetWarningColor(warning.Type));
                        
                        featureCollection.Add(feature);
                    }
                }
            }
            
            _logger($"Created {featureCollection.Features.Count} warning features");
            return featureCollection;
        }

        #region Private Methods

        /// <summary>
        /// Loads radar stations from the embedded JSON file
        /// </summary>
        private void LoadStations()
        {
            try
            {
                string filePath = Path.Combine("Data", "stations.json");
                Logger.LogInfo($"Loading stations from: {filePath}");
                Logger.LogInfo($"File exists: {File.Exists(filePath)}");
                
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    Logger.LogInfo($"JSON file size: {json.Length} characters");
                    
                    // Configure JsonSerializer with case-insensitive property matching
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    
                    _stations = JsonSerializer.Deserialize<List<RadarStation>>(json, options);
                    Logger.LogInfo($"Deserialized {_stations?.Count ?? 0} stations from JSON");
                    
                    // Log the first few stations for verification
                    if (_stations != null && _stations.Count > 0)
                    {
                        Logger.LogInfo($"First station: Id={_stations[0].Id}, Name={_stations[0].Name}, Lat={_stations[0].Latitude}, Lon={_stations[0].Longitude}");
                        
                        // Check if KFWS exists
                        var kfws = _stations.FirstOrDefault(s => s.Id == "KFWS");
                        if (kfws != null)
                        {
                            Logger.LogInfo($"KFWS found: {kfws.Name} at {kfws.Latitude}, {kfws.Longitude}");
                        }
                        else
                        {
                            Logger.LogWarning("KFWS not found in loaded stations");
                        }
                    }
                    
                    _logger($"Loaded {_stations.Count} stations from JSON file");
                }
                else
                {
                    Logger.LogWarning($"Stations file not found at {filePath}, using hardcoded stations");
                    _stations = GetHardcodedStations();
                    _logger("Using hardcoded stations");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "LoadStations");
                _logger($"Error loading radar stations: {ex.Message}");
                _stations = GetHardcodedStations();
            }
        }

        /// <summary>
        /// Gets a list of hardcoded stations
        /// </summary>
        private List<RadarStation> GetHardcodedStations()
        {
            return new List<RadarStation>
            {
                new RadarStation
                {
                    Id = "KFWS",
                    Name = "Dallas/Fort Worth",
                    State = "TX",
                    Country = "US",
                    Latitude = 32.5731,
                    Longitude = -97.3031,
                    Elevation = 208,
                    Type = "NEXRAD WSR-88D"
                },
                new RadarStation
                {
                    Id = "KOHX",
                    Name = "Nashville",
                    State = "TN",
                    Country = "US",
                    Latitude = 36.2472,
                    Longitude = -86.5625,
                    Elevation = 192,
                    Type = "NEXRAD WSR-88D"
                },
                new RadarStation
                {
                    Id = "KDIX",
                    Name = "Philadelphia",
                    State = "PA",
                    Country = "US",
                    Latitude = 39.9472,
                    Longitude = -74.4108,
                    Elevation = 149,
                    Type = "NEXRAD WSR-88D"
                },
                new RadarStation
                {
                    Id = "KLSX",
                    Name = "St. Louis",
                    State = "MO",
                    Country = "US",
                    Latitude = 38.6989,
                    Longitude = -90.6828,
                    Elevation = 185,
                    Type = "NEXRAD WSR-88D"
                },
                new RadarStation
                {
                    Id = "KMUX",
                    Name = "San Francisco",
                    State = "CA",
                    Country = "US",
                    Latitude = 37.1550,
                    Longitude = -121.8983,
                    Elevation = 1057,
                    Type = "NEXRAD WSR-88D"
                },
                new RadarStation
                {
                    Id = "KTLX",
                    Name = "Oklahoma City",
                    State = "OK",
                    Country = "US", 
                    Latitude = 35.3331,
                    Longitude = -97.2778,
                    Elevation = 370,
                    Type = "NEXRAD WSR-88D"
                }
            };
        }

        /// <summary>
        /// Generate synthetic radar data for testing
        /// </summary>
        private RadarData GenerateSyntheticData(string stationId, RadarProductType productType)
        {
            var station = GetStationById(stationId);
            if (station == null)
                return null;

            var data = new RadarData
            {
                StationId = stationId,
                ProductType = productType,
                Timestamp = DateTime.UtcNow,
                Gates = new List<RadarGate>(),
                Mode = "SYNTHETIC"
            };
            
            Random random = new Random();
            for (int i = 0; i < 360; i += 2)
            {
                for (int distance = 10; distance < 150; distance += 5)
                {
                    if (random.NextDouble() < 0.30)
                        continue;

                    double azimuth = i;
                    double value = GenerateValueForProduct(productType, random);
                    
                    double lat, lon;
                    (lat, lon) = GetLatLonFromRangeAzimuth(station.Latitude, station.Longitude, distance, azimuth);
                    
                    var gate = new RadarGate
                    {
                        Azimuth = azimuth,
                        Range = distance,
                        Value = value,
                        Latitude = lat,
                        Longitude = lon,
                        Elevation = 0.5
                    };
                    
                    if ((distance < 50 && random.NextDouble() < 0.60) || random.NextDouble() < 0.40)
                    {
                        data.Gates.Add(gate);
                    }
                }
            }
            
            _logger($"Generated {data.Gates.Count} synthetic gates for {stationId}, {productType}");
            return data;
        }

        private double GenerateValueForProduct(RadarProductType productType, Random random)
        {
            switch(productType)
            {
                case RadarProductType.BaseReflectivity:
                case RadarProductType.CompositeReflectivity:
                case RadarProductType.SuperResolutionBaseReflectivity:
                case RadarProductType.LowLayerComposite:
                    return random.NextDouble() * 70.0 - 10.0; // -10 to 60 dBZ
                
                case RadarProductType.BaseVelocity:
                case RadarProductType.StormRelativeMeanVelocity:
                case RadarProductType.VADWindProfile:
                    return random.NextDouble() * 120.0 - 60.0; // -60 to 60 m/s
                
                case RadarProductType.SpectrumWidth:
                    return random.NextDouble() * 10.0; // 0 to 10 m/s
                
                case RadarProductType.CorrelationCoefficient:
                    return 0.7 + random.NextDouble() * 0.3; // 0.7 to 1.0
                
                case RadarProductType.DifferentialReflectivity:
                    return random.NextDouble() * 12.0 - 4.0; // -4 to 8 dB
                
                case RadarProductType.SpecificDifferentialPhase:
                    return random.NextDouble() * 180.0; // 0 to 180 degrees
                
                case RadarProductType.OneHourAccumulation:
                case RadarProductType.StormTotalPrecipitation:
                case RadarProductType.DigitalPrecipitationArray:
                    return random.NextDouble() * 5.0; // 0 to 5 inches
                
                case RadarProductType.EnhancedEchoTops:
                    return random.NextDouble() * 50000.0; // 0 to 50,000 feet
                
                case RadarProductType.MesocycloneDetection:
                case RadarProductType.TornadicVortexSignature:
                    return random.NextDouble() > 0.9 ? 1.0 : 0.0; // Binary
                
                case RadarProductType.HydrometeorClassification:
                    return Math.Floor(random.NextDouble() * 10.0); // 0-9 categories
                
                default:
                    return random.NextDouble() * 100.0; // Generic 0-100 scale
            }
        }

        /// <summary>
        /// Calculate lat/lon from radar-centric range and azimuth
        /// </summary>
        private (double lat, double lon) GetLatLonFromRangeAzimuth(double stationLat, double stationLon, double rangeKm, double azimuthDeg)
        {
            const double DegToRad = Math.PI / 180.0;
            const double RadToDeg = 180.0 / Math.PI;
            const double EarthRadiusKm = 6371.0;
            
            double latRad = stationLat * DegToRad;
            double lonRad = stationLon * DegToRad;
            double azRad = azimuthDeg * DegToRad;
            double rangeRad = rangeKm / EarthRadiusKm;
            
            double newLatRad = Math.Asin(Math.Sin(latRad) * Math.Cos(rangeRad) + 
                                         Math.Cos(latRad) * Math.Sin(rangeRad) * Math.Cos(azRad));
                                         
            double newLonRad = lonRad + Math.Atan2(Math.Sin(azRad) * Math.Sin(rangeRad) * Math.Cos(latRad),
                                                 Math.Cos(rangeRad) - Math.Sin(latRad) * Math.Sin(newLatRad));
            
            return (newLatRad * RadToDeg, newLonRad * RadToDeg);
        }

        private RadarGate BlendGates(List<RadarGate> gates, RadarProductType productType)
        {
            if (productType == RadarProductType.BaseReflectivity || 
                productType == RadarProductType.CompositeReflectivity ||
                productType == RadarProductType.SuperResolutionBaseReflectivity ||
                productType == RadarProductType.LowLayerComposite)
            {
                // For reflectivity, use maximum value
                var maxGate = gates.OrderByDescending(g => g.Value).First();
                return new RadarGate
                {
                    Latitude = gates.Average(g => g.Latitude),
                    Longitude = gates.Average(g => g.Longitude),
                    Value = maxGate.Value,
                    Azimuth = 0,
                    Range = 0,
                    Elevation = 0
                };
            }
            else
            {
                // For other products, use weighted average
                double totalWeight = 0;
                double weightedValue = 0;
                
                foreach (var gate in gates)
                {
                    double weight = 1.0 / (0.1 + gate.Range);
                    weightedValue += gate.Value * weight;
                    totalWeight += weight;
                }
                
                return new RadarGate
                {
                    Latitude = gates.Average(g => g.Latitude),
                    Longitude = gates.Average(g => g.Longitude),
                    Value = weightedValue / totalWeight,
                    Azimuth = 0,
                    Range = 0,
                    Elevation = 0
                };
            }
        }

        private Color GetReflectivityColor(double value)
        {
            if (value < 0) return Color.FromArgb(120, 120, 120); // Gray
            if (value < 10) return Color.FromArgb(0, 0, 235);    // Blue
            if (value < 20) return Color.FromArgb(0, 235, 235);  // Cyan
            if (value < 30) return Color.FromArgb(0, 235, 0);    // Green
            if (value < 40) return Color.FromArgb(235, 235, 0);  // Yellow
            if (value < 50) return Color.FromArgb(235, 120, 0);  // Orange
            if (value < 60) return Color.FromArgb(235, 0, 0);    // Red
            return Color.FromArgb(200, 0, 200);                  // Purple
        }

        private Color GetVelocityColor(double value)
        {
            if (value < -32) return Color.FromArgb(0, 0, 255);     // Blue (away)
            if (value < -16) return Color.FromArgb(0, 128, 255);   // Light blue (away)
            if (value < -8) return Color.FromArgb(0, 255, 128);    // Light green (away)
            if (value < -2) return Color.FromArgb(0, 200, 0);      // Green (away)
            if (value < 2) return Color.FromArgb(180, 180, 180);   // Gray (near-zero)
            if (value < 8) return Color.FromArgb(200, 200, 0);     // Yellow (toward)
            if (value < 16) return Color.FromArgb(255, 128, 0);    // Orange (toward)
            if (value < 32) return Color.FromArgb(255, 0, 0);      // Red (toward)
            return Color.FromArgb(200, 0, 200);                    // Purple (toward)
        }

        private Color GetDefaultColor(double value)
        {
            float normalizedValue = (float)((value - -32) / (70 - -32));
            normalizedValue = Math.Max(0, Math.Min(1, normalizedValue));
            int colorValue = (int)(255 * normalizedValue);
            return Color.FromArgb(colorValue, colorValue, colorValue);
        }

        private Color GetWarningColor(WarningType warningType)
        {
            switch (warningType)
            {
                case WarningType.TornadoWatch:
                    return Color.FromArgb(255, 255, 0); // Yellow
                case WarningType.Tornado:
                    return Color.FromArgb(255, 0, 0); // Red
                case WarningType.SevereThunderstormWatch:
                    return Color.FromArgb(0, 255, 255); // Cyan
                case WarningType.SevereThunderstorm:
                    return Color.FromArgb(255, 165, 0); // Orange
                case WarningType.Flood:
                    return Color.FromArgb(0, 255, 0); // Green
                case WarningType.FloodWarning:
                    return Color.FromArgb(0, 128, 0); // Dark Green
                case WarningType.FlashFlood:
                    return Color.FromArgb(139, 0, 0); // Dark Red
                case WarningType.FlashFloodWatch:
                    return Color.FromArgb(144, 238, 144); // Light Green
                default:
                    return Color.FromArgb(128, 128, 128); // Gray
            }
        }

        #endregion
    }
}
