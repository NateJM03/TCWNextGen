using System;
using System.Collections.Generic;
using System.Drawing;

namespace NexradViewer.Models
{
    /// <summary>
    /// Represents a point in map coordinates
    /// </summary>
    public struct MPoint
    {
        /// <summary>
        /// Gets or sets the X coordinate (longitude)
        /// </summary>
        public double X { get; set; }
        
        /// <summary>
        /// Gets or sets the Y coordinate (latitude)
        /// </summary>
        public double Y { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MPoint"/> struct
        /// </summary>
        public MPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
        
        /// <summary>
        /// Returns a string representation of the point
        /// </summary>
        public override string ToString()
        {
            return $"({X:F6}, {Y:F6})";
        }
    }
    
    /// <summary>
    /// Base interface for map features
    /// </summary>
    public interface IFeature
    {
        /// <summary>
        /// Gets or sets the properties of the feature
        /// </summary>
        Dictionary<string, object> Properties { get; set; }
        
        /// <summary>
        /// Gets the geometry type of the feature
        /// </summary>
        string GeometryType { get; }
    }
    
    /// <summary>
    /// Represents a point feature on the map
    /// </summary>
    public class PointFeature : IFeature
    {
        /// <summary>
        /// Gets or sets the latitude
        /// </summary>
        public double Latitude { get; set; }
        
        /// <summary>
        /// Gets or sets the longitude
        /// </summary>
        public double Longitude { get; set; }
        
        /// <summary>
        /// Gets or sets the properties of the feature
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        
        /// <summary>
        /// Gets the geometry type of the feature
        /// </summary>
        public string GeometryType => "Point";
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PointFeature"/> class
        /// </summary>
        public PointFeature()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PointFeature"/> class
        /// </summary>
        public PointFeature(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
    
    /// <summary>
    /// Represents a collection of features
    /// </summary>
    public class FeatureCollection
    {
        /// <summary>
        /// Gets or sets the features in the collection
        /// </summary>
        public List<IFeature> Features { get; set; } = new List<IFeature>();
        
        /// <summary>
        /// Gets the number of features in the collection
        /// </summary>
        public int Count => Features.Count;
        
        /// <summary>
        /// Adds a feature to the collection
        /// </summary>
        public void Add(IFeature feature)
        {
            Features.Add(feature);
        }
        
        /// <summary>
        /// Clears all features from the collection
        /// </summary>
        public void Clear()
        {
            Features.Clear();
        }
    }
    
    /// <summary>
    /// Represents a coordinate pair
    /// </summary>
    public struct Coordinate
    {
        /// <summary>
        /// Gets or sets the X coordinate (longitude)
        /// </summary>
        public double X { get; set; }
        
        /// <summary>
        /// Gets or sets the Y coordinate (latitude)
        /// </summary>
        public double Y { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Coordinate"/> struct
        /// </summary>
        public Coordinate(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
    
    /// <summary>
    /// Represents a radar station feature
    /// </summary>
    public class StationFeature : PointFeature
    {
        /// <summary>
        /// Gets or sets the station ID
        /// </summary>
        public string StationId { get; set; }
        
        /// <summary>
        /// Gets or sets the station name
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="StationFeature"/> class
        /// </summary>
        public StationFeature(double latitude, double longitude, string stationId, string name) 
            : base(latitude, longitude)
        {
            StationId = stationId;
            Name = name;
            Properties.Add("StationId", stationId);
            Properties.Add("Name", name);
        }
    }
    
    /// <summary>
    /// Represents a radar data feature
    /// </summary>
    public class RadarFeature : PointFeature
    {
        /// <summary>
        /// Gets or sets the radar value
        /// </summary>
        public double Value { get; set; }
        
        /// <summary>
        /// Gets or sets the product type
        /// </summary>
        public RadarProductType ProductType { get; set; }
        
        /// <summary>
        /// Gets or sets the color for this radar point
        /// </summary>
        public Color Color { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RadarFeature"/> class
        /// </summary>
        public RadarFeature(double latitude, double longitude, double value, RadarProductType productType) 
            : base(latitude, longitude)
        {
            Value = value;
            ProductType = productType;
            Properties.Add("Value", value);
            Properties.Add("ProductType", productType.ToString());
        }
    }
    
    /// <summary>
    /// Represents a weather warning feature
    /// </summary>
    public class WarningFeature : PointFeature
    {
        /// <summary>
        /// Gets or sets the warning type
        /// </summary>
        public WarningType WarningType { get; set; }
        
        /// <summary>
        /// Gets or sets the warning title
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Gets or sets the warning description
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WarningFeature"/> class
        /// </summary>
        public WarningFeature(double latitude, double longitude, WarningType warningType, string title, string description) 
            : base(latitude, longitude)
        {
            WarningType = warningType;
            Title = title;
            Description = description;
            Properties.Add("WarningType", warningType.ToString());
            Properties.Add("Title", title);
            Properties.Add("Description", description);
        }
    }
    
    /// <summary>
    /// Memory provider for storing features
    /// </summary>
    public class MemoryProvider
    {
        /// <summary>
        /// Gets or sets the features in this provider
        /// </summary>
        public List<IFeature> Features { get; set; } = new List<IFeature>();
        
        /// <summary>
        /// Adds a feature to the provider
        /// </summary>
        public void Add(IFeature feature)
        {
            Features.Add(feature);
        }
        
        /// <summary>
        /// Clears all features from the provider
        /// </summary>
        public void Clear()
        {
            Features.Clear();
        }
    }
}
