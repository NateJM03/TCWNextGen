using System;

namespace NexradViewer.Models
{
    /// <summary>
    /// Represents a single radar data point (gate)
    /// </summary>
    public class RadarGate
    {
        /// <summary>
        /// The meteorological value at this gate (e.g., reflectivity in dBZ)
        /// </summary>
        public double Value { get; set; }
        
        /// <summary>
        /// Range from radar in kilometers
        /// </summary>
        public double Range { get; set; }
        
        /// <summary>
        /// Azimuth angle in degrees (0-359)
        /// </summary>
        public double Azimuth { get; set; }
        
        /// <summary>
        /// Elevation angle in degrees
        /// </summary>
        public double Elevation { get; set; }
        
        /// <summary>
        /// Latitude in decimal degrees
        /// </summary>
        public double Latitude { get; set; }
        
        /// <summary>
        /// Longitude in decimal degrees
        /// </summary>
        public double Longitude { get; set; }
        
        /// <summary>
        /// Height above ground in meters (if available)
        /// </summary>
        public double? Height { get; set; }
        
        /// <summary>
        /// Whether this gate has valid data
        /// </summary>
        public bool IsValid => !double.IsNaN(Value) && Value != 0;
        
        /// <summary>
        /// Creates a string representation of this gate
        /// </summary>
        public override string ToString()
        {
            return $"Gate: Value={Value}, Range={Range}km, Azimuth={Azimuth}°, Lat/Lon={Latitude}/{Longitude}";
        }
    }
}
