using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using BruTile;
using BruTile.Predefined;
using BruTile.Web;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
using Mapsui.UI.Avalonia;
using NexradViewer.Models;
using NexradViewer.Utils;

namespace NexradViewer.Controls
{
    /// <summary>
    /// Enhanced MapControl using Mapsui with OpenStreetMap tiles for proper mapping functionality
    /// ONLINE MAP - REQUIRES INTERNET CONNECTION
    /// </summary>
    public class MapControl : UserControl
    {
        private Mapsui.UI.Avalonia.MapControl _mapsuiControl;
        private readonly Action<string> _logger;
        private float _opacity = 0.8f;
        private Dictionary<string, ILayer> _customLayers = new Dictionary<string, ILayer>();

        /// <summary>
        /// Gets the underlying Mapsui Map
        /// </summary>
        public Mapsui.Map Map => _mapsuiControl?.Map;

        /// <summary>
        /// Gets or sets the opacity for rendering
        /// </summary>
        public new float Opacity
        {
            get => _opacity;
            set
            {
                _opacity = value;
                RefreshLayerOpacity();
            }
        }

        /// <summary>
        /// Gets or sets the zoom level
        /// </summary>
        public double ZoomLevel
        {
            get => _mapsuiControl?.Map?.Navigator?.Viewport.Resolution ?? 0.0;
            set
            {
                if (_mapsuiControl?.Map?.Navigator != null)
                {
                    _mapsuiControl.Map.Navigator.ZoomTo(value);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapControl"/> class with no parameters
        /// Required for XAML instantiation
        /// </summary>
        public MapControl() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapControl"/> class
        /// </summary>
        public MapControl(Action<string> logger)
        {
            _logger = logger ?? (msg => Console.WriteLine($"[MapControl] {msg}"));
            
            try
            {
                InitializeComponents();
                Log("ONLINE MapControl initialized successfully - REQUIRES INTERNET");
            }
            catch (Exception ex)
            {
                Log($"Error initializing MapControl: {ex.Message}");
                CreateFallbackUI(ex);
            }
        }

        /// <summary>
        /// Create fallback UI when map initialization fails
        /// </summary>
        private void CreateFallbackUI(Exception ex)
        {
            var textBlock = new TextBlock
            {
                Text = $"Map initialization failed: {ex.Message}\n\nCheck logs for details.\nThis map requires internet connectivity.",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.Red),
                FontSize = 16,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };
            
            Content = new Border
            {
                Background = new SolidColorBrush(Colors.LightGray),
                BorderBrush = new SolidColorBrush(Colors.Gray),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10),
                Child = textBlock
            };
        }

        /// <summary>
        /// Initialize the Mapsui components with OpenStreetMap tiles
        /// </summary>
        private void InitializeComponents()
        {
            // Create Mapsui MapControl
            _mapsuiControl = new Mapsui.UI.Avalonia.MapControl();
            
            // Initialize the map
            var map = new Mapsui.Map();
            
            // Add OpenStreetMap tile layer
            try
            {
                var osmLayer = CreateOpenStreetMapLayer();
                map.Layers.Add(osmLayer);
                Log("Added OpenStreetMap tile layer - internet connectivity required");
            }
            catch (Exception ex)
            {
                Log($"Failed to add OpenStreetMap layer: {ex.Message}");
                throw; // Re-throw to trigger fallback UI
            }

            // Set the map to the control
            _mapsuiControl.Map = map;
            
            // Set initial view to continental US
            var centerLonLat = new Mapsui.MPoint(-98.5795, 39.8283); // Geographic center of continental US
            var mercatorCoords = SphericalMercator.FromLonLat(centerLonLat.X, centerLonLat.Y);
            var centerSphericalMercator = new Mapsui.MPoint(mercatorCoords.x, mercatorCoords.y);
            
            // Set appropriate zoom level for continental US view
            var continentalUSResolution = 20000000.0; // Good zoom level for viewing entire continental US
            map.Navigator.CenterOnAndZoomTo(centerSphericalMercator, continentalUSResolution);
            
            // Set the control as content
            Content = _mapsuiControl;
            
            // Subscribe to map events
            map.DataChanged += (sender, args) => Log("Map data changed");
            
            Log("ONLINE Mapsui map initialized with OpenStreetMap tiles - pan/zoom/scroll functionality enabled");
        }

        /// <summary>
        /// Create OpenStreetMap tile layer using BruTile
        /// </summary>
        private TileLayer CreateOpenStreetMapLayer()
        {
            try
            {
                // Create OpenStreetMap tile source using BruTile
                var osmTileSource = new HttpTileSource(new GlobalSphericalMercator(0, 18),
                    "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
                    new[] { "a", "b", "c" }, name: "OpenStreetMap");

                var tileLayer = new TileLayer(osmTileSource)
                {
                    Name = "OpenStreetMap"
                };

                return tileLayer;
            }
            catch (Exception ex)
            {
                Log($"Error creating OpenStreetMap layer: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Creates a style for radar data visualization
        /// </summary>
        private IStyle CreateRadarStyle(RadarProductType productType, float opacity)
        {
            // Create different styles based on radar product type - using correct enum values
            var color = productType switch
            {
                RadarProductType.BaseReflectivity => Mapsui.Styles.Color.Green,
                RadarProductType.BaseVelocity => Mapsui.Styles.Color.Blue,
                RadarProductType.SpectrumWidth => Mapsui.Styles.Color.Purple,
                _ => Mapsui.Styles.Color.Gray
            };

            return new SymbolStyle
            {
                SymbolScale = 0.5,
                Fill = new Mapsui.Styles.Brush(color),
                Outline = new Mapsui.Styles.Pen(Mapsui.Styles.Color.Black, 1),
                Opacity = opacity
            };
        }

        /// <summary>
        /// Creates a style for radar station markers
        /// </summary>
        private IStyle CreateStationStyle()
        {
            return new SymbolStyle
            {
                SymbolScale = 0.8,
                Fill = new Mapsui.Styles.Brush(Mapsui.Styles.Color.Red),
                Outline = new Mapsui.Styles.Pen(Mapsui.Styles.Color.DarkRed, 2)
            };
        }

        /// <summary>
        /// Creates a style for weather warning polygons
        /// </summary>
        private IStyle CreateWarningStyle(WarningType warningType)
        {
            // Using correct enum values
            var color = warningType switch
            {
                WarningType.Tornado => Mapsui.Styles.Color.Red,
                WarningType.SevereThunderstorm => Mapsui.Styles.Color.Orange,
                WarningType.FloodWarning => Mapsui.Styles.Color.Green,
                _ => Mapsui.Styles.Color.Yellow
            };

            return new VectorStyle
            {
                Fill = new Mapsui.Styles.Brush(color),
                Outline = new Mapsui.Styles.Pen(color, 2),
                Opacity = 0.3f
            };
        }

        /// <summary>
        /// Centers the map on a location
        /// </summary>
        public void CenterOn(double longitude, double latitude, double? resolution = null)
        {
            try
            {
                if (_mapsuiControl?.Map?.Navigator != null)
                {
                    var mercatorCoords = SphericalMercator.FromLonLat(longitude, latitude);
                    var centerSphericalMercator = new Mapsui.MPoint(mercatorCoords.x, mercatorCoords.y);
                    
                    if (resolution.HasValue)
                    {
                        _mapsuiControl.Map.Navigator.CenterOnAndZoomTo(centerSphericalMercator, resolution.Value);
                    }
                    else
                    {
                        _mapsuiControl.Map.Navigator.CenterOn(centerSphericalMercator);
                    }
                    
                    Log($"Centered map on {latitude:F4}, {longitude:F4}");
                }
            }
            catch (Exception ex)
            {
                Log($"Error centering map: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Adds radar data to the map
        /// </summary>
        public void AddRadarLayer(RadarData radarData, string layerName, float opacity = 0.8f)
        {
            try
            {
                RemoveLayer(layerName);
                
                Log($"Adding radar layer '{layerName}' with {radarData.Gates.Count} gates");
                
                // Create memory provider for radar gates
                var features = new List<Mapsui.IFeature>();
                
                foreach (var gate in radarData.Gates)
                {
                    // Convert lat/lon to spherical mercator
                    var mercatorCoords = SphericalMercator.FromLonLat(gate.Longitude, gate.Latitude);
                    var point = new Mapsui.MPoint(mercatorCoords.x, mercatorCoords.y);
                    var feature = new Mapsui.Layers.PointFeature(point);
                    
                    // Add attributes using indexer syntax
                    feature["Value"] = gate.Value;
                    feature["ProductType"] = radarData.ProductType.ToString();
                    
                    features.Add(feature);
                }
                
                var memoryProvider = new Mapsui.Providers.MemoryProvider(features);
                var layer = new Layer("RadarData")
                {
                    Name = layerName,
                    DataSource = memoryProvider,
                    Style = CreateRadarStyle(radarData.ProductType, opacity),
                    Opacity = opacity
                };
                
                _mapsuiControl.Map.Layers.Add(layer);
                _customLayers[layerName] = layer;
                
                Log($"Radar layer '{layerName}' added successfully with {features.Count} features");
            }
            catch (Exception ex)
            {
                Log($"Error adding radar layer: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Adds station markers to the map
        /// </summary>
        public void AddStationMarkers(IEnumerable<RadarStation> stations, string layerName = "Stations")
        {
            try
            {
                RemoveLayer(layerName);
                
                var stationList = stations.ToList();
                Log($"Adding {stationList.Count} station markers");
                
                var features = new List<Mapsui.IFeature>();
                
                foreach (var station in stationList)
                {
                    var mercatorCoords = SphericalMercator.FromLonLat(station.Longitude, station.Latitude);
                    var point = new Mapsui.MPoint(mercatorCoords.x, mercatorCoords.y);
                    var feature = new Mapsui.Layers.PointFeature(point);
                    
                    // Using correct property name and indexer syntax
                    feature["StationId"] = station.Id;
                    feature["Name"] = station.Name;
                    feature["State"] = station.State;
                    
                    features.Add(feature);
                }
                
                var memoryProvider = new Mapsui.Providers.MemoryProvider(features);
                var layer = new Layer("Stations")
                {
                    Name = layerName,
                    DataSource = memoryProvider,
                    Style = CreateStationStyle()
                };
                
                _mapsuiControl.Map.Layers.Add(layer);
                _customLayers[layerName] = layer;
                
                Log($"Station markers layer added with {features.Count} stations");
            }
            catch (Exception ex)
            {
                Log($"Error adding station markers: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Adds warning polygons to the map
        /// </summary>
        public void AddWarningLayer(IEnumerable<WeatherWarning> warnings, string layerName = "Warnings")
        {
            try
            {
                RemoveLayer(layerName);
                
                var warningList = warnings.ToList();
                Log($"Adding {warningList.Count} weather warnings");
                
                var features = new List<Mapsui.IFeature>();
                
                foreach (var warning in warningList)
                {
                    // Use the Coordinates property which exists in WeatherWarning
                    if (warning.Coordinates != null && warning.Coordinates.Count > 0)
                    {
                        // Create point features from warning coordinates
                        foreach (var coordinate in warning.Coordinates)
                        {
                            var mercatorCoords = SphericalMercator.FromLonLat(coordinate.Item2, coordinate.Item1); // lon, lat
                            var point = new Mapsui.MPoint(mercatorCoords.x, mercatorCoords.y);
                            var feature = new Mapsui.Layers.PointFeature(point);
                            
                            // Using correct property names and indexer syntax
                            feature["WarningType"] = warning.Type.ToString();
                            feature["Description"] = warning.Description;
                            feature["ExpirationTime"] = warning.ExpiresTime.ToString();
                            
                            features.Add(feature);
                        }
                    }
                }
                
                var memoryProvider = new Mapsui.Providers.MemoryProvider(features);
                var layer = new Layer("Warnings")
                {
                    Name = layerName,
                    DataSource = memoryProvider,
                    Style = CreateWarningStyle(WarningType.Tornado) // Use a default style for now
                };
                
                _mapsuiControl.Map.Layers.Add(layer);
                _customLayers[layerName] = layer;
                
                Log($"Warning layer added with {features.Count} warning points");
            }
            catch (Exception ex)
            {
                Log($"Error adding warning layer: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Remove a layer by name
        /// </summary>
        public void RemoveLayer(string layerName)
        {
            try
            {
                if (_customLayers.TryGetValue(layerName, out var layer))
                {
                    _mapsuiControl.Map.Layers.Remove(layer);
                    _customLayers.Remove(layerName);
                    Log($"Removed layer: {layerName}");
                }
            }
            catch (Exception ex)
            {
                Log($"Error removing layer {layerName}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Refresh the map display
        /// </summary>
        public void RefreshMap()
        {
            try
            {
                _mapsuiControl?.Map?.RefreshData();
            }
            catch (Exception ex)
            {
                Log($"Error refreshing map: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Update opacity for all custom layers
        /// </summary>
        private void RefreshLayerOpacity()
        {
            try
            {
                foreach (var layer in _customLayers.Values)
                {
                    layer.Opacity = _opacity;
                }
                
                Log($"Layer opacity updated to {_opacity}");
                RefreshMap();
            }
            catch (Exception ex)
            {
                Log($"Error updating layer opacity: {ex.Message}");
            }
        }

        /// <summary>
        /// Zoom to fit all features in the current layers
        /// </summary>
        public void ZoomToFit()
        {
            try
            {
                if (_mapsuiControl?.Map != null && _customLayers.Any())
                {
                    // Use extent instead of envelope
                    var extent = _mapsuiControl.Map.Extent;
                    if (extent != null)
                    {
                        _mapsuiControl.Map.Navigator.ZoomToBox(extent);
                        Log("Zoomed to fit all features");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Error zooming to fit: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets the map type/style - ONLINE ONLY
        /// </summary>
        public void SetMapType(string mapType)
        {
            try
            {
                // Remove existing base layers
                var baseLayers = _mapsuiControl.Map.Layers.Where(l => l.Name.Contains("OpenStreetMap") || l.Name.Contains("Base")).ToList();
                foreach (var layer in baseLayers)
                {
                    _mapsuiControl.Map.Layers.Remove(layer);
                }

                // Add new ONLINE base layer based on type
                TileLayer newBaseLayer = null;
                switch (mapType.ToLower())
                {
                    case "street":
                    case "openstreetmap":
                    default:
                        newBaseLayer = CreateOpenStreetMapLayer();
                        break;
                    case "satellite":
                        // You could add satellite imagery here (e.g., Bing Maps, etc.)
                        newBaseLayer = CreateOpenStreetMapLayer(); // Fallback to OSM for now
                        break;
                    case "terrain":
                        // You could add terrain maps here
                        newBaseLayer = CreateOpenStreetMapLayer(); // Fallback to OSM for now
                        break;
                    case "dark":
                        // You could add dark theme maps here
                        newBaseLayer = CreateOpenStreetMapLayer(); // Fallback to OSM for now
                        break;
                }

                if (newBaseLayer != null)
                {
                    _mapsuiControl.Map.Layers.Insert(0, newBaseLayer); // Insert at bottom
                    Log($"Switched to {mapType} map type - ONLINE");
                }
            }
            catch (Exception ex)
            {
                Log($"Error setting map type to {mapType}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Log a message using the logger if available
        /// </summary>
        private void Log(string message)
        {
            try
            {
                _logger?.Invoke(message);
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }
}
