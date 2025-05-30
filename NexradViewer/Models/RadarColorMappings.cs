using System;
using System.Collections.Generic;
using System.Drawing;

namespace NexradViewer.Models
{
    /// <summary>
    /// Contains color mappings for radar product types
    /// </summary>
    public static class RadarColorMappings
    {
        /// <summary>
        /// Maps product types to their default color scales
        /// </summary>
        public static readonly Dictionary<RadarProductType, RadarColorScale> DefaultColorScales = new Dictionary<RadarProductType, RadarColorScale>
        {
            { RadarProductType.BaseReflectivity, new RadarColorScale(
                new List<RadarColorStop>
                {
                    new RadarColorStop(-30, Color.FromArgb(0, 0, 0, 0)),    // Transparent
                    new RadarColorStop(-10, Color.FromArgb(100, 120, 120, 120)), // Light gray
                    new RadarColorStop(0, Color.FromArgb(255, 0, 0, 235)),    // Blue
                    new RadarColorStop(10, Color.FromArgb(255, 0, 235, 235)),  // Cyan
                    new RadarColorStop(20, Color.FromArgb(255, 0, 235, 0)),    // Green
                    new RadarColorStop(30, Color.FromArgb(255, 235, 235, 0)),  // Yellow
                    new RadarColorStop(40, Color.FromArgb(255, 235, 120, 0)),  // Orange
                    new RadarColorStop(50, Color.FromArgb(255, 235, 0, 0)),    // Red
                    new RadarColorStop(60, Color.FromArgb(255, 200, 0, 200)),  // Purple
                    new RadarColorStop(70, Color.FromArgb(255, 255, 255, 255)) // White
                })
            },
            { RadarProductType.BaseVelocity, new RadarColorScale(
                new List<RadarColorStop>
                {
                    new RadarColorStop(-70, Color.FromArgb(255, 255, 255, 255)), // White
                    new RadarColorStop(-50, Color.FromArgb(255, 100, 0, 255)),  // Purple
                    new RadarColorStop(-32, Color.FromArgb(255, 0, 0, 255)),     // Blue
                    new RadarColorStop(-16, Color.FromArgb(255, 0, 128, 255)),   // Light blue
                    new RadarColorStop(-8, Color.FromArgb(255, 0, 255, 128)),    // Light green
                    new RadarColorStop(-2, Color.FromArgb(255, 0, 200, 0)),      // Green
                    new RadarColorStop(0, Color.FromArgb(160, 180, 180, 180)),   // Gray
                    new RadarColorStop(2, Color.FromArgb(255, 200, 200, 0)),     // Yellow
                    new RadarColorStop(8, Color.FromArgb(255, 255, 128, 0)),    // Orange
                    new RadarColorStop(16, Color.FromArgb(255, 255, 0, 0)),      // Red
                    new RadarColorStop(32, Color.FromArgb(255, 200, 0, 200)),    // Purple
                    new RadarColorStop(50, Color.FromArgb(255, 255, 255, 255))   // White
                })
            },
            { RadarProductType.CorrelationCoefficient, new RadarColorScale(
                new List<RadarColorStop>
                {
                    new RadarColorStop(0.0, Color.FromArgb(255, 0, 0, 0)),       // Black
                    new RadarColorStop(0.7, Color.FromArgb(255, 255, 0, 0)),     // Red
                    new RadarColorStop(0.8, Color.FromArgb(255, 255, 128, 0)),   // Orange
                    new RadarColorStop(0.9, Color.FromArgb(255, 255, 255, 0)),   // Yellow
                    new RadarColorStop(0.95, Color.FromArgb(255, 0, 255, 0)),    // Green
                    new RadarColorStop(0.98, Color.FromArgb(255, 0, 128, 255)),  // Light blue
                    new RadarColorStop(1.0, Color.FromArgb(255, 0, 0, 255))      // Blue
                })
            }
        };
    }

    /// <summary>
    /// Represents a color scale for rendering radar data
    /// </summary>
    public class RadarColorScale
    {
        /// <summary>
        /// List of color stops in the scale
        /// </summary>
        public List<RadarColorStop> ColorStops { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RadarColorScale"/> class
        /// </summary>
        public RadarColorScale(List<RadarColorStop> colorStops)
        {
            ColorStops = colorStops;
        }

        /// <summary>
        /// Gets the color for a value using this color scale
        /// </summary>
        public Color GetColor(double value)
        {
            if (ColorStops == null || ColorStops.Count == 0)
            {
                return Color.Gray;
            }

            // If value is below the lowest stop, use the lowest color
            if (value <= ColorStops[0].Value)
            {
                return ColorStops[0].Color;
            }

            // If value is above the highest stop, use the highest color
            if (value >= ColorStops[ColorStops.Count - 1].Value)
            {
                return ColorStops[ColorStops.Count - 1].Color;
            }

            // Find the two stops that bracket the value
            for (int i = 0; i < ColorStops.Count - 1; i++)
            {
                if (value >= ColorStops[i].Value && value < ColorStops[i + 1].Value)
                {
                    // Linear interpolation between the two colors
                    double ratio = (value - ColorStops[i].Value) / (ColorStops[i + 1].Value - ColorStops[i].Value);
                    return InterpolateColor(ColorStops[i].Color, ColorStops[i + 1].Color, ratio);
                }
            }

            // Default fallback
            return Color.Gray;
        }

        private Color InterpolateColor(Color color1, Color color2, double ratio)
        {
            int a = (int)(color1.A + (color2.A - color1.A) * ratio);
            int r = (int)(color1.R + (color2.R - color1.R) * ratio);
            int g = (int)(color1.G + (color2.G - color1.G) * ratio);
            int b = (int)(color1.B + (color2.B - color1.B) * ratio);

            return Color.FromArgb(a, r, g, b);
        }
    }

    /// <summary>
    /// Represents a color stop in a color scale
    /// </summary>
    public class RadarColorStop
    {
        /// <summary>
        /// The value at which this color applies
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// The color for this stop
        /// </summary>
        public Color Color { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RadarColorStop"/> class
        /// </summary>
        public RadarColorStop(double value, Color color)
        {
            Value = value;
            Color = color;
        }
    }
}
