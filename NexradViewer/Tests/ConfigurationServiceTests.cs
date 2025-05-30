using System;
using System.Collections.Generic;
using System.Text;
using NexradViewer.Services;

namespace NexradViewer.Utils
{
    /// <summary>
    /// Helper class to validate configuration settings.
    /// This can be used for diagnostic purposes.
    /// </summary>
    public class ConfigurationValidator
    {
        private readonly ConfigurationService _config;
        private readonly List<string> _results = new List<string>();

        public ConfigurationValidator(ConfigurationService config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Validates all configuration settings
        /// </summary>
        /// <returns>A report of validation results</returns>
        public string ValidateAll()
        {
            // Clear previous results
            _results.Clear();
            
            // Validate core settings
            _results.Add($"BrandingText: {_config.BrandingText}");
            _results.Add($"DefaultZoomLevel: {_config.DefaultZoomLevel}");
            _results.Add($"DefaultRefreshInterval: {_config.DefaultRefreshInterval}");
            _results.Add($"AutoLoadNationwide: {_config.AutoLoadNationwide}");
            
            // Validate data settings
            _results.Add($"Level2BucketName: {_config.Level2BucketName}");
            _results.Add($"Level2ChunksBucketName: {_config.Level2ChunksBucketName}");
            _results.Add($"Level3BucketName: {_config.Level3BucketName}");
            _results.Add($"MaxStationsForComposite: {_config.MaxStationsForComposite}");
            _results.Add($"StationCacheDuration: {_config.StationCacheDuration}");
            _results.Add($"WarningCacheDuration: {_config.WarningCacheDuration}");
            
            // Validate map settings
            _results.Add($"DefaultMapType: {_config.DefaultMapType}");
            _results.Add($"DefaultLayerOpacity: {_config.DefaultLayerOpacity}");
            _results.Add($"UseGPUAcceleration: {_config.UseGPUAcceleration}");
            _results.Add($"TileSize: {_config.TileSize}");
            _results.Add($"LocalMBTilesPath: {_config.LocalMBTilesPath}");
            
            // Validate playback settings
            _results.Add($"PlaybackFrameCount: {_config.PlaybackFrameCount}");
            _results.Add($"PlaybackDefaultSpeed: {_config.PlaybackDefaultSpeed}");
            
            // Validate UI settings
            _results.Add($"DefaultTheme: {_config.DefaultTheme}");
            _results.Add($"UseCustomFont: {_config.UseCustomFont}");
            _results.Add($"CustomFontName: {_config.CustomFontName}");
            _results.Add($"EnableSmoothPanning: {_config.EnableSmoothPanning}");
            
            // Verify helper methods
            ValidateHelperMethods();
            
            // Return a formatted report
            return string.Join(Environment.NewLine, _results);
        }
        
        /// <summary>
        /// Validates the helper methods of the configuration service
        /// </summary>
        private void ValidateHelperMethods()
        {
            _results.Add("");
            _results.Add("HELPER METHODS VALIDATION:");
            
            // Test GetString
            var brandingText = _config.GetString("BrandingText");
            _results.Add($"GetString(\"BrandingText\"): {brandingText}");
            
            // Test GetString with default
            var nonExistent = _config.GetString("NonExistentKey", "DefaultValue");
            _results.Add($"GetString(\"NonExistentKey\", \"DefaultValue\"): {nonExistent}");
            
            // Test GetInt
            var zoomLevel = _config.GetInt("DefaultZoomLevel");
            _results.Add($"GetInt(\"DefaultZoomLevel\"): {zoomLevel}");
            
            // Test GetDouble
            var opacity = _config.GetDouble("DefaultLayerOpacity");
            _results.Add($"GetDouble(\"DefaultLayerOpacity\"): {opacity}");
            
            // Test GetBool
            var useGPU = _config.GetBool("UseGPUAcceleration");
            _results.Add($"GetBool(\"UseGPUAcceleration\"): {useGPU}");
        }
    }
}
