using System.Text.Json.Serialization;

namespace NexradViewer.Models
{
    /// <summary>
    /// Represents a NEXRAD radar station
    /// </summary>
    public class RadarStation
    {
        /// <summary>
        /// Station ID (ICAO identifier)
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        /// <summary>
        /// Station name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        /// <summary>
        /// State or region
        /// </summary>
        [JsonPropertyName("state")]
        public string State { get; set; }
        
        /// <summary>
        /// Country
        /// </summary>
        [JsonPropertyName("country")]
        public string Country { get; set; }
        
        /// <summary>
        /// Latitude
        /// </summary>
        [JsonPropertyName("lat")]
        public double Latitude { get; set; }
        
        /// <summary>
        /// Longitude
        /// </summary>
        [JsonPropertyName("lon")]
        public double Longitude { get; set; }
        
        /// <summary>
        /// Elevation (in meters)
        /// </summary>
        [JsonPropertyName("elevation")]
        public double Elevation { get; set; }
        
        /// <summary>
        /// Station type (e.g., "NEXRAD WSR-88D")
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public RadarStation() { }
        
        /// <summary>
        /// Constructor with basic properties
        /// </summary>
        public RadarStation(string id, string name, double latitude, double longitude)
        {
            Id = id;
            Name = name;
            Latitude = latitude;
            Longitude = longitude;
        }
        
        /// <summary>
        /// Get a friendly display name
        /// </summary>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(State))
                return $"{Id} - {Name}";
            
            return $"{Id} - {Name}, {State}";
        }
    }
}
