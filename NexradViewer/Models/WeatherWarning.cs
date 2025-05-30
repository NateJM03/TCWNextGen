using System;
using System.Collections.Generic;
using System.Drawing;

namespace NexradViewer.Models
{
    /// <summary>
    /// Represents a weather warning or alert from the National Weather Service
    /// </summary>
    public class WeatherWarning
    {
        /// <summary>
        /// Gets or sets the unique identifier for the warning
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Gets or sets the warning type
        /// </summary>
        public WarningType Type { get; set; }
        
        /// <summary>
        /// Gets or sets the title or headline of the warning
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Gets or sets the descriptive text of the warning
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the issuing office for the warning (e.g., "NWS Dallas/Fort Worth")
        /// </summary>
        public string IssuingOffice { get; set; }
        
        /// <summary>
        /// Gets or sets the full message text of the warning
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Gets or sets a list of affected areas (counties, parishes, etc.)
        /// </summary>
        public List<string> AffectedAreas { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets or sets the source of the warning (typically "NWS")
        /// </summary>
        public string Source { get; set; }
        
        /// <summary>
        /// Gets or sets the severity level of the warning
        /// </summary>
        public string Severity { get; set; }
        
        /// <summary>
        /// Gets or sets the certainty level of the warning
        /// </summary>
        public string Certainty { get; set; }
        
        /// <summary>
        /// Gets or sets the urgency level of the warning
        /// </summary>
        public string Urgency { get; set; }
        
        /// <summary>
        /// Gets or sets the event that triggered the warning
        /// </summary>
        public string Event { get; set; }
        
        /// <summary>
        /// Gets or sets the issuance time of the warning
        /// </summary>
        public DateTime IssuedTime { get; set; }
        
        /// <summary>
        /// Gets or sets the expiration time of the warning
        /// </summary>
        public DateTime ExpiresTime { get; set; }
        
        /// <summary>
        /// Gets or sets the polygons that define the warning area
        /// </summary>
        public List<List<LatLon>> Polygons { get; set; } = new List<List<LatLon>>();
        
        /// <summary>
        /// Gets or sets the coordinates that define the warning area (as a simplified list for map rendering)
        /// </summary>
        public List<Tuple<double, double>> Coordinates 
        { 
            get 
            {
                // Convert from Polygons structure if not already set
                if (_coordinates == null || _coordinates.Count == 0)
                {
                    _coordinates = new List<Tuple<double, double>>();
                    if (Polygons != null && Polygons.Count > 0)
                    {
                        foreach (var point in Polygons[0]) // Use the first polygon
                        {
                            _coordinates.Add(new Tuple<double, double>(point.Latitude, point.Longitude));
                        }
                    }
                }
                return _coordinates;
            }
            set { _coordinates = value; }
        }
        
        private List<Tuple<double, double>> _coordinates = new List<Tuple<double, double>>();
        
        /// <summary>
        /// Gets or sets the affected areas/zones
        /// </summary>
        public List<string> AffectedZones { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets or sets the instruction text
        /// </summary>
        public string Instruction { get; set; }
        
        /// <summary>
        /// Gets whether the warning is still active
        /// </summary>
        public bool IsActive => DateTime.UtcNow < ExpiresTime;
        
        /// <summary>
        /// Gets the color to use for displaying this warning
        /// </summary>
        public Color GetColor()
        {
            return MappingTypes.GetWarningColor(Type);
        }
        
        /// <summary>
        /// Creates a new WeatherWarning with no data
        /// </summary>
        public WeatherWarning() { }
        
        /// <summary>
        /// Creates a new WeatherWarning with the specified parameters
        /// </summary>
        public WeatherWarning(string id, WarningType type, string title, 
                              string severity, DateTime issuedTime, DateTime expiresTime)
        {
            Id = id;
            Type = type;
            Title = title;
            Severity = severity;
            IssuedTime = issuedTime;
            ExpiresTime = expiresTime;
            Source = "NWS";
            Event = title;
        }
        
        /// <summary>
        /// Gets a string representation of this warning
        /// </summary>
        public override string ToString()
        {
            return $"{Event} - {Title} (Expires: {ExpiresTime.ToLocalTime():g})";
        }
    }
    
    /// <summary>
    /// Represents a latitude/longitude coordinate
    /// </summary>
    public class LatLon
    {
        /// <summary>
        /// Gets or sets the latitude value
        /// </summary>
        public double Latitude { get; set; }
        
        /// <summary>
        /// Gets or sets the longitude value
        /// </summary>
        public double Longitude { get; set; }
        
        /// <summary>
        /// Creates a new LatLon instance
        /// </summary>
        public LatLon() { }
        
        /// <summary>
        /// Creates a new LatLon instance with the specified coordinates
        /// </summary>
        public LatLon(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
        
        /// <summary>
        /// Creates a string representation of these coordinates
        /// </summary>
        public override string ToString()
        {
            return $"{Latitude:F6},{Longitude:F6}";
        }
    }
}
