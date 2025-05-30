using System;
using System.IO;
using System.Threading.Tasks;

namespace NexradViewer.Services
{
    /// <summary>
    /// Provides map tiles for offline use
    /// </summary>
    public class MapTileProvider
    {
        private readonly string _cacheDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapTileProvider"/> class.
        /// </summary>
        public MapTileProvider()
        {
            _cacheDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MapData");
            
            // Ensure cache directory exists
            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
            }
        }

        /// <summary>
        /// Downloads map tiles for offline use
        /// </summary>
        public async Task<int> DownloadUsTiles(int minZoom, int maxZoom)
        {
            // Boundaries for continental US
            double minLat = 24.0; // Southern Florida
            double maxLat = 49.0; // Northern Washington
            double minLon = -125.0; // Western California
            double maxLon = -66.0; // Eastern Maine
            
            // In a real implementation, this would download tiles for the specified area
            // and store them in the cache directory
            
            // For now, we'll just simulate downloading tiles
            await Task.Delay(3000); // Simulate network delay
            
            // Calculate rough number of tiles (this is a simplification)
            int tilesCount = 0;
            for (int zoom = minZoom; zoom <= maxZoom; zoom++)
            {
                // Number of tiles increases exponentially with zoom level
                int factor = (int)Math.Pow(2, zoom);
                tilesCount += 10 * factor; // Just an estimate
            }
            
            // Create placeholder files to simulate downloaded tiles
            for (int i = 0; i < 5; i++)
            {
                string tilePath = Path.Combine(_cacheDirectory, $"tile_sample_{i}.png");
                if (!File.Exists(tilePath))
                {
                    using (File.Create(tilePath)) { }
                }
            }
            
            // Return the rough number of tiles that would be downloaded
            return tilesCount;
        }

        /// <summary>
        /// Gets a map tile for the specified coordinates and zoom level
        /// </summary>
        public string GetTilePath(double lat, double lon, int zoom)
        {
            // In a real implementation, this would convert from lat/lon to tile coordinates
            // and return the path to the cached tile if it exists
            
            // For now, we'll just return a placeholder
            return Path.Combine(_cacheDirectory, "placeholder.txt");
        }

        /// <summary>
        /// Calculates the tile size in degrees for a given zoom level
        /// </summary>
        public double GetTileSizeInDegrees(int zoom)
        {
            // At zoom level 0, the whole world is covered by one tile (360 degrees)
            // Each zoom level divides the tile size by 2
            return 360.0 / Math.Pow(2, zoom);
        }

        /// <summary>
        /// Gets the cache size
        /// </summary>
        public long GetCacheSize()
        {
            long size = 0;
            
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(_cacheDirectory);
                foreach (FileInfo file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    size += file.Length;
                }
            }
            catch (Exception)
            {
                // Ignore errors
            }
            
            return size;
        }

        /// <summary>
        /// Clears the tile cache
        /// </summary>
        public void ClearCache()
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(_cacheDirectory);
                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    file.Delete();
                }
                
                foreach (DirectoryInfo dir in dirInfo.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
            catch (Exception)
            {
                // Ignore errors
            }
        }
    }
}
