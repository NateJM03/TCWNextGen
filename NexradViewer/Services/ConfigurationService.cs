using System;
using System.Configuration;
using System.Drawing;

namespace NexradViewer.Services
{
    /// <summary>
    /// Service for accessing application configuration settings
    /// </summary>
    public class ConfigurationService
    {
        /// <summary>
        /// Gets a configuration value as string
        /// </summary>
        public string GetString(string key, string defaultValue = "")
        {
            try
            {
                string value = ConfigurationManager.AppSettings[key];
                return string.IsNullOrEmpty(value) ? defaultValue : value;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets a configuration value as integer
        /// </summary>
        public int GetInt(string key, int defaultValue = 0)
        {
            try
            {
                string value = ConfigurationManager.AppSettings[key];
                return int.TryParse(value, out int result) ? result : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets a configuration value as double
        /// </summary>
        public double GetDouble(string key, double defaultValue = 0.0)
        {
            try
            {
                string value = ConfigurationManager.AppSettings[key];
                return double.TryParse(value, out double result) ? result : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets a configuration value as boolean
        /// </summary>
        public bool GetBool(string key, bool defaultValue = false)
        {
            try
            {
                string value = ConfigurationManager.AppSettings[key];
                return bool.TryParse(value, out bool result) ? result : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        #region App Settings Properties

        /// <summary>
        /// Gets the branding text for the application
        /// </summary>
        public string BrandingText => GetString("BrandingText", "TheClearWeather | NextGenRadar");

        /// <summary>
        /// Gets the default zoom level for the map
        /// </summary>
        public int DefaultZoomLevel => GetInt("DefaultZoomLevel", 5);

        /// <summary>
        /// Gets the default refresh interval in minutes
        /// </summary>
        public int DefaultRefreshInterval => GetInt("DefaultRefreshInterval", 5);

        /// <summary>
        /// Gets whether to auto-load the nationwide view on startup
        /// </summary>
        public bool AutoLoadNationwide => GetBool("AutoLoadNationwide", false);

        /// <summary>
        /// Gets the Level 2 bucket name
        /// </summary>
        public string Level2BucketName => GetString("Level2BucketName", "noaa-nexrad-level2");

        /// <summary>
        /// Gets the Level 2 chunks bucket name
        /// </summary>
        public string Level2ChunksBucketName => GetString("Level2ChunksBucketName", "unidata-nexrad-level2-chunks");

        /// <summary>
        /// Gets the Level 3 bucket name
        /// </summary>
        public string Level3BucketName => GetString("Level3BucketName", "noaa-nexrad-level3");

        /// <summary>
        /// Gets the maximum number of stations to include in the nationwide composite
        /// </summary>
        public int MaxStationsForComposite => GetInt("MaxStationsForComposite", 160);

        /// <summary>
        /// Gets the station cache duration in minutes
        /// </summary>
        public int StationCacheDuration => GetInt("StationCacheDuration", 1440);

        /// <summary>
        /// Gets the warning cache duration in minutes
        /// </summary>
        public int WarningCacheDuration => GetInt("WarningCacheDuration", 5);

        /// <summary>
        /// Gets the default map type
        /// </summary>
        public string DefaultMapType => GetString("DefaultMapType", "Street");

        /// <summary>
        /// Gets the default layer opacity
        /// </summary>
        public double DefaultLayerOpacity => GetDouble("DefaultLayerOpacity", 0.8);

        /// <summary>
        /// Gets whether to use GPU acceleration
        /// </summary>
        public bool UseGPUAcceleration => GetBool("UseGPUAcceleration", true);

        /// <summary>
        /// Gets the tile size for map rendering
        /// </summary>
        public int TileSize => GetInt("TileSize", 512);

        /// <summary>
        /// Gets the local MBTiles path
        /// </summary>
        public string LocalMBTilesPath => GetString("LocalMBTilesPath", "MapData");

        /// <summary>
        /// Gets the number of frames to cache for playback
        /// </summary>
        public int PlaybackFrameCount => GetInt("PlaybackFrameCount", 10);

        /// <summary>
        /// Gets the default playback speed in milliseconds
        /// </summary>
        public int PlaybackDefaultSpeed => GetInt("PlaybackDefaultSpeed", 500);

        /// <summary>
        /// Gets the default theme
        /// </summary>
        public string DefaultTheme => GetString("DefaultTheme", "Dark");

        /// <summary>
        /// Gets whether to use the custom font
        /// </summary>
        public bool UseCustomFont => GetBool("UseCustomFont", true);

        /// <summary>
        /// Gets the custom font name
        /// </summary>
        public string CustomFontName => GetString("CustomFontName", "Nebula Sans");

        /// <summary>
        /// Gets whether to enable smooth panning
        /// </summary>
        public bool EnableSmoothPanning => GetBool("EnableSmoothPanning", true);

        #endregion
    }
}
