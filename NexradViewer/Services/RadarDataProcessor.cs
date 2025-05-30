using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NexradViewer.Models;
using System.IO;
using SharpCompress.Compressors.BZip2;
using SharpCompress.Compressors;
using System.Text.Json;
using System.Net.Http;
using System.Linq;

namespace NexradViewer.Services
{
    /// <summary>
    /// Process radar data from various sources into a unified format
    /// </summary>
    public class RadarDataProcessor
    {
        private readonly NexradService _nexradService;
        private readonly Action<string> _logger;
        private readonly HttpClient _httpClient;
        
        // Cache directory
        private readonly string _cacheDir;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RadarDataProcessor"/> class
        /// </summary>
        public RadarDataProcessor(NexradService nexradService, Action<string> logger = null)
        {
            _nexradService = nexradService;
            _logger = logger ?? (msg => { });
            _httpClient = new HttpClient();
            
            // Set up cache directory
            _cacheDir = Path.Combine(Path.GetTempPath(), "NexradViewer", "Cache");
            if (!Directory.Exists(_cacheDir))
            {
                Directory.CreateDirectory(_cacheDir);
            }
        }
        
        /// <summary>
        /// Process radar data for a station and product
        /// </summary>
        public async Task<List<RadarGate>> ProcessRadarData(string stationId, RadarProductType productType)
        {
            Log($"Processing radar data for {stationId}, product {productType}");
            
            try
            {
                // Get product data from type
                bool isLevel2 = MappingTypes.IsLevel2Product.TryGetValue(productType, out bool level2) && level2;
                
                // For Level 3 products, we'll process data differently than Level 2
                if (!isLevel2)
                {
                    return await ProcessLevel3Data(stationId, productType);
                }
                
                // If this is Level 2 dual-pol product, we need to use the Level 2 decoder
                string category = MappingTypes.ProductCategories.TryGetValue(productType, out string cat) ? cat : string.Empty;
                if (category == "Dual-Polarization")
                {
                    return new List<RadarGate>(); // This would be handled by Level2Decoder
                }
                
                // For Level 2 base products (reflectivity, velocity, spectrum width),
                // we could use either Level 2 decoder or process differently
                return await ProcessLevel2Data(stationId, productType);
            }
            catch (Exception ex)
            {
                Log($"Error processing radar data: {ex.Message}");
                return new List<RadarGate>();
            }
        }
        
        /// <summary>
        /// Process Level 2 radar data
        /// </summary>
        private async Task<List<RadarGate>> ProcessLevel2Data(string stationId, RadarProductType productType)
        {
            Log($"Processing Level 2 data for {stationId}, product {productType}");
            
            try
            {
                // Generate synthetic test data for now
                var gates = new List<RadarGate>();
                var random = new Random();
                
                // Get station info
                var station = _nexradService.GetStationById(stationId);
                if (station == null)
                {
                    Log($"Station {stationId} not found");
                    return gates;
                }
                
            // Value ranges dependent on product
            double minValue = 0;
            double maxValue = 100;
            switch (productType)
            {
                case RadarProductType.BaseReflectivity:
                    minValue = -10;
                    maxValue = 70;
                    break;
                case RadarProductType.BaseVelocity:
                    minValue = -60;
                    maxValue = 60;
                    break;
                case RadarProductType.SpectrumWidth:
                    minValue = 0;
                    maxValue = 15;
                    break;
                case RadarProductType.DifferentialReflectivity:
                    minValue = -8;
                    maxValue = 8;
                    break;
                case RadarProductType.CorrelationCoefficient:
                    minValue = 0;
                    maxValue = 1;
                    break;
                case RadarProductType.SpecificDifferentialPhase:
                    minValue = -2;
                    maxValue = 7;
                    break;
                case RadarProductType.HydrometeorClassification:
                    minValue = 0;
                    maxValue = 10;
                    break;
            }
                
                // Generate gates in a radial pattern
                for (int azimuth = 0; azimuth < 360; azimuth += 2)
                {
                    for (int range = 10; range < 150; range += 5)
                    {
                        // Skip some gates to create more realistic pattern
                        if (random.NextDouble() < 0.7)
                            continue;
                        
                        // Create synthetic value based on product type
                        double value = minValue + random.NextDouble() * (maxValue - minValue);
                        
                        // Add some "weather-like" patterns
                        if (azimuth > 45 && azimuth < 135 && range > 50 && range < 100)
                        {
                            value = minValue + (maxValue - minValue) * 0.7; // Higher values in a sector
                        }
                        
                        // Calculate lat/lon from station-relative coordinates
                        (double lat, double lon) = CalculateLatLon(
                            station.Latitude, station.Longitude, range, azimuth);
                        
                        gates.Add(new RadarGate
                        {
                            Azimuth = azimuth,
                            Range = range,
                            Value = value,
                            Latitude = lat,
                            Longitude = lon,
                            Elevation = 0.5 // Lowest elevation angle
                        });
                    }
                }
                
                Log($"Processed {gates.Count} gates for {stationId}, {productType}");
                return gates;
            }
            catch (Exception ex)
            {
                Log($"Error processing Level 2 data: {ex.Message}");
                return new List<RadarGate>();
            }
        }
        
        /// <summary>
        /// Process Level 3 radar data from a stream
        /// </summary>
        public async Task<RadarData> ProcessLevel3Data(Stream stream, string stationId, RadarProductType productType)
        {
            Log($"Processing Level 3 data from stream for {stationId}, product {productType}");
            
            try
            {
                // Process the stream containing Level 3 data
                // For now, just create a simple RadarData object with the synthetic gates
                var gates = await ProcessLevel3Data(stationId, productType);
                
                var radarData = new RadarData
                {
                    StationId = stationId,
                    ProductType = productType,
                    Timestamp = DateTime.UtcNow,
                    Gates = gates,
                    Mode = "LEVEL3"
                };
                
                Log($"Processed Level 3 data from stream with {gates.Count} gates");
                return radarData;
            }
            catch (Exception ex)
            {
                Log($"Error processing Level 3 data from stream: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Process Level 3 radar data
        /// </summary>
        private async Task<List<RadarGate>> ProcessLevel3Data(string stationId, RadarProductType productType)
        {
            Log($"Processing Level 3 data for {stationId}, product {productType}");
            
            try
            {
                // Generate more limited synthetic data for Level 3 products
                var gates = new List<RadarGate>();
                var random = new Random();
                
                // Get station info
                var station = _nexradService.GetStationById(stationId);
                if (station == null)
                {
                    Log($"Station {stationId} not found");
                    return gates;
                }
                
                // Level 3 products have fewer gates in general - generate simplified pattern
                int gateCount = 1000;
                
            // Value range based on product
            double minValue = 0;
            double maxValue = 100;
            
            switch (productType)
            {
                case RadarProductType.BaseReflectivity:
                case RadarProductType.CompositeReflectivity:
                case RadarProductType.SuperResolutionBaseReflectivity:
                case RadarProductType.LowLayerComposite:
                    minValue = -10;
                    maxValue = 70;
                    break;
                case RadarProductType.BaseVelocity:
                case RadarProductType.StormRelativeMeanVelocity:
                case RadarProductType.VADWindProfile:
                    minValue = -60;
                    maxValue = 60;
                    break;
                case RadarProductType.OneHourAccumulation:
                case RadarProductType.StormTotalPrecipitation:
                case RadarProductType.DigitalPrecipitationArray:
                    minValue = 0;
                    maxValue = 10; // Inches
                    break;
                case RadarProductType.EnhancedEchoTops:
                    minValue = 1000;
                    maxValue = 50000; // Feet
                    break;
                case RadarProductType.MesocycloneDetection:
                case RadarProductType.TornadicVortexSignature:
                    minValue = 0;
                    maxValue = 1; // Binary detection
                    break;
            }
                
                // Generate gates with pattern dependent on product
                for (int i = 0; i < gateCount; i++)
                {
                    // Random location around the station
                    double azimuth = random.NextDouble() * 360.0;
                    double range = random.NextDouble() * 200.0; // km
                    
                    // Add clustering for more realistic patterns
                    if (i % 10 == 0)
                    {
                        range = random.NextDouble() * 100.0; // More data closer to station
                    }
                    if (i % 5 == 0) 
                    {
                        azimuth = (azimuth + 5) % 360; // Clump azimuths
                    }
                    
                    // Calculate lat/lon
                    (double lat, double lon) = CalculateLatLon(
                        station.Latitude, station.Longitude, range, azimuth);
                    
            // Generate value appropriate to product
            double value;
            
            // For precipitation products, create more realistic patterns
            if (productType == RadarProductType.OneHourAccumulation || 
                productType == RadarProductType.StormTotalPrecipitation ||
                productType == RadarProductType.DigitalPrecipitationArray)
                    {
                        if (i % 100 < 70)
                        {
                            value = minValue + (maxValue - minValue) * 0.1; // Mostly light precipitation
                        }
                        else if (i % 100 < 90)
                        {
                            value = minValue + (maxValue - minValue) * 0.3; // Some moderate
                        }
                        else
                        {
                            value = minValue + (maxValue - minValue) * 0.7; // A little heavy
                        }
                    }
                    else
                    {
                        value = minValue + random.NextDouble() * (maxValue - minValue);
                    }
                    
                    gates.Add(new RadarGate
                    {
                        Azimuth = azimuth,
                        Range = range,
                        Value = value,
                        Latitude = lat,
                        Longitude = lon,
                        Elevation = 0.5
                    });
                }
                
                Log($"Processed {gates.Count} gates for Level 3 product {productType}");
                return gates;
            }
            catch (Exception ex)
            {
                Log($"Error processing Level 3 data: {ex.Message}");
                return new List<RadarGate>();
            }
        }
        
        
        /// <summary>
        /// Calculate lat/lon from station-relative polar coordinates
        /// </summary>
        private (double lat, double lon) CalculateLatLon(double stationLat, double stationLon, double rangeKm, double azimuthDeg)
        {
            const double DegToRad = Math.PI / 180.0;
            const double RadToDeg = 180.0 / Math.PI;
            const double EarthRadiusKm = 6371.0;
            
            // Convert to radians
            double latRad = stationLat * DegToRad;
            double lonRad = stationLon * DegToRad;
            double azRad = azimuthDeg * DegToRad;
            
            // Convert range from km to radians
            double rangeRad = rangeKm / EarthRadiusKm;
            
            // Calculate new position
            double newLatRad = Math.Asin(Math.Sin(latRad) * Math.Cos(rangeRad) + 
                                         Math.Cos(latRad) * Math.Sin(rangeRad) * Math.Cos(azRad));
                                         
            double newLonRad = lonRad + Math.Atan2(Math.Sin(azRad) * Math.Sin(rangeRad) * Math.Cos(latRad),
                                                 Math.Cos(rangeRad) - Math.Sin(latRad) * Math.Sin(newLatRad));
            
            // Convert back to degrees
            return (newLatRad * RadToDeg, newLonRad * RadToDeg);
        }
        
        /// <summary>
        /// Log a message
        /// </summary>
        private void Log(string message)
        {
            _logger?.Invoke($"[RadarDataProcessor] {message}");
        }
    }
}
