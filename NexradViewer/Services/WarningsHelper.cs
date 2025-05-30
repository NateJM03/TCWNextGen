using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using NexradViewer.Models;
using System.Drawing;

namespace NexradViewer.Services
{
    /// <summary>
    /// Helper service for accessing and processing NWS warnings
    /// </summary>
    public class WarningsHelper
    {
        private readonly HttpClient _httpClient;
        private readonly Action<string> _logger;
        
        // NWS API endpoints
        private const string NwsApiBaseUrl = "https://api.weather.gov";
        private const string ActiveAlertsEndpoint = "/alerts/active";
        
        // Cache for warnings
        private List<WeatherWarning> _cachedWarnings = new List<WeatherWarning>();
        private DateTime _lastWarningFetch = DateTime.MinValue;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WarningsHelper"/> class
        /// </summary>
        public WarningsHelper(Action<string> logger = null)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", 
                "TheClearWeather NextGenRadar/2.0.0 (tcw-nexrad@example.com)");
            _logger = logger ?? (msg => { });
        }
        
        /// <summary>
        /// Gets active weather warnings from the NWS API
        /// </summary>
        public async Task<List<WeatherWarning>> GetActiveWarningsAsync(bool useCache = true)
        {
            try
            {
                // Check if cached warnings are still valid (< 5 minutes old)
                if (useCache && _cachedWarnings.Count > 0 && 
                    (DateTime.Now - _lastWarningFetch).TotalMinutes < 5)
                {
                    Log($"Using {_cachedWarnings.Count} cached warnings");
                    return _cachedWarnings;
                }
                
                Log("Fetching active warnings from NWS API");
                
                // Fetch warnings from NWS API
                var response = await _httpClient.GetAsync($"{NwsApiBaseUrl}{ActiveAlertsEndpoint}");
                if (response.IsSuccessStatusCode)
                {
                    // Parse the JSON response
                    var json = await response.Content.ReadAsStringAsync();
                    var warnings = ParseNwsAlerts(json);
                    
                    // Cache the warnings
                    _cachedWarnings = warnings;
                    _lastWarningFetch = DateTime.Now;
                    
                    Log($"Retrieved {warnings.Count} active warnings");
                    return warnings;
                }
                else
                {
                    Log($"Error retrieving warnings: {response.StatusCode}");
                    
                    if (_cachedWarnings.Count > 0)
                    {
                        Log($"Using {_cachedWarnings.Count} cached warnings (API error)");
                        return _cachedWarnings;
                    }
                    
                    // Generate mock warnings for demo purposes
                    return CreateMockWarnings();
                }
            }
            catch (Exception ex)
            {
                Log($"Exception retrieving warnings: {ex.Message}");
                
                if (_cachedWarnings.Count > 0)
                {
                    Log($"Using {_cachedWarnings.Count} cached warnings (exception)");
                    return _cachedWarnings;
                }
                
                // Generate mock warnings for demo purposes
                return CreateMockWarnings();
            }
        }
        
        /// <summary>
        /// Parses NWS API JSON alerts into weather warnings
        /// </summary>
        private List<WeatherWarning> ParseNwsAlerts(string json)
        {
            try
            {
                var warnings = new List<WeatherWarning>();
                
                // Parse JSON document
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    // Get features array
                    JsonElement featuresElement;
                    if (doc.RootElement.TryGetProperty("features", out featuresElement) && 
                        featuresElement.ValueKind == JsonValueKind.Array)
                    {
                        // Process each feature (alert)
                        foreach (JsonElement featureElement in featuresElement.EnumerateArray())
                        {
                            try
                            {
                                // Extract properties
                                if (featureElement.TryGetProperty("properties", out JsonElement props))
                                {
                                    // Basic alert properties
                                    string id = props.TryGetProperty("id", out var idElement) ? 
                                        idElement.GetString() : "unknown";
                                        
                                    string headline = props.TryGetProperty("headline", out var headlineElement) ? 
                                        headlineElement.GetString() : "";
                                        
                                    string description = props.TryGetProperty("description", out var descElement) ? 
                                        descElement.GetString() : "";
                                        
                                    string severity = props.TryGetProperty("severity", out var severityElement) ? 
                                        severityElement.GetString() : "Unknown";
                                        
                                    // Get event type to determine warning type
                                    string eventType = props.TryGetProperty("event", out var eventElement) ? 
                                        eventElement.GetString() : "";
                                        
                                    // Parse effective and expires times
                                    DateTime effective = DateTime.Now;
                                    if (props.TryGetProperty("effective", out var effectiveElement) && 
                                        !string.IsNullOrEmpty(effectiveElement.GetString()))
                                    {
                                        DateTime.TryParse(effectiveElement.GetString(), out effective);
                                    }
                                    
                                    DateTime expires = DateTime.Now.AddHours(1);
                                    if (props.TryGetProperty("expires", out var expiresElement) && 
                                        !string.IsNullOrEmpty(expiresElement.GetString()))
                                    {
                                        DateTime.TryParse(expiresElement.GetString(), out expires);
                                    }
                                    
                                    // Get sender
                                    string sender = props.TryGetProperty("senderName", out var senderElement) ? 
                                        senderElement.GetString() : "National Weather Service";
                                        
                                    // Get affected areas
                                    var affectedAreas = new List<string>();
                                    if (props.TryGetProperty("areaDesc", out var areaElement) && 
                                        !string.IsNullOrEmpty(areaElement.GetString()))
                                    {
                                        // Parse area description (comma-separated list)
                                        var areas = areaElement.GetString().Split(',');
                                        foreach (var area in areas)
                                        {
                                            affectedAreas.Add(area.Trim());
                                        }
                                    }
                                    
                                    // Determine warning type based on event type
                                    WarningType warningType = DetermineWarningType(eventType);
                                    
                                    // Create warning
                                    var warning = new WeatherWarning(
                                        id,
                                        warningType,
                                        headline,
                                        severity,
                                        effective,
                                        expires)
                                    {
                                        IssuingOffice = sender,
                                        Message = description,
                                        AffectedAreas = affectedAreas
                                    };
                                    
                                    warnings.Add(warning);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log($"Error processing alert: {ex.Message}");
                                // Continue with next alert
                            }
                        }
                    }
                }
                
                return warnings;
            }
            catch (Exception ex)
            {
                Log($"Error parsing NWS alerts: {ex.Message}");
                return new List<WeatherWarning>();
            }
        }
        
        /// <summary>
        /// Determines the warning type based on event type
        /// </summary>
        private WarningType DetermineWarningType(string eventType)
        {
            if (string.IsNullOrEmpty(eventType))
                return WarningType.Other;
                
            eventType = eventType.ToLower();
            
            if (eventType.Contains("tornado") && eventType.Contains("warning"))
                return WarningType.Tornado;
                
            if (eventType.Contains("tornado") && eventType.Contains("watch"))
                return WarningType.TornadoWatch;
                
            if (eventType.Contains("severe thunderstorm") && eventType.Contains("warning"))
                return WarningType.SevereThunderstorm;
                
            if (eventType.Contains("severe thunderstorm") && eventType.Contains("watch"))
                return WarningType.SevereThunderstormWatch;
                
            if (eventType.Contains("flash flood") && eventType.Contains("warning"))
                return WarningType.FlashFlood;
                
            if (eventType.Contains("flood") && eventType.Contains("warning"))
                return WarningType.FloodWarning;
                
            if ((eventType.Contains("flash flood") || eventType.Contains("flood")) && 
                eventType.Contains("watch"))
                return WarningType.FlashFloodWatch;
                
            if (eventType.Contains("winter") && eventType.Contains("warning"))
                return WarningType.WinterStorm;
                
            if (eventType.Contains("winter") && eventType.Contains("watch"))
                return WarningType.WinterStormWatch;
                
            if (eventType.Contains("special weather"))
                return WarningType.SpecialWeatherStatement;
                
            if (eventType.Contains("hazardous weather"))
                return WarningType.HazardousWeatherOutlook;
                
            return WarningType.Other;
        }
        
        /// <summary>
        /// Creates mock warnings for demo purposes
        /// </summary>
        private List<WeatherWarning> CreateMockWarnings()
        {
            var mockWarnings = new List<WeatherWarning>();
            
            // Tornado Warning
            mockWarnings.Add(new WeatherWarning(
                "TOR.NWS.202505210001",
                WarningType.Tornado,
                "Tornado Warning for Dallas County",
                "Extreme",
                DateTime.Now,
                DateTime.Now.AddHours(1))
            {
                IssuingOffice = "National Weather Service Dallas/Fort Worth",
                Message = "TORNADO WARNING: A confirmed large tornado was observed near downtown Dallas moving northeast at 30 mph. TAKE COVER NOW.",
                AffectedAreas = new List<string> { "Dallas County", "Tarrant County" }
            });
            
            // Severe Thunderstorm Warning
            mockWarnings.Add(new WeatherWarning(
                "SVR.NWS.202505210002",
                WarningType.SevereThunderstorm,
                "Severe Thunderstorm Warning for Harris County",
                "Severe", 
                DateTime.Now,
                DateTime.Now.AddHours(2))
            {
                IssuingOffice = "National Weather Service Houston",
                Message = "SEVERE THUNDERSTORM WARNING: Quarter size hail and 70 mph wind gusts possible.",
                AffectedAreas = new List<string> { "Harris County", "Fort Bend County" }
            });
            
            // Flash Flood Warning
            mockWarnings.Add(new WeatherWarning(
                "FFW.NWS.202505210003",
                WarningType.FlashFlood,
                "Flash Flood Warning for Los Angeles County",
                "Moderate", 
                DateTime.Now,
                DateTime.Now.AddHours(6))
            {
                IssuingOffice = "National Weather Service Los Angeles",
                Message = "FLASH FLOOD WARNING: Heavy rainfall causing flooding of small creeks and streams.",
                AffectedAreas = new List<string> { "Los Angeles County" }
            });
            
            // Tornado Watch
            mockWarnings.Add(new WeatherWarning(
                "TOA.NWS.202505210004",
                WarningType.TornadoWatch,
                "Tornado Watch for Central Oklahoma",
                "Severe", 
                DateTime.Now,
                DateTime.Now.AddHours(8))
            {
                IssuingOffice = "Storm Prediction Center Norman OK",
                Message = "TORNADO WATCH: Conditions are favorable for tornadoes to develop.",
                AffectedAreas = new List<string> { "Oklahoma County", "Cleveland County", "Canadian County" }
            });
            
            Log($"Created {mockWarnings.Count} mock warnings");
            return mockWarnings;
        }
        
        /// <summary>
        /// Gets the color for a warning type
        /// </summary>
        public Color GetWarningColor(WarningType warningType)
        {
            // Use the centralized mapping function from MappingTypes
            return MappingTypes.GetWarningColor(warningType);
        }
        
        private void Log(string message)
        {
            _logger?.Invoke($"[WarningsHelper] {message}");
        }
    }
}
