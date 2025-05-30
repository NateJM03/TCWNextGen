using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using NexradViewer.Models;
using NexradViewer.Services;
using NexradViewer.Controls;
using NexradViewer.Utils;
using Color = Avalonia.Media.Color;

namespace NexradViewer
{
    public partial class MainWindow : Window
    {
        // Services
        private readonly NexradService _nexradService;
        private readonly ConfigurationService _config;
        
        // Map controls
        private MapControl _mapControl;
        private MapControl _secondMapControl;
        
        // Timers and state
        private readonly Timer _refreshTimer;
        private bool _isRefreshing = false;
        
        // Map controls and views
        private bool _isSideBySideEnabled = false;
        private float _layerOpacity = 0.8f;
        private string _selectedMapType = "Street";
        
        // Current selection
        private string _selectedStationId = "KFWS"; // Default to Dallas/Fort Worth
        private RadarProductType _selectedProduct = RadarProductType.BaseReflectivity;
        private RadarProductType _secondSelectedProduct = RadarProductType.BaseVelocity;
        private bool _showWarnings = false;
        private bool _autoRefresh = false;
        private int _refreshInterval = 5; // minutes
        private bool _autoLoadNationwide = false; // Explicitly disabled by default
        
        // Weather warning filter flags
        private bool _showTornadoWarnings = true;
        private bool _showThunderstormWarnings = true;
        private bool _showFloodWarnings = true;
        private bool _showWinterWarnings = true;
        private bool _showTornadoWatches = true;
        private bool _showThunderstormWatches = true;
        private bool _showFloodWatches = true;
        private bool _showWinterWatches = true;
        private bool _showSpecialStatements = true;
        private bool _showHazardousOutlooks = true;
        
        // Time comparison settings
        private List<DateTime> _availableTimeScans = new List<DateTime>();
        private Timer _animationTimer;
        private int _currentAnimationFrame = 0;
        private bool _isPlayingAnimation = false;

        private bool _isInitialized = false;

        // Track last refresh time
        private DateTime LastRefreshTime = DateTime.MinValue;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            
            // Initialize logger first
            Logger.Initialize();
            Logger.LogInfo("=== APPLICATION STARTUP ===");
            Logger.LogInfo("MainWindow constructor started");
            
            try
            {
                // Initialize services
                Logger.LogInfo("Initializing ConfigurationService");
                _config = new ConfigurationService();
                
                Logger.LogInfo("Initializing NexradService");
                _nexradService = new NexradService(logger: UpdateConsoleText, config: _config);
                
                // Load configuration settings
                _refreshInterval = _config.DefaultRefreshInterval;
                _layerOpacity = (float)_config.DefaultLayerOpacity;
                _selectedMapType = _config.DefaultMapType;
                _autoLoadNationwide = _config.AutoLoadNationwide;
                
                Logger.LogInfo($"Configuration loaded - RefreshInterval: {_refreshInterval}, LayerOpacity: {_layerOpacity}, MapType: {_selectedMapType}, AutoLoadNationwide: {_autoLoadNationwide}");
                
                // Setup refresh timer
                _refreshTimer = new Timer(60000); // 1 minute checks
                _refreshTimer.Elapsed += RefreshTimer_Elapsed;
                _refreshTimer.AutoReset = true;
                _refreshTimer.Start();
                
                Logger.LogInfo("Refresh timer initialized and started");
                
                // Setup initial UI state
                Loaded += MainWindow_Loaded;
                
                Logger.LogInfo("MainWindow constructor completed successfully");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "MainWindow constructor");
                throw;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isInitialized) return;
            _isInitialized = true;
            
            try
            {
                _mapControl = this.FindControl<MapControl>("MapControl");
                _secondMapControl = this.FindControl<MapControl>("SecondMapControl");
                
                // Initialize map controls
                if (_mapControl?.Map != null)
                {
                    // Center on continental US
                    _mapControl.CenterOn(-97.0, 38.5, 0.05);
                }
                
                if (_secondMapControl?.Map != null)
                {
                    // Center on continental US
                    _secondMapControl.CenterOn(-97.0, 38.5, 0.05);
                }

                // Initialize UI controls
                var stationComboBox = this.FindControl<ComboBox>("StationComboBox");
                var productComboBox = this.FindControl<ComboBox>("ProductComboBox");
                var secondProductComboBox = this.FindControl<ComboBox>("SecondProductComboBox");
                var sideBySideToggle = this.FindControl<ToggleSwitch>("SideBySideToggle");
                var secondProductPanel = this.FindControl<StackPanel>("SecondProductPanel");
                var mapTypeComboBox = this.FindControl<ComboBox>("MapTypeComboBox");
                var opacitySlider = this.FindControl<Slider>("OpacitySlider");
                var warningsToggle = this.FindControl<ToggleSwitch>("WarningsToggle");
                var autoRefreshToggle = this.FindControl<ToggleSwitch>("AutoRefreshToggle");
                var refreshIntervalComboBox = this.FindControl<ComboBox>("RefreshIntervalComboBox");
                var refreshButton = this.FindControl<Button>("RefreshButton");
                var nationwideViewButton = this.FindControl<Button>("NationwideViewButton");
                var downloadMapButton = this.FindControl<Button>("DownloadMapButton");
                var alertCategoriesButton = this.FindControl<Button>("AlertCategoriesButton");
                var helpButton = this.FindControl<Button>("HelpButton");
                var settingsButton = this.FindControl<Button>("SettingsButton");
                var closeSettingsButton = this.FindControl<Button>("CloseSettingsButton");
                var defaultSettingsButton = this.FindControl<Button>("DefaultSettingsButton");
                var autoStartNationwideCheckBox = this.FindControl<CheckBox>("AutoStartNationwideCheckBox");
                var enableSmoothPanningCheckBox = this.FindControl<CheckBox>("EnableSmoothPanningCheckBox");
                var settingsPopup = this.FindControl<Popup>("SettingsPopup");
                var statusText = this.FindControl<TextBlock>("StatusText");
                
                // Connect event handlers
                refreshButton.Click += RefreshButton_Click;
                downloadMapButton.Click += DownloadMapButton_Click;
                nationwideViewButton.Click += NationwideViewButton_Click;
                alertCategoriesButton.Click += AlertCategoriesButton_Click;
                helpButton.Click += HelpButton_Click;
                settingsButton.Click += SettingsButton_Click;
                closeSettingsButton.Click += CloseSettingsButton_Click;
                defaultSettingsButton.Click += DefaultSettingsButton_Click;
                warningsToggle.IsCheckedChanged += WarningsToggle_IsCheckedChanged;
                autoRefreshToggle.IsCheckedChanged += AutoRefreshToggle_IsCheckedChanged;
                sideBySideToggle.IsCheckedChanged += SideBySideToggle_IsCheckedChanged;
                mapTypeComboBox.SelectionChanged += MapTypeComboBox_SelectionChanged;
                opacitySlider.ValueChanged += OpacitySlider_ValueChanged;
                
                // Initialize settings
                autoStartNationwideCheckBox.IsChecked = _autoLoadNationwide;
                autoStartNationwideCheckBox.IsCheckedChanged += AutoStartNationwideCheckBox_IsCheckedChanged;
                
                // Load stations
                var stations = _nexradService.GetStations();
                stationComboBox.ItemsSource = stations.OrderBy(s => s.Name).Select(s => s.Name + " (" + s.Id + ")").ToList();
                stationComboBox.SelectionChanged += StationComboBox_SelectionChanged;
                
                // Select default station
                var defaultStationIndex = stations.FindIndex(s => s.Id == _selectedStationId);
                if (defaultStationIndex >= 0)
                {
                    stationComboBox.SelectedIndex = defaultStationIndex;
                }
                else if (stations.Count > 0)
                {
                    stationComboBox.SelectedIndex = 0;
                }
                
                // Setup product dropdown with all available products
                productComboBox.Items.Clear();
                PopulateProductComboBox(productComboBox);
                productComboBox.SelectedIndex = 0;
                productComboBox.SelectionChanged += ProductComboBox_SelectionChanged;
                
                // Setup second product dropdown
                secondProductComboBox.Items.Clear();
                PopulateProductComboBox(secondProductComboBox);
                secondProductComboBox.SelectedIndex = 1; // Default to Velocity for comparison
                secondProductComboBox.SelectionChanged += SecondProductComboBox_SelectionChanged;
                
                // Initialize map type
                mapTypeComboBox.SelectedIndex = 0; // Street map by default
                
                // Set initial opacity
                opacitySlider.Value = _layerOpacity;
                
                // Setup refresh interval dropdown
                refreshIntervalComboBox.Items.Clear();
                refreshIntervalComboBox.Items.Add("1");
                refreshIntervalComboBox.Items.Add("5");
                refreshIntervalComboBox.Items.Add("10");
                refreshIntervalComboBox.Items.Add("15");
                refreshIntervalComboBox.Items.Add("30");
                refreshIntervalComboBox.SelectedIndex = 1; // Default to 5 minutes
                refreshIntervalComboBox.SelectionChanged += RefreshIntervalComboBox_SelectionChanged;
                
                // Decide on initial data load
                UpdateStatusText("Initializing application...");
                
                if (_autoLoadNationwide)
                {
                    await LoadNationwideView();
                }
                else
                {
                    await RefreshRadarData();
                }
            }
            catch (Exception ex)
            {
                UpdateStatusText($"Error initializing app: {ex.Message}");
            }
        }

        private void PopulateProductComboBox(ComboBox comboBox)
        {
            // Add Level 2 products
            comboBox.Items.Add("Base Reflectivity");
            comboBox.Items.Add("Base Velocity");
            comboBox.Items.Add("Spectrum Width");
            comboBox.Items.Add("Correlation Coefficient");
            comboBox.Items.Add("Differential Reflectivity");
            comboBox.Items.Add("Differential Phase");
            comboBox.Items.Add("Specific Differential Phase");
            
            // Add Level 3 Reflectivity products
            comboBox.Items.Add("Digital Reflectivity (0.5°)");
            comboBox.Items.Add("Digital Reflectivity (1.5°)");
            comboBox.Items.Add("Digital Reflectivity (2.4°)");
            comboBox.Items.Add("Composite Reflectivity");
            
            // Add Level 3 Velocity products
            comboBox.Items.Add("Digital Velocity (0.5°)");
            comboBox.Items.Add("Storm Relative Motion");
            comboBox.Items.Add("VAD Wind Profile");
            
            // Add Level 3 Precipitation products
            comboBox.Items.Add("One-Hour Precipitation");
            comboBox.Items.Add("Storm Total Precipitation");
            comboBox.Items.Add("Digital Precipitation Array");
            comboBox.Items.Add("Hydrometeor Classification");
            comboBox.Items.Add("Vertically Integrated Liquid");
            
            // Add Level 3 Severe Weather products
            comboBox.Items.Add("Echo Tops");
            comboBox.Items.Add("Mesocyclone Detection");
            comboBox.Items.Add("Tornadic Vortex Signature");
            
            // Add other weather products
            comboBox.Items.Add("GOES Visible Imagery");
            comboBox.Items.Add("GOES Infrared Imagery");
            comboBox.Items.Add("Surface Temperature");
            comboBox.Items.Add("Wind Field");
            comboBox.Items.Add("Cloud Cover");
        }
        
        private Models.RadarProductType GetProductFromComboBoxIndex(int index)
        {
            switch (index)
            {
                case 0: return Models.RadarProductType.BaseReflectivity;
                case 1: return Models.RadarProductType.BaseVelocity;
                case 2: return Models.RadarProductType.SpectrumWidth;
                case 3: return Models.RadarProductType.CorrelationCoefficient;
                case 4: return Models.RadarProductType.DifferentialReflectivity;
                case 5: return Models.RadarProductType.SpecificDifferentialPhase;
                case 6: return Models.RadarProductType.SpecificDifferentialPhase; // KDP
                case 7: return Models.RadarProductType.BaseReflectivity; // Digital Reflectivity 0.5°
                case 8: return Models.RadarProductType.BaseReflectivity; // Digital Reflectivity 1.5°
                case 9: return Models.RadarProductType.BaseReflectivity; // Digital Reflectivity 2.4°
                case 10: return Models.RadarProductType.CompositeReflectivity;
                case 11: return Models.RadarProductType.BaseVelocity; // Digital Velocity 0.5°
                case 12: return Models.RadarProductType.StormRelativeMeanVelocity;
                case 13: return Models.RadarProductType.VADWindProfile;
                case 14: return Models.RadarProductType.OneHourAccumulation;
                case 15: return Models.RadarProductType.StormTotalPrecipitation;
                case 16: return Models.RadarProductType.DigitalPrecipitationArray;
                case 17: return Models.RadarProductType.HydrometeorClassification;
                case 18: return Models.RadarProductType.BaseReflectivity; // VIL - not in enum, use reflectivity
                case 19: return Models.RadarProductType.EnhancedEchoTops;
                case 20: return Models.RadarProductType.MesocycloneDetection;
                case 21: return Models.RadarProductType.TornadicVortexSignature;
                case 22: return Models.RadarProductType.BaseReflectivity; // GOES Visible - not in enum
                case 23: return Models.RadarProductType.BaseReflectivity; // GOES Infrared - not in enum
                case 24: return Models.RadarProductType.BaseReflectivity; // Surface Temperature - not in enum
                case 25: return Models.RadarProductType.BaseReflectivity; // Wind Field - not in enum
                case 26: return Models.RadarProductType.BaseReflectivity; // Cloud Cover - not in enum
                default: return Models.RadarProductType.BaseReflectivity;
            }
        }

        /// <summary>
        /// Update status text
        /// </summary>
        private void UpdateConsoleText(string message)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var consoleText = this.FindControl<TextBlock>("ConsoleText");
                if (consoleText != null)
                {
                    consoleText.Text += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
                }
            });
        }
        
        private void UpdateStatusText(string message)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var statusText = this.FindControl<TextBlock>("StatusText");
                if (statusText != null)
                {
                    statusText.Text = $"Status: {message}";
                }
                
                // Also update console for important messages
                UpdateConsoleText(message);
            });
        }
    }
}
