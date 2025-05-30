using System;
using System.Collections.Generic;

namespace NexradViewer.Models
{
    /// <summary>
    /// Represents processed radar data for display
    /// </summary>
    public class RadarData
    {
        /// <summary>
        /// Gets or sets the station ID
        /// </summary>
        public string StationId { get; set; }
        
        /// <summary>
        /// Gets or sets the radar product
        /// </summary>
        public RadarProductType ProductType { get; set; }
        
        /// <summary>
        /// Gets or sets the legacy radar product (for backward compatibility)
        /// </summary>
        public RadarProduct Product { get; set; }
        
        /// <summary>
        /// Gets or sets the timestamp of the data
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Gets or sets the volume coverage pattern (VCP)
        /// </summary>
        public int Vcp { get; set; }
        
        /// <summary>
        /// Gets or sets the elevation angle in degrees
        /// </summary>
        public double Elevation { get; set; }
        
        /// <summary>
        /// Gets or sets the radar station latitude
        /// </summary>
        public double Latitude { get; set; }
        
        /// <summary>
        /// Gets or sets the radar station longitude
        /// </summary>
        public double Longitude { get; set; }
        
        /// <summary>
        /// Gets or sets the collection of radar gates (data points)
        /// </summary>
        public List<RadarGate> Gates { get; set; } = new List<RadarGate>();
        
        /// <summary>
        /// Gets or sets additional information about the data (like mode, source, etc.)
        /// </summary>
        public string Mode { get; set; }
        
        /// <summary>
        /// Gets whether the data has any gates
        /// </summary>
        public bool HasData => Gates != null && Gates.Count > 0;
    }
}
