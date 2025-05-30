using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using NexradViewer.Models;
using NexradViewer.Services;
using NexradViewer.Controls;
using NexradViewer.Utils;

namespace NexradViewer
{
    // Partial class containing event handlers and data management methods
    public partial class MainWindow
    {
        private void StationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Extract station ID from selection
                var comboBox = sender as ComboBox;
                var selectionText = comboBox.SelectedItem as string;
                
                if (string.IsNullOrEmpty(selectionText)) return;
                
                // Extract station ID from format "Name (ID)"
                var startIndex = selectionText.LastIndexOf('(') + 1;
                var endIndex = selectionText.LastIndexOf(')');
                
                if (startIndex > 0 && endIndex > startIndex)
                {
                    string stationId = selectionText.Substring(startIndex, endIndex - startIndex);
                    
                    // Update selected station
                    _selectedStationId = stationId;
                    
                    // Center map on station
                    var station = _nexradService.GetStationById(stationId);
                    if (station != null)
                    {
                        _mapControl.CenterOn(station.Longitude, station.Latitude, 0.01);
                    }
                    
                    // Refresh radar data
                    _ = RefreshRadarData();
                }
            }
            catch (Exception ex)
            {
                UpdateStatusText($"Error changing station: {ex.Message}");
            }
        }
        
        private void ProductComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Get selected product
                var comboBox = sender as ComboBox;
                int selectedIndex = comboBox.SelectedIndex;
                
                if (selectedIndex >= 0)
                {
                    // Update selected product
                    _selectedProduct = GetProductFromComboBoxIndex(selectedIndex);
                    
                    // Refresh radar data for primary map
                    _ = RefreshRadarData();
                }
            }
            catch (Exception ex)
            {
                UpdateStatusText($"Error changing product: {ex.Message}");
            }
        }
        
        private void SecondProductComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Get selected product for side-by-side view
                var comboBox = sender as ComboBox;
                int selectedIndex = comboBox.SelectedIndex;
                
                if (selectedIndex >= 0)
                {
                    // Update second selected product
                    _secondSelectedProduct = GetProductFromComboBoxIndex(selectedIndex);
                    
                    // Refresh radar data for second map if side-by-side is enabled
                    if (_isSideBySideEnabled)
                    {
                        _ = RefreshSecondMapData();
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatusText($"Error changing second product: {ex.Message}");
            }
        }
        
        private async Task RefreshSecondMapData()
        {
            if (_isRefreshing) return;
            
            try
            {
                UpdateStatusText($"Loading {_secondSelectedProduct} data for second map...");
                
                // Get radar data for second product
                var scan = await _nexradService.GetLatestScanAsync(_selectedStationId, _secondSelectedProduct);
                
                if (scan != null && scan.Gates.Count > 0)
                {
                    // Use MapControl methods to add radar data
                    _secondMapControl.AddRadarLayer(scan, "Radar", _layerOpacity);
                    
                    // Add station marker
                    var stations = new List<RadarStation> { _nexradService.GetStationById(_selectedStationId) };
                    _secondMapControl.AddStationMarkers(stations, "Stations");
                    
                    UpdateStatusText($"Loaded {scan.Gates.Count} data points for second map");
                }
                else
                {
                    UpdateStatusText($"No radar data available for second product");
                }
            }
            catch (Exception ex)
            {
                UpdateStatusText($"Error refreshing second map data: {ex.Message}");
            }
        }
        
        private void RefreshIntervalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Get selected interval
                var comboBox = sender as ComboBox;
                var selectedItem = comboBox.SelectedItem as string;
                
                if (int.TryParse(selectedItem, out int minutes))
                {
                    _refreshInterval = minutes;
                }
            }
            catch (Exception ex)
            {
                UpdateStatusText($"Error changing refresh interval: {ex.Message}");
            }
        }
        
        private void WarningsToggle_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleSwitch;
            _showWarnings = toggle.IsChecked == true;
            
            // Refresh warnings layer
            _ = RefreshWeatherWarnings();
        }
        
        private void AutoRefreshToggle_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleSwitch;
            _autoRefresh = toggle.IsChecked == true;
        }
        
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshRadarData();
        }
        
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var popup = this.FindControl<Popup>("SettingsPopup");
            popup.IsOpen = true;
        }
        
        private void CloseSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var popup = this.FindControl<Popup>("SettingsPopup");
            popup.IsOpen = false;
        }
        
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateStatusText("Help feature - displays application documentation and usage instructions");
                // TODO: Implement help dialog or open documentation
            }
            catch (Exception ex)
            {
                UpdateStatusText($"Error opening help: {ex.Message}");
            }
        }
        
        private void DefaultSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Reset all settings to defaults
                _refreshInterval = 5;
                _layerOpacity = 0.8f;
                _selectedMapType = "Street";
                _autoLoadNationwide = false;
                _showWarnings = false;
                _autoRefresh = false;
                
                // Update UI controls
                var refreshIntervalComboBox = this.FindControl<ComboBox>("RefreshIntervalComboBox");
                var opacitySlider = this.FindControl<Slider>("OpacitySlider");
                var mapTypeComboBox = this.FindControl<ComboBox>("MapTypeComboBox");
                var autoStartNationwideCheckBox = this.FindControl<CheckBox>("AutoStartNationwideCheckBox");
                var warningsToggle = this.FindControl<ToggleSwitch>("WarningsToggle");
                var autoRefreshToggle = this.FindControl<ToggleSwitch>("AutoRefreshToggle");
                
                refreshIntervalComboBox.SelectedIndex = 1; // 5 minutes
                opacitySlider.Value = _layerOpacity;
                mapTypeComboBox.SelectedIndex = 0; // Street
                autoStartNationwideCheckBox.IsChecked = _autoLoadNationwide;
                warningsToggle.IsChecked = _showWarnings;
                autoRefreshToggle.IsChecked = _autoRefresh;
                
                UpdateStatusText("Settings reset to defaults");
            }
            catch (Exception ex)
            {
                UpdateStatusText($"Error resetting settings: {ex.Message}");
            }
        }
        
        private async void NationwideViewButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadNationwideView();
        }
        
        private void AlertCategoriesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateStatusText("Alert categories feature is not yet implemented");
            }
            catch (Exception ex)
            {
                UpdateStatusText($"Error opening alert categories: {ex.Message}");
            }
        }
        
        private void AutoStartNationwideCheckBox_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                var checkbox = sender as CheckBox;
                _autoLoadNationwide = checkbox.IsChecked == true;
            }
            catch (Exception ex)
            {
                UpdateConsoleText($"Error changing auto-load setting: {ex.Message}");
            }
        }
        
        private async Task LoadNationwideView()
        {
            try
            {
                UpdateStatusText("Loading nationwide radar view...");
                
                // Set map to nationwide view
                _mapControl.CenterOn(-97.0, 38.5, 0.05);
                
                // Load composite nationwide radar data
                var nationwideData = await _nexradService.GetNationwideCompositeAsync();
                
                // Use MapControl methods to add radar data
                _mapControl.AddRadarLayer(nationwideData, "Radar", _layerOpacity);
                
                // Add all station markers
                var allStations = _nexradService.GetStations();
                _mapControl.AddStationMarkers(allStations, "Stations");
                
                UpdateStatusText("Loaded nationwide radar composite");
                
                // Refresh warnings if enabled
                if (_showWarnings)
                {
                    await RefreshWeatherWarnings();
                }
            }
            catch (Exception ex)
            {
                UpdateStatusText($"Error loading nationwide view: {ex.Message}");
            }
        }
        
        private async void DownloadMapButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                button.IsEnabled = false;
                button.Content = "Downloading...";
                
                try 
                {
                    UpdateStatusText("Starting US map download for offline use...");
                    await DownloadMapTilesAsync();
                }
                finally
                {
                    button.Content = "Download US Map";
                    button.IsEnabled = true;
                }
            }
        }
        
        private async void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_autoRefresh && !_isRefreshing)
            {
                // Check if it's time to refresh
                var lastRefresh = DateTime.Now - LastRefreshTime;
                if (lastRefresh.TotalMinutes >= _refreshInterval)
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await RefreshRadarData();
                    });
                }
            }
        }
        
        /// <summary>
        /// Refresh radar data
        /// </summary>
        private async Task RefreshRadarData()
        {
            if (_isRefreshing) return;
            
            try
            {
                _isRefreshing = true;
                Logger.LogInfo($"=== REFRESH RADAR DATA START ===");
                Logger.LogInfo($"Station: {_selectedStationId}, Product: {_selectedProduct}");
                UpdateStatusText($"Loading radar data for {_selectedStationId}...");
                
                // Check if map control is available
                if (_mapControl == null)
                {
                    Logger.LogError("MapControl is null - cannot display radar data");
                    UpdateStatusText("Error: Map control not available");
                    return;
                }
                
                Logger.LogInfo($"MapControl available, map object: {(_mapControl.Map != null ? "Available" : "NULL")}");
                
                // Get radar data
                Logger.LogInfo("Calling NexradService.GetLatestScanAsync...");
                var scan = await _nexradService.GetLatestScanAsync(_selectedStationId, _selectedProduct);
                
                if (scan != null)
                {
                    Logger.LogInfo($"Scan returned: StationId={scan.StationId}, ProductType={scan.ProductType}, Gates={scan.Gates?.Count ?? 0}, Mode={scan.Mode}");
                    
                    if (scan.Gates != null && scan.Gates.Count > 0)
                    {
                        Logger.LogInfo($"Sample gate data: Lat={scan.Gates[0].Latitude}, Lon={scan.Gates[0].Longitude}, Value={scan.Gates[0].Value}");
                        
                        // Use MapControl methods to add radar data
                        Logger.LogInfo("Calling AddRadarLayer...");
                        _mapControl.AddRadarLayer(scan, "Radar", _layerOpacity);
                        
                        // Add station marker
                        var station = _nexradService.GetStationById(_selectedStationId);
                        if (station != null)
                        {
                            Logger.LogInfo($"Adding station marker: {station.Name} at {station.Latitude}, {station.Longitude}");
                            var stations = new List<RadarStation> { station };
                            _mapControl.AddStationMarkers(stations, "Stations");
                        }
                        else
                        {
                            Logger.LogWarning($"Station {_selectedStationId} not found in station list");
                        }
                        
                        // Update status
                        UpdateStatusText($"Loaded {scan.Gates.Count} data points at {DateTime.Now:HH:mm:ss}");
                        LastRefreshTime = DateTime.Now;
                        Logger.LogInfo($"Radar data loaded successfully: {scan.Gates.Count} gates");
                    }
                    else
                    {
                        Logger.LogWarning($"Scan returned but gates are null or empty. Gates null: {scan.Gates == null}, Count: {scan.Gates?.Count ?? -1}");
                        UpdateStatusText($"No radar data available for {_selectedStationId} - empty scan");
                    }
                }
                else
                {
                    Logger.LogError($"GetLatestScanAsync returned null for station {_selectedStationId}, product {_selectedProduct}");
                    UpdateStatusText($"No radar data available for {_selectedStationId} - null scan");
                }
                
                // Refresh warnings if needed
                if (_showWarnings)
                {
                    Logger.LogInfo("Refreshing weather warnings...");
                    await RefreshWeatherWarnings();
                }
                
                Logger.LogInfo("=== REFRESH RADAR DATA END ===");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "RefreshRadarData");
                UpdateStatusText($"Error refreshing radar data: {ex.Message}");
            }
            finally
            {
                _isRefreshing = false;
            }
        }
        
        /// <summary>
        /// Refresh weather warnings
        /// </summary>
        private async Task RefreshWeatherWarnings()
        {
            try
            {
                if (_showWarnings)
                {
                    UpdateStatusText("Loading weather warnings...");
                    
                    // Get warnings
                    var warnings = await _nexradService.GetActiveWarningsAsync();
                    
                    // Use MapControl methods to add warning data
                    _mapControl.AddWarningLayer(warnings, "Warnings");
                    
                    if (_isSideBySideEnabled)
                    {
                        _secondMapControl.AddWarningLayer(warnings, "Warnings");
                    }
                    
                    UpdateStatusText($"Loaded {warnings.Count} weather warnings");
                }
                else
                {
                    // Clear warnings layer
                    _mapControl.RemoveLayer("Warnings");
                    
                    if (_isSideBySideEnabled)
                    {
                        _secondMapControl.RemoveLayer("Warnings");
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatusText($"Error refreshing weather warnings: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Download US map tiles for offline use
        /// </summary>
        private async Task<bool> DownloadMapTilesAsync()
        {
            try
            {
                UpdateStatusText("Starting US map tiles download...");
                
                var mapProvider = new MapTileProvider();
                int tilesDownloaded = await mapProvider.DownloadUsTiles(3, 8);
                
                UpdateStatusText($"Downloaded {tilesDownloaded} map tiles for offline use");
                return true;
            }
            catch (Exception ex)
            {
                UpdateStatusText($"Error downloading map tiles: {ex.Message}");
                return false;
            }
        }
        
        private void SideBySideToggle_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                var toggle = sender as ToggleSwitch;
                _isSideBySideEnabled = toggle.IsChecked == true;
                
                // Show/hide second product panel and map
                var secondProductPanel = this.FindControl<StackPanel>("SecondProductPanel");
                var grid = this.FindControl<Grid>("MainGrid");
                var secondMapControl = this.FindControl<MapControl>("SecondMapControl");
                
                secondProductPanel.IsVisible = _isSideBySideEnabled;
                
                // Adjust the column width for the second map
                if (grid != null && grid.ColumnDefinitions.Count > 1)
                {
                    grid.ColumnDefinitions[1].Width = _isSideBySideEnabled ? 
                        new GridLength(1, GridUnitType.Star) : 
                        new GridLength(0, GridUnitType.Pixel);
                }
                
                secondMapControl.IsVisible = _isSideBySideEnabled;
                
                if (_isSideBySideEnabled)
                {
                    // Copy current view to second map
                    var station = _nexradService.GetStationById(_selectedStationId);
                    if (station != null)
                    {
                        _secondMapControl.CenterOn(station.Longitude, station.Latitude, 0.01);
                    }
                    
                    // Load data for second map
                    _ = RefreshSecondMapData();
                }
            }
            catch (Exception ex)
            {
                UpdateStatusText($"Error toggling side-by-side view: {ex.Message}");
            }
        }
        
        private void MapTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var comboBox = sender as ComboBox;
                var selectedItem = comboBox.SelectedItem as ComboBoxItem;
                if (selectedItem != null)
                {
                    _selectedMapType = selectedItem.Content.ToString();
                    UpdateStatusText($"Changed map type to {_selectedMapType}");
                }
            }
            catch (Exception ex)
            {
                UpdateStatusText($"Error changing map type: {ex.Message}");
            }
        }
        
        private void OpacitySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            try
            {
                var slider = sender as Slider;
                _layerOpacity = (float)slider.Value;
                
                // Update opacity in map controls
                if (_mapControl != null)
                {
                    _mapControl.Opacity = _layerOpacity;
                }
                
                if (_isSideBySideEnabled && _secondMapControl != null)
                {
                    _secondMapControl.Opacity = _layerOpacity;
                }
            }
            catch (Exception ex)
            {
                UpdateConsoleText($"Error changing opacity: {ex.Message}");
            }
        }
    }
}
