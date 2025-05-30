using System;
using System.Collections.Generic;
using System.Linq;
using NexradViewer.Models;
using NexradViewer.Utils;

namespace NexradViewer.Controls
{
    /// <summary>
    /// Custom Map implementation for NextGenRadar
    /// </summary>
    public class Map
    {
        private readonly List<MapLayer> _layers = new List<MapLayer>();
        private MapNavigator _navigator;
        
        /// <summary>
        /// Raised when the map view changes (e.g., pan, zoom)
        /// </summary>
        public event EventHandler ViewChanged;
        
        /// <summary>
        /// Gets the navigator for this map
        /// </summary>
        public MapNavigator Navigator => _navigator;
        
        /// <summary>
        /// Gets the layers collection for this map
        /// </summary>
        public List<MapLayer> Layers => _layers;
        
        /// <summary>
        /// Gets or sets the home action for the map
        /// </summary>
        public Action<MapNavigator> Home { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Map"/> class
        /// </summary>
        public Map()
        {
            try
            {
                // Initialize navigator with default viewport
                _navigator = new MapNavigator();
                
                // Set default home action if none is provided
                Home = navigator => navigator.CenterOn(new MPoint(-98.5795, 39.8283), 0.05);
                
                // Subscribe to navigator viewport changes
                _navigator.ViewportChanged += Navigator_ViewportChanged;
                
                Logger.Log("Map initialized successfully");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Map initialization");
            }
        }
        
        /// <summary>
        /// Called when the navigator's viewport changes
        /// </summary>
        private void Navigator_ViewportChanged(object sender, EventArgs e)
        {
            // Raise the ViewChanged event
            OnViewChanged();
        }
        
        /// <summary>
        /// Raises the ViewChanged event
        /// </summary>
        protected virtual void OnViewChanged()
        {
            try
            {
                ViewChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Map.OnViewChanged");
            }
        }
        
        /// <summary>
        /// Refreshes the map by raising the ViewChanged event
        /// </summary>
        public void Refresh()
        {
            OnViewChanged();
        }
    }
    
    /// <summary>
    /// Base class for map layers
    /// </summary>
    public class MapLayer
    {
        /// <summary>
        /// Gets or sets the name of the layer
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets whether the layer is visible
        /// </summary>
        public bool IsVisible { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the opacity of the layer (0.0 to 1.0)
        /// </summary>
        public float Opacity { get; set; } = 1.0f;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MapLayer"/> class
        /// </summary>
        public MapLayer(string name = "")
        {
            Name = name;
        }
        
        /// <summary>
        /// Signals that the data in this layer has changed and should be redrawn
        /// </summary>
        public virtual void DataHasChanged()
        {
            // Base implementation does nothing; subclasses can override
        }
    }
    
    /// <summary>
    /// Memory-based map layer for storing features in memory
    /// </summary>
    public class MemoryLayer : MapLayer
    {
        private List<IFeature> _features = new List<IFeature>();
        
        /// <summary>
        /// Gets or sets the features in this layer
        /// </summary>
        public List<IFeature> Features 
        {
            get => _features;
            set => _features = value ?? new List<IFeature>();
        }
        
        /// <summary>
        /// Gets the number of features in this layer
        /// </summary>
        public int Count => _features?.Count ?? 0;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryLayer"/> class
        /// </summary>
        public MemoryLayer(string name = "") : base(name)
        {
        }
        
        /// <summary>
        /// Signals that the data in this layer has changed and should be redrawn
        /// </summary>
        public override void DataHasChanged()
        {
            // In a real implementation, this would trigger rendering
            // For now, it's just a placeholder
            Logger.Log($"Layer {Name} data changed - {_features?.Count ?? 0} features");
        }
    }
    
    /// <summary>
    /// Map navigator that controls the view (pan, zoom) of the map
    /// </summary>
    public class MapNavigator
    {
        private Viewport _viewport;
        
        /// <summary>
        /// Raised when the viewport changes
        /// </summary>
        public event EventHandler ViewportChanged;
        
        /// <summary>
        /// Gets the current viewport
        /// </summary>
        public Viewport Viewport => _viewport;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MapNavigator"/> class
        /// </summary>
        public MapNavigator()
        {
            try
            {
                // Initialize with a default viewport
                _viewport = new Viewport
                {
                    CenterX = -98.5795, // Continental US center approximate longitude
                    CenterY = 39.8283,  // Continental US center approximate latitude
                    Resolution = 0.05    // Initial resolution
                };
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "MapNavigator initialization");
            }
        }
        
        /// <summary>
        /// Centers the viewport on the specified coordinates
        /// </summary>
        public void CenterOn(double x, double y)
        {
            try
            {
                _viewport.CenterX = x;
                _viewport.CenterY = y;
                OnViewportChanged();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "MapNavigator.CenterOn");
            }
        }
        
        /// <summary>
        /// Centers the viewport on the specified coordinates with the specified resolution
        /// </summary>
        public void CenterOn(double x, double y, double resolution)
        {
            try
            {
                _viewport.CenterX = x;
                _viewport.CenterY = y;
                _viewport.Resolution = resolution;
                OnViewportChanged();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "MapNavigator.CenterOn with resolution");
            }
        }
        
        /// <summary>
        /// Centers the viewport on the specified point
        /// </summary>
        public void CenterOn(MPoint point)
        {
            CenterOn(point.X, point.Y);
        }
        
        /// <summary>
        /// Centers the viewport on the specified point with the specified resolution
        /// </summary>
        public void CenterOn(MPoint point, double resolution)
        {
            CenterOn(point.X, point.Y, resolution);
        }
        
        /// <summary>
        /// Zooms to the specified resolution
        /// </summary>
        public void ZoomTo(double resolution)
        {
            try
            {
                _viewport.Resolution = resolution;
                OnViewportChanged();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "MapNavigator.ZoomTo");
            }
        }
        
        /// <summary>
        /// Zooms in (decreases resolution)
        /// </summary>
        public void ZoomIn()
        {
            try
            {
                _viewport.Resolution /= 2.0;
                OnViewportChanged();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "MapNavigator.ZoomIn");
            }
        }
        
        /// <summary>
        /// Zooms out (increases resolution)
        /// </summary>
        public void ZoomOut()
        {
            try
            {
                _viewport.Resolution *= 2.0;
                OnViewportChanged();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "MapNavigator.ZoomOut");
            }
        }
        
        /// <summary>
        /// Raises the ViewportChanged event
        /// </summary>
        protected virtual void OnViewportChanged()
        {
            try
            {
                ViewportChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "MapNavigator.OnViewportChanged");
            }
        }
    }
    
    /// <summary>
    /// Represents the map viewport (visible area)
    /// </summary>
    public class Viewport
    {
        /// <summary>
        /// Gets or sets the center X coordinate (longitude)
        /// </summary>
        public double CenterX { get; set; }
        
        /// <summary>
        /// Gets or sets the center Y coordinate (latitude)
        /// </summary>
        public double CenterY { get; set; }
        
        /// <summary>
        /// Gets or sets the resolution (smaller values = more zoom)
        /// </summary>
        public double Resolution { get; set; }
        
        /// <summary>
        /// Gets or sets the width of the viewport in pixels
        /// </summary>
        public double Width { get; set; } = 1000;
        
        /// <summary>
        /// Gets or sets the height of the viewport in pixels
        /// </summary>
        public double Height { get; set; } = 600;
    }
}
