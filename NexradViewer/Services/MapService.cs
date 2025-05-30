using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using NexradViewer.Models;

namespace NexradViewer.Services
{
    /// <summary>
    /// Service for handling map-related functionality
    /// </summary>
    public class MapService
    {
        private readonly NexradService _nexradService;
        private readonly RadarDataProcessor _radarDataProcessor;
        private readonly Action<string> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public MapService(NexradService nexradService, RadarDataProcessor radarDataProcessor, Action<string> logger = null)
        {
            _nexradService = nexradService;
            _radarDataProcessor = radarDataProcessor;
            _logger = logger ?? (msg => { });
        }

        /// <summary>
        /// Get map features for displaying radar stations
        /// </summary>
        public List<StationFeature> GetStationFeatures()
        {
            var stationFeatures = new List<StationFeature>();
            var stations = _nexradService.GetStations();

            foreach (var station in stations)
            {
                stationFeatures.Add(new StationFeature(
                    station.Latitude, 
                    station.Longitude, 
                    station.Id, 
                    station.Name));
            }

            return stationFeatures;
        }

    /// <summary>
    /// Get map features for displaying radar data
    /// </summary>
    public async Task<List<RadarFeature>> GetRadarFeaturesAsync(string stationId, RadarProductType productType)
    {
        try
        {
            string productName = MappingTypes.ProductDisplayNames.TryGetValue(productType, out string name) ? name : productType.ToString();
            _logger($"Getting {productName} radar features for {stationId}");
            
            var gates = await _radarDataProcessor.ProcessRadarData(stationId, productType);
            var features = new List<RadarFeature>();

            foreach (var gate in gates)
            {
                var feature = new RadarFeature(
                    gate.Latitude,
                    gate.Longitude,
                    (float)gate.Value, 
                    productType);
                
                feature.Color = GetColorForValue((float)gate.Value, productType);
                features.Add(feature);
            }

            _logger($"Processed {features.Count} radar features");
            return features;
        }
        catch (Exception ex)
        {
            _logger($"Error getting radar features: {ex.Message}");
            return new List<RadarFeature>();
        }
    }

        /// <summary>
        /// Get color for a radar value based on product type
        /// </summary>
        public Color GetColorForValue(float value, RadarProductType productType)
        {
            // Zero or negative values (no data) are transparent
            if (value <= 0)
                return Color.Transparent;

            // Different color scales per product
            switch (productType)
            {
                case RadarProductType.BaseVelocity:
                case RadarProductType.StormRelativeMeanVelocity:
                case RadarProductType.VADWindProfile:
                    return GetVelocityColor(value);
                case RadarProductType.SpectrumWidth:
                    return GetSpectrumWidthColor(value);
                case RadarProductType.CorrelationCoefficient:
                    return GetCorrelationCoefficientColor(value);
                case RadarProductType.DifferentialReflectivity:
                    return GetDifferentialReflectivityColor(value);
                case RadarProductType.HydrometeorClassification:
                    return GetHydrometeorClassColor(value);
                case RadarProductType.OneHourAccumulation:
                case RadarProductType.StormTotalPrecipitation:
                case RadarProductType.DigitalPrecipitationArray:
                    return GetPrecipitationColor(value);
                case RadarProductType.EnhancedEchoTops:
                    return GetEchoTopsColor(value);
                case RadarProductType.SpecificDifferentialPhase:
                    return GetPhiDPColor(value);
                case RadarProductType.MesocycloneDetection:
                case RadarProductType.TornadicVortexSignature:
                    return GetStormRelativeMotionColor(value);
                default:
                    return GetReflectivityColor(value);
            }
        }

        /// <summary>
        /// Get color for reflectivity value (dBZ)
        /// </summary>
        private Color GetReflectivityColor(float dbz)
        {
            // Standard reflectivity color scale
            if (dbz < 5) return Color.FromArgb(70, 0, 0, 0); // Transparent gray for very light echoes
            if (dbz < 10) return Color.FromArgb(130, 4, 233, 231); // Light teal
            if (dbz < 15) return Color.FromArgb(150, 1, 159, 244); // Light blue
            if (dbz < 20) return Color.FromArgb(160, 3, 0, 244); // Blue
            if (dbz < 25) return Color.FromArgb(170, 2, 253, 2); // Green
            if (dbz < 30) return Color.FromArgb(180, 1, 197, 1); // Dark green
            if (dbz < 35) return Color.FromArgb(190, 0, 142, 0); // Darker green
            if (dbz < 40) return Color.FromArgb(200, 253, 248, 2); // Yellow
            if (dbz < 45) return Color.FromArgb(210, 229, 188, 0); // Gold
            if (dbz < 50) return Color.FromArgb(220, 253, 149, 0); // Orange
            if (dbz < 55) return Color.FromArgb(230, 253, 0, 0); // Red
            if (dbz < 60) return Color.FromArgb(240, 212, 0, 0); // Dark red
            if (dbz < 65) return Color.FromArgb(245, 188, 0, 0); // Darker red
            if (dbz < 70) return Color.FromArgb(250, 248, 0, 253); // Magenta
            if (dbz < 75) return Color.FromArgb(255, 163, 0, 163); // Purple
            
            return Color.FromArgb(255, 119, 0, 119); // Deep purple for extreme values
        }

        /// <summary>
        /// Get color for velocity value (m/s)
        /// </summary>
        private Color GetVelocityColor(float velocity)
        {
            // Standard velocity color scale
            // Negative values: green (moving toward the radar)
            // Positive values: red (moving away from the radar)
            // Brighter colors indicate faster speeds
            
            // Zero velocity
            if (Math.Abs(velocity) < 0.5) 
                return Color.FromArgb(130, 150, 150, 150); // Gray
                
            // Velocities away from the radar (positive)
            if (velocity > 0)
            {
                if (velocity < 5) return Color.FromArgb(140, 191, 120, 114);  // Light pink
                if (velocity < 10) return Color.FromArgb(160, 214, 97, 89);   // Light red
                if (velocity < 20) return Color.FromArgb(180, 237, 73, 63);   // Medium red
                if (velocity < 35) return Color.FromArgb(200, 255, 29, 17);   // Bright red
                if (velocity < 50) return Color.FromArgb(220, 224, 8, 7);     // Deep red
                if (velocity < 65) return Color.FromArgb(240, 186, 8, 6);     // Dark red
                return Color.FromArgb(255, 133, 4, 3);                        // Very dark red
            }
            // Velocities toward the radar (negative)
            else
            {
                float abs = Math.Abs(velocity);
                if (abs < 5) return Color.FromArgb(140, 120, 189, 128);       // Light teal
                if (abs < 10) return Color.FromArgb(160, 97, 211, 111);       // Light green
                if (abs < 20) return Color.FromArgb(180, 71, 237, 89);        // Medium green
                if (abs < 35) return Color.FromArgb(200, 28, 255, 53);        // Bright green
                if (abs < 50) return Color.FromArgb(220, 11, 224, 32);        // Deep green
                if (abs < 65) return Color.FromArgb(240, 8, 186, 24);         // Dark green
                return Color.FromArgb(255, 4, 133, 17);                       // Very dark green
            }
        }

        /// <summary>
        /// Get radar warning features for display on map
        /// </summary>
        public List<WarningFeature> GetWarningFeatures(IEnumerable<WeatherWarning> warnings)
        {
            var features = new List<WarningFeature>();
            
            foreach (var warning in warnings)
            {
                if (warning.Coordinates == null || warning.Coordinates.Count < 3)
                    continue;
                
                // Get first coordinate for the warning (center point)
                var firstCoord = warning.Coordinates[0];
                
                var feature = new WarningFeature(
                    firstCoord.Item1, 
                    firstCoord.Item2, 
                    warning.Type, 
                    warning.Title, 
                    warning.Description);
                
                // Store the color 
                var warningColor = GetWarningColor(warning.Type, warning.Severity);
                feature.Properties.Add("Color", warningColor);
                feature.Properties.Add("Severity", warning.Severity);
                
                // Convert all coordinates and add to properties
                var coords = ConvertCoordinates(warning.Coordinates);
                feature.Properties.Add("CoordinatesList", coords);
                
                features.Add(feature);
            }
            
            return features;
        }
        
        /// <summary>
        /// Get color for spectrum width value (m/s)
        /// </summary>
        private Color GetSpectrumWidthColor(float value)
        {
            // Spectrum width represents velocity spread/turbulence
            if (value < 1) return Color.FromArgb(150, 100, 100, 100); // Light gray
            if (value < 2) return Color.FromArgb(170, 150, 150, 150); // Gray
            if (value < 3) return Color.FromArgb(180, 96, 96, 175);   // Blue-gray
            if (value < 4) return Color.FromArgb(190, 65, 105, 225);  // Royal blue
            if (value < 5) return Color.FromArgb(200, 30, 144, 255);  // Dodger blue
            if (value < 6) return Color.FromArgb(210, 0, 255, 127);   // Spring green
            if (value < 8) return Color.FromArgb(220, 50, 205, 50);   // Lime green
            if (value < 10) return Color.FromArgb(230, 255, 215, 0);  // Gold
            if (value < 12) return Color.FromArgb(240, 255, 165, 0);  // Orange
            if (value < 15) return Color.FromArgb(250, 255, 0, 0);    // Red
            
            return Color.FromArgb(255, 139, 0, 0);                    // Dark red
        }
        
        /// <summary>
        /// Get color for correlation coefficient value
        /// </summary>
        private Color GetCorrelationCoefficientColor(float value)
        {
            // CC values range from 0 to 1
            // Lower values often indicate non-meteorological targets or mixed precipitation
            if (value < 0.5) return Color.FromArgb(200, 0, 0, 0);      // Black (non-meteorological)
            if (value < 0.7) return Color.FromArgb(200, 128, 0, 128);  // Purple (hail, debris)
            if (value < 0.8) return Color.FromArgb(200, 255, 0, 0);    // Red (mixed precip)
            if (value < 0.85) return Color.FromArgb(200, 255, 140, 0); // Orange (rain/snow mix)
            if (value < 0.9) return Color.FromArgb(200, 255, 255, 0);  // Yellow (light snow)
            if (value < 0.93) return Color.FromArgb(200, 0, 255, 0);   // Green (drizzle)
            if (value < 0.96) return Color.FromArgb(200, 0, 200, 255); // Cyan (light rain)
            if (value < 0.98) return Color.FromArgb(200, 0, 0, 255);   // Blue (rain)
            
            return Color.FromArgb(200, 255, 255, 255);                 // White (pure rain)
        }
        
        /// <summary>
        /// Get color for differential reflectivity value (dB)
        /// </summary>
        private Color GetDifferentialReflectivityColor(float value)
        {
            // ZDR represents the ratio of horizontal to vertical reflectivity
            // Negative: vertically oriented particles (ice crystals, hail)
            // Near zero: spherical particles (small drops, dry snow)
            // Positive: horizontally oriented particles (rain drops)
            if (value < -3) return Color.FromArgb(200, 148, 0, 211);     // Dark purple
            if (value < -1) return Color.FromArgb(200, 75, 0, 130);      // Indigo
            if (value < -0.5) return Color.FromArgb(200, 0, 0, 255);     // Blue
            if (value < 0) return Color.FromArgb(200, 0, 191, 255);      // Deep sky blue
            if (value < 0.5) return Color.FromArgb(200, 46, 139, 87);    // Sea green
            if (value < 1) return Color.FromArgb(200, 0, 255, 0);        // Green
            if (value < 2) return Color.FromArgb(200, 255, 255, 0);      // Yellow
            if (value < 3) return Color.FromArgb(200, 255, 165, 0);      // Orange
            if (value < 4) return Color.FromArgb(200, 255, 0, 0);        // Red
            if (value < 5) return Color.FromArgb(200, 139, 0, 0);        // Dark red
            
            return Color.FromArgb(200, 128, 0, 128);                     // Purple
        }
        
        /// <summary>
        /// Get color for hydrometeor classification
        /// </summary>
        private Color GetHydrometeorClassColor(float value)
        {
            // Standard hydrometeor class colors
            switch ((int)value)
            {
                case 0: return Color.FromArgb(0, 0, 0, 0);            // No data (transparent)
                case 1: return Color.FromArgb(200, 128, 128, 128);    // Biological (gray)
                case 2: return Color.FromArgb(200, 0, 0, 0);          // Ground clutter (black)
                case 3: return Color.FromArgb(200, 0, 252, 0);        // Ice crystals (green)
                case 4: return Color.FromArgb(200, 0, 200, 255);      // Dry snow (cyan)
                case 5: return Color.FromArgb(200, 0, 0, 255);        // Wet snow (blue)
                case 6: return Color.FromArgb(200, 255, 255, 0);      // Light rain (yellow)
                case 7: return Color.FromArgb(200, 255, 165, 0);      // Heavy rain (orange)
                case 8: return Color.FromArgb(200, 255, 0, 0);        // Big drops (red)
                case 9: return Color.FromArgb(200, 128, 0, 128);      // Graupel (purple)
                case 10: return Color.FromArgb(200, 255, 105, 180);   // Hail (pink)
                case 11: return Color.FromArgb(200, 188, 143, 143);   // Unknown (rosy brown)
                default: return Color.FromArgb(200, 255, 255, 255);   // Default (white)
            }
        }
        
        /// <summary>
        /// Get color for precipitation amount (inches)
        /// </summary>
        private Color GetPrecipitationColor(float inches)
        {
            // Standard precipitation color scale
            if (inches < 0.1) return Color.FromArgb(130, 173, 216, 230);  // Light blue
            if (inches < 0.25) return Color.FromArgb(150, 0, 255, 255);  // Cyan
            if (inches < 0.5) return Color.FromArgb(170, 0, 200, 255);   // Sky blue
            if (inches < 0.75) return Color.FromArgb(180, 0, 84, 255);   // Royal blue
            if (inches < 1.0) return Color.FromArgb(190, 0, 127, 0);     // Green
            if (inches < 1.5) return Color.FromArgb(200, 0, 255, 0);     // Lime
            if (inches < 2.0) return Color.FromArgb(210, 255, 255, 0);   // Yellow
            if (inches < 2.5) return Color.FromArgb(220, 255, 186, 0);   // Amber
            if (inches < 3.0) return Color.FromArgb(230, 255, 127, 0);   // Orange
            if (inches < 4.0) return Color.FromArgb(240, 255, 0, 0);     // Red
            if (inches < 5.0) return Color.FromArgb(245, 139, 0, 0);     // Dark red
            if (inches < 6.0) return Color.FromArgb(250, 177, 0, 177);   // Medium magenta
            
            return Color.FromArgb(255, 139, 0, 139);                     // Dark magenta
        }
        
        /// <summary>
        /// Get color for vertically integrated liquid (kg/m²)
        /// </summary>
        private Color GetVILColor(float value)
        {
            // Standard VIL color scale
            if (value < 1) return Color.FromArgb(130, 0, 0, 200);        // Dark blue
            if (value < 5) return Color.FromArgb(150, 0, 100, 255);      // Blue
            if (value < 10) return Color.FromArgb(170, 0, 235, 235);     // Cyan
            if (value < 15) return Color.FromArgb(180, 0, 200, 0);       // Green
            if (value < 20) return Color.FromArgb(190, 0, 255, 0);       // Bright green
            if (value < 25) return Color.FromArgb(200, 255, 255, 0);     // Yellow
            if (value < 30) return Color.FromArgb(210, 255, 200, 0);     // Gold
            if (value < 35) return Color.FromArgb(220, 255, 150, 0);     // Orange
            if (value < 40) return Color.FromArgb(230, 255, 100, 0);     // Bright orange
            if (value < 50) return Color.FromArgb(240, 255, 0, 0);       // Red
            if (value < 60) return Color.FromArgb(245, 200, 0, 0);       // Dark red
            if (value < 70) return Color.FromArgb(250, 150, 0, 150);     // Purple
            
            return Color.FromArgb(255, 255, 0, 255);                     // Magenta
        }
        
        /// <summary>
        /// Get color for echo tops (thousands of feet)
        /// </summary>
        private Color GetEchoTopsColor(float kft)
        {
            // Standard echo tops color scale
            if (kft < 5) return Color.FromArgb(150, 0, 255, 255);       // Cyan
            if (kft < 10) return Color.FromArgb(170, 0, 0, 255);        // Blue
            if (kft < 15) return Color.FromArgb(180, 0, 200, 0);        // Green
            if (kft < 20) return Color.FromArgb(190, 255, 255, 0);      // Yellow
            if (kft < 25) return Color.FromArgb(210, 255, 150, 0);      // Orange
            if (kft < 30) return Color.FromArgb(230, 255, 0, 0);        // Red
            if (kft < 40) return Color.FromArgb(240, 255, 0, 255);      // Magenta
            if (kft < 50) return Color.FromArgb(250, 128, 0, 128);      // Purple
            
            return Color.FromArgb(255, 255, 255, 255);                  // White
        }
        
        /// <summary>
        /// Get color for PhiDP value (degrees)
        /// </summary>
        private Color GetPhiDPColor(float value)
        {
            // PhiDP typically ranges from 0 to 180 degrees
            if (value < 20) return Color.FromArgb(180, 0, 0, 200);      // Dark blue
            if (value < 40) return Color.FromArgb(180, 0, 100, 255);    // Blue
            if (value < 60) return Color.FromArgb(180, 0, 190, 255);    // Light blue
            if (value < 80) return Color.FromArgb(180, 0, 255, 100);    // Blue-green
            if (value < 100) return Color.FromArgb(180, 0, 255, 0);     // Green
            if (value < 120) return Color.FromArgb(180, 255, 255, 0);   // Yellow
            if (value < 140) return Color.FromArgb(180, 255, 150, 0);   // Orange
            if (value < 160) return Color.FromArgb(180, 255, 0, 0);     // Red
            
            return Color.FromArgb(180, 200, 0, 100);                    // Burgundy
        }
        
        /// <summary>
        /// Get color for KDP value (degrees/km)
        /// </summary>
        private Color GetKDPColor(float value)
        {
            // KDP ranges from negative to positive values
            // Negative values: Light blues
            // Positive values: Yellows to reds (higher rain rates)
            if (value < -2) return Color.FromArgb(180, 0, 0, 150);      // Very dark blue
            if (value < -1) return Color.FromArgb(180, 0, 0, 255);      // Deep blue
            if (value < -0.5) return Color.FromArgb(180, 0, 127, 255);  // Blue
            if (value < 0) return Color.FromArgb(180, 0, 200, 255);     // Light blue
            if (value < 0.5) return Color.FromArgb(180, 123, 255, 123); // Light green
            if (value < 1) return Color.FromArgb(180, 0, 255, 0);       // Green
            if (value < 1.5) return Color.FromArgb(180, 200, 255, 0);   // Yellow-green
            if (value < 2) return Color.FromArgb(180, 255, 255, 0);     // Yellow
            if (value < 2.5) return Color.FromArgb(180, 255, 200, 0);   // Gold
            if (value < 3) return Color.FromArgb(180, 255, 150, 0);     // Orange
            if (value < 4) return Color.FromArgb(180, 255, 100, 0);     // Dark orange
            if (value < 5) return Color.FromArgb(180, 255, 0, 0);       // Red
            
            return Color.FromArgb(180, 200, 0, 0);                      // Dark red
        }
        
        /// <summary>
        /// Get color for storm relative motion (m/s)
        /// </summary>
        private Color GetStormRelativeMotionColor(float value)
        {
            // Similar to velocity but adjusted for storm motion
            return GetVelocityColor(value);
        }

        /// <summary>
        /// Get color for warning type and severity
        /// </summary>
        private Color GetWarningColor(WarningType type, string severity)
        {
            // Default colors based on type
            Color baseColor;
            int alpha = 180; // Semi-transparent
            
            // Warnings - Immediate threat colors (reds, oranges)
            switch (type)
            {
                // Warning colors - immediate threats
                case WarningType.Tornado:
                    baseColor = Color.FromArgb(alpha, 255, 0, 0); // Red
                    break;
                case WarningType.SevereThunderstorm:
                    baseColor = Color.FromArgb(alpha, 255, 165, 0); // Orange
                    break;
                case WarningType.FlashFlood:
                    baseColor = Color.FromArgb(alpha, 0, 255, 255); // Cyan
                    break;
                case WarningType.FloodWarning:
                    baseColor = Color.FromArgb(alpha, 0, 128, 255); // Light blue
                    break;
                case WarningType.Marine:
                    baseColor = Color.FromArgb(alpha, 173, 216, 230); // Light blue
                    break;
                case WarningType.WinterStorm:
                    baseColor = Color.FromArgb(alpha, 255, 250, 250); // Snow
                    break;
                case WarningType.ExtremeWind:
                    baseColor = Color.FromArgb(alpha, 218, 165, 32); // Goldenrod
                    break;
                case WarningType.Blizzard:
                case WarningType.IceStorm:
                    baseColor = Color.FromArgb(alpha, 138, 43, 226); // Purple
                    break;
                    
                // Watch colors - be prepared (yellows)
                case WarningType.TornadoWatch:
                    baseColor = Color.FromArgb(alpha, 255, 255, 0); // Yellow
                    break;
                case WarningType.SevereThunderstormWatch:
                    baseColor = Color.FromArgb(alpha, 255, 215, 0); // Gold
                    break;
                case WarningType.FlashFloodWatch:
                    baseColor = Color.FromArgb(alpha, 152, 251, 152); // Pale Green
                    break;
                case WarningType.WinterStormWatch:
                    baseColor = Color.FromArgb(alpha, 176, 224, 230); // Powder Blue
                    break;
                // Removed non-existent warning types
                    
                // Advisory colors - be aware (greens, light blues)
                case WarningType.SpecialWeatherStatement:
                    baseColor = Color.FromArgb(alpha, 60, 179, 113); // Medium Sea Green
                    break;
                case WarningType.HazardousWeatherOutlook:
                    baseColor = Color.FromArgb(alpha, 144, 238, 144); // Light Green
                    break;
                case WarningType.FrostFreeze:
                    baseColor = Color.FromArgb(alpha, 230, 230, 250); // Lavender
                    break;
                case WarningType.Flood:
                    baseColor = Color.FromArgb(alpha, 175, 238, 238); // Pale Turquoise
                    break;
                    
                // Default/Other
                default:
                    baseColor = Color.FromArgb(alpha, 128, 128, 128); // Gray
                    break;
            }
            
            // Adjust alpha based on severity
            if (!string.IsNullOrEmpty(severity))
            {
                switch (severity.ToLower())
                {
                    case "extreme":
                        alpha = 230;
                        break;
                    case "severe":
                        alpha = 210;
                        break;
                    case "moderate":
                        alpha = 180;
                        break;
                    case "minor":
                        alpha = 150;
                        break;
                }
            }
            
            return Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B);
        }

        /// <summary>
        /// Convert Tuple coordinates to Coordinate objects
        /// </summary>
        private List<Coordinate> ConvertCoordinates(List<Tuple<double, double>> tuples)
        {
            if (tuples == null)
                return new List<Coordinate>();
                
            var coordinates = new List<Coordinate>();
            foreach (var tuple in tuples)
            {
                coordinates.Add(new Coordinate(tuple.Item1, tuple.Item2));
            }
            
            return coordinates;
        }
        
        /// <summary>
        /// Log a message
        /// </summary>
        private void Log(string message)
        {
            _logger?.Invoke($"[MapService] {message}");
        }
    }
}
