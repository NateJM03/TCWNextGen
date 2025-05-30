using System;
using System.Collections.Generic;

namespace NexradViewer.Models
{
    /// <summary>
    /// Represents a color stop in a color scale
    /// </summary>
    public class ColorStop
    {
        /// <summary>
        /// The value at which this color should be used
        /// </summary>
        public float Value { get; set; }
        
        /// <summary>
        /// The color for this stop
        /// </summary>
        public System.Drawing.Color Color { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorStop"/> class
        /// </summary>
        public ColorStop(float value, System.Drawing.Color color)
        {
            Value = value;
            Color = color;
        }
    }

    /// <summary>
    /// Represents radar product types that can be displayed
    /// </summary>
    public class RadarProduct
    {
        /// <summary>
        /// The product code (e.g., "REF" for reflectivity)
        /// </summary>
        public string Code { get; set; }
        
        /// <summary>
        /// The display name for the product
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// The unit of measurement (e.g., "dBZ" for reflectivity)
        /// </summary>
        public string Unit { get; set; }
        
        /// <summary>
        /// The color scale configuration for this product
        /// </summary>
        public List<ColorStop> ColorScale { get; set; } = new List<ColorStop>();
        
        /// <summary>
        /// The minimum value for the product
        /// </summary>
        public float MinValue { get; set; }
        
        /// <summary>
        /// The maximum value for the product
        /// </summary>
        public float MaxValue { get; set; }
        
        /// <summary>
        /// Whether this is a Level 2 product
        /// </summary>
        public bool IsLevel2 { get; set; }

        /// <summary>
        /// Product category (reflectivity, velocity, etc.)
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Get a color for a specific value using this product's color scale
        /// </summary>
        public System.Drawing.Color GetColorForValue(float value)
        {
            if (value <= MinValue || ColorScale.Count == 0)
            {
                return System.Drawing.Color.Transparent;
            }
            
            if (value >= MaxValue)
            {
                return ColorScale[ColorScale.Count - 1].Color;
            }
            
            // Find the appropriate color stop
            for (int i = 0; i < ColorScale.Count - 1; i++)
            {
                if (value >= ColorScale[i].Value && value < ColorScale[i + 1].Value)
                {
                    // Could interpolate between the colors for smoother transitions
                    return ColorScale[i].Color;
                }
            }
            
            return System.Drawing.Color.Transparent;
        }
        
        #region Color Scale Methods
        /// <summary>
        /// Get the standard NWS reflectivity color scale
        /// </summary>
        private static List<ColorStop> GetReflectivityColorScale()
        {
            return new List<ColorStop>
            {
                new ColorStop(-32, System.Drawing.Color.FromArgb(0, 0, 0, 0)), // Transparent
                new ColorStop(-30, System.Drawing.Color.FromArgb(120, 120, 120)), // Light gray
                new ColorStop(-20, System.Drawing.Color.FromArgb(180, 180, 180)), // Gray
                new ColorStop(-10, System.Drawing.Color.FromArgb(96, 96, 240)), // Light blue
                new ColorStop(0, System.Drawing.Color.FromArgb(0, 0, 235)), // Blue
                new ColorStop(10, System.Drawing.Color.FromArgb(0, 235, 235)), // Cyan
                new ColorStop(20, System.Drawing.Color.FromArgb(0, 235, 0)), // Green
                new ColorStop(30, System.Drawing.Color.FromArgb(235, 235, 0)), // Yellow
                new ColorStop(40, System.Drawing.Color.FromArgb(235, 120, 0)), // Orange
                new ColorStop(50, System.Drawing.Color.FromArgb(235, 0, 0)), // Red
                new ColorStop(60, System.Drawing.Color.FromArgb(200, 0, 200)), // Purple
                new ColorStop(70, System.Drawing.Color.FromArgb(255, 0, 255)), // Magenta
                new ColorStop(95, System.Drawing.Color.FromArgb(255, 255, 255)) // White
            };
        }
        
        /// <summary>
        /// Get the standard NWS velocity color scale
        /// </summary>
        private static List<ColorStop> GetVelocityColorScale()
        {
            return new List<ColorStop>
            {
                new ColorStop(-75, System.Drawing.Color.FromArgb(255, 0, 128)), // Magenta (away)
                new ColorStop(-64, System.Drawing.Color.FromArgb(128, 0, 255)), // Purple (away)
                new ColorStop(-50, System.Drawing.Color.FromArgb(0, 0, 255)), // Blue (away)
                new ColorStop(-32, System.Drawing.Color.FromArgb(0, 128, 255)), // Light blue (away)
                new ColorStop(-16, System.Drawing.Color.FromArgb(0, 255, 128)), // Light green (away)
                new ColorStop(-8, System.Drawing.Color.FromArgb(0, 200, 0)), // Green (away)
                new ColorStop(-2, System.Drawing.Color.FromArgb(180, 180, 180)), // Gray (near-zero)
                new ColorStop(0, System.Drawing.Color.FromArgb(0, 0, 0, 0)), // Transparent (zero)
                new ColorStop(2, System.Drawing.Color.FromArgb(180, 180, 180)), // Gray (near-zero)
                new ColorStop(8, System.Drawing.Color.FromArgb(200, 200, 0)), // Yellow (toward)
                new ColorStop(16, System.Drawing.Color.FromArgb(255, 128, 0)), // Orange (toward)
                new ColorStop(32, System.Drawing.Color.FromArgb(255, 0, 0)), // Red (toward)
                new ColorStop(50, System.Drawing.Color.FromArgb(255, 0, 128)), // Hot pink (toward)
                new ColorStop(64, System.Drawing.Color.FromArgb(200, 0, 200)), // Purple (toward)
                new ColorStop(75, System.Drawing.Color.FromArgb(128, 0, 64)) // Maroon (toward)
            };
        }
        
        /// <summary>
        /// Get the spectrum width color scale
        /// </summary>
        private static List<ColorStop> GetSpectrumWidthColorScale()
        {
            return new List<ColorStop>
            {
                new ColorStop(0, System.Drawing.Color.FromArgb(0, 0, 0, 0)), // Transparent
                new ColorStop(2, System.Drawing.Color.FromArgb(0, 0, 100)), // Dark blue
                new ColorStop(4, System.Drawing.Color.FromArgb(0, 0, 200)), // Blue
                new ColorStop(6, System.Drawing.Color.FromArgb(0, 200, 200)), // Cyan
                new ColorStop(8, System.Drawing.Color.FromArgb(0, 200, 0)), // Green
                new ColorStop(10, System.Drawing.Color.FromArgb(200, 200, 0)), // Yellow
                new ColorStop(15, System.Drawing.Color.FromArgb(200, 100, 0)), // Orange
                new ColorStop(20, System.Drawing.Color.FromArgb(200, 0, 0)), // Red
                new ColorStop(25, System.Drawing.Color.FromArgb(200, 0, 200)), // Purple
                new ColorStop(30, System.Drawing.Color.FromArgb(255, 255, 255)) // White
            };
        }

        /// <summary>
        /// Get the precipitation color scale
        /// </summary>
        private static List<ColorStop> GetPrecipitationColorScale()
        {
            return new List<ColorStop>
            {
                new ColorStop(0, System.Drawing.Color.FromArgb(0, 0, 0, 0)), // Transparent
                new ColorStop(0.01f, System.Drawing.Color.FromArgb(180, 180, 180)), // Light gray
                new ColorStop(0.1f, System.Drawing.Color.FromArgb(170, 170, 230)), // Light blue
                new ColorStop(0.25f, System.Drawing.Color.FromArgb(85, 85, 255)), // Blue
                new ColorStop(0.5f, System.Drawing.Color.FromArgb(0, 170, 255)), // Darker blue
                new ColorStop(1, System.Drawing.Color.FromArgb(0, 255, 255)), // Cyan
                new ColorStop(1.5f, System.Drawing.Color.FromArgb(0, 255, 0)), // Green
                new ColorStop(2, System.Drawing.Color.FromArgb(255, 255, 0)), // Yellow
                new ColorStop(3, System.Drawing.Color.FromArgb(255, 170, 0)), // Orange
                new ColorStop(4, System.Drawing.Color.FromArgb(255, 85, 0)), // Bright orange
                new ColorStop(6, System.Drawing.Color.FromArgb(255, 0, 0)), // Red
                new ColorStop(8, System.Drawing.Color.FromArgb(255, 0, 255)), // Magenta
                new ColorStop(10, System.Drawing.Color.FromArgb(170, 0, 170)), // Purple
                new ColorStop(12, System.Drawing.Color.FromArgb(255, 255, 255)), // White
                new ColorStop(24, System.Drawing.Color.FromArgb(255, 255, 255)) // White
            };
        }

        /// <summary>
        /// Get the echo tops color scale
        /// </summary>
        private static List<ColorStop> GetEchoTopsColorScale()
        {
            return new List<ColorStop>
            {
                new ColorStop(0, System.Drawing.Color.FromArgb(0, 0, 0, 0)), // Transparent
                new ColorStop(1, System.Drawing.Color.FromArgb(100, 100, 195)), // Light blue
                new ColorStop(5, System.Drawing.Color.FromArgb(0, 0, 255)), // Blue
                new ColorStop(10, System.Drawing.Color.FromArgb(0, 255, 255)), // Cyan
                new ColorStop(15, System.Drawing.Color.FromArgb(0, 255, 0)), // Green
                new ColorStop(20, System.Drawing.Color.FromArgb(255, 255, 0)), // Yellow
                new ColorStop(30, System.Drawing.Color.FromArgb(255, 100, 0)), // Orange
                new ColorStop(40, System.Drawing.Color.FromArgb(255, 0, 0)), // Red
                new ColorStop(50, System.Drawing.Color.FromArgb(255, 0, 255)), // Magenta
                new ColorStop(60, System.Drawing.Color.FromArgb(200, 0, 200)), // Purple
                new ColorStop(70, System.Drawing.Color.FromArgb(255, 255, 255)) // White
            };
        }

        /// <summary>
        /// Get a binary color scale (for detection products)
        /// </summary>
        private static List<ColorStop> GetBinaryColorScale()
        {
            return new List<ColorStop>
            {
                new ColorStop(0, System.Drawing.Color.FromArgb(0, 0, 0, 0)), // Transparent
                new ColorStop(1, System.Drawing.Color.FromArgb(255, 0, 0)) // Red
            };
        }

        /// <summary>
        /// Get the ZDR color scale
        /// </summary>
        private static List<ColorStop> GetZDRColorScale()
        {
            return new List<ColorStop>
            {
                new ColorStop(-8, System.Drawing.Color.FromArgb(146, 0, 255)), // Purple
                new ColorStop(-5, System.Drawing.Color.FromArgb(0, 0, 255)), // Blue
                new ColorStop(-3, System.Drawing.Color.FromArgb(0, 255, 255)), // Cyan
                new ColorStop(-1, System.Drawing.Color.FromArgb(0, 255, 0)), // Green
                new ColorStop(0, System.Drawing.Color.FromArgb(180, 180, 180)), // Gray
                new ColorStop(1, System.Drawing.Color.FromArgb(255, 255, 0)), // Yellow
                new ColorStop(3, System.Drawing.Color.FromArgb(255, 150, 0)), // Orange
                new ColorStop(5, System.Drawing.Color.FromArgb(255, 0, 0)), // Red
                new ColorStop(8, System.Drawing.Color.FromArgb(255, 0, 255)) // Magenta
            };
        }

        /// <summary>
        /// Get the CC color scale
        /// </summary>
        private static List<ColorStop> GetCCColorScale()
        {
            return new List<ColorStop>
            {
                new ColorStop(0, System.Drawing.Color.FromArgb(255, 0, 0)), // Red (low correlation)
                new ColorStop(0.7f, System.Drawing.Color.FromArgb(255, 255, 0)), // Yellow
                new ColorStop(0.9f, System.Drawing.Color.FromArgb(0, 255, 0)), // Green
                new ColorStop(0.95f, System.Drawing.Color.FromArgb(0, 0, 255)), // Blue
                new ColorStop(0.98f, System.Drawing.Color.FromArgb(255, 0, 255)), // Magenta
                new ColorStop(1.0f, System.Drawing.Color.FromArgb(255, 255, 255)) // White (high correlation)
            };
        }

        /// <summary>
        /// Get the phase color scale
        /// </summary>
        private static List<ColorStop> GetPhaseColorScale()
        {
            return new List<ColorStop>
            {
                new ColorStop(0, System.Drawing.Color.FromArgb(0, 0, 255)), // Blue
                new ColorStop(60, System.Drawing.Color.FromArgb(0, 255, 255)), // Cyan
                new ColorStop(120, System.Drawing.Color.FromArgb(0, 255, 0)), // Green
                new ColorStop(180, System.Drawing.Color.FromArgb(255, 255, 0)), // Yellow
                new ColorStop(240, System.Drawing.Color.FromArgb(255, 0, 0)), // Red
                new ColorStop(300, System.Drawing.Color.FromArgb(255, 0, 255)), // Magenta
                new ColorStop(360, System.Drawing.Color.FromArgb(0, 0, 255)) // Blue (wrap around)
            };
        }

        /// <summary>
        /// Get the KDP color scale
        /// </summary>
        private static List<ColorStop> GetKDPColorScale()
        {
            return new List<ColorStop>
            {
                new ColorStop(-2, System.Drawing.Color.FromArgb(146, 0, 255)), // Purple (negative)
                new ColorStop(-1, System.Drawing.Color.FromArgb(0, 0, 255)), // Blue (negative)
                new ColorStop(0, System.Drawing.Color.FromArgb(180, 180, 180)), // Gray (near zero)
                new ColorStop(0.5f, System.Drawing.Color.FromArgb(0, 255, 0)), // Green (low)
                new ColorStop(1, System.Drawing.Color.FromArgb(255, 255, 0)), // Yellow (medium)
                new ColorStop(2, System.Drawing.Color.FromArgb(255, 128, 0)), // Orange (medium-high)
                new ColorStop(3, System.Drawing.Color.FromArgb(255, 0, 0)), // Red (high)
                new ColorStop(5, System.Drawing.Color.FromArgb(255, 0, 255)), // Magenta (very high)
                new ColorStop(7, System.Drawing.Color.FromArgb(255, 255, 255)) // White (extreme)
            };
        }

        /// <summary>
        /// Get the hydrometeor classification color scale
        /// </summary>
        private static List<ColorStop> GetHydrometeorClassColorScale()
        {
            return new List<ColorStop>
            {
                new ColorStop(0, System.Drawing.Color.FromArgb(200, 200, 200)), // Light Gray (Unknown)
                new ColorStop(1, System.Drawing.Color.FromArgb(102, 255, 153)), // Light Green (Biological)
                new ColorStop(2, System.Drawing.Color.FromArgb(0, 204, 204)), // Teal (Ground Clutter)
                new ColorStop(3, System.Drawing.Color.FromArgb(5, 5, 255)), // Blue (Ice Crystals)
                new ColorStop(4, System.Drawing.Color.FromArgb(0, 128, 255)), // Light Blue (Dry Snow)
                new ColorStop(5, System.Drawing.Color.FromArgb(0, 255, 255)), // Cyan (Wet Snow)
                new ColorStop(6, System.Drawing.Color.FromArgb(0, 255, 0)), // Green (Light Rain)
                new ColorStop(7, System.Drawing.Color.FromArgb(255, 255, 0)), // Yellow (Moderate Rain)
                new ColorStop(8, System.Drawing.Color.FromArgb(255, 128, 0)), // Orange (Heavy Rain)
                new ColorStop(9, System.Drawing.Color.FromArgb(255, 0, 0)), // Red (Hail)
                new ColorStop(10, System.Drawing.Color.FromArgb(255, 0, 255)) // Magenta (Large Hail)
            };
        }

        /// <summary>
        /// Get the melting layer color scale
        /// </summary>
        private static List<ColorStop> GetMeltingLayerColorScale()
        {
            return new List<ColorStop>
            {
                new ColorStop(0, System.Drawing.Color.FromArgb(0, 0, 0, 0)), // Transparent (Not detected)
                new ColorStop(1, System.Drawing.Color.FromArgb(255, 255, 0)), // Yellow (Layer bottom)
                new ColorStop(2, System.Drawing.Color.FromArgb(255, 128, 0)), // Orange (Layer middle)
                new ColorStop(3, System.Drawing.Color.FromArgb(255, 0, 0)) // Red (Layer top)
            };
        }
        #endregion

        #region Product Definitions
        // Level 2 Products
        /// <summary>
        /// Predefined Level 2 reflectivity product
        /// </summary>
        public static readonly RadarProduct Reflectivity = new RadarProduct
        {
            Code = "REF",
            DisplayName = "Reflectivity",
            Unit = "dBZ",
            IsLevel2 = true,
            Category = "Reflectivity",
            MinValue = -32,
            MaxValue = 95,
            ColorScale = GetReflectivityColorScale()
        };
        
        /// <summary>
        /// Predefined Level 2 velocity product
        /// </summary>
        public static readonly RadarProduct Velocity = new RadarProduct
        {
            Code = "VEL",
            DisplayName = "Velocity",
            Unit = "m/s",
            IsLevel2 = true,
            Category = "Velocity",
            MinValue = -75,
            MaxValue = 75,
            ColorScale = GetVelocityColorScale()
        };
        
        /// <summary>
        /// Predefined Level 2 spectrum width product
        /// </summary>
        public static readonly RadarProduct SpectrumWidth = new RadarProduct
        {
            Code = "SW",
            DisplayName = "Spectrum Width",
            Unit = "m/s",
            IsLevel2 = true,
            Category = "Spectrum Width",
            MinValue = 0,
            MaxValue = 30,
            ColorScale = GetSpectrumWidthColorScale()
        };

        // Level 3 Products - Reflectivity
        /// <summary>
        /// Predefined Level 3 base reflectivity product
        /// </summary>
        public static readonly RadarProduct BaseReflectivity = new RadarProduct
        {
            Code = "N0R",
            DisplayName = "Base Reflectivity",
            Unit = "dBZ",
            IsLevel2 = false,
            Category = "Reflectivity",
            MinValue = -32,
            MaxValue = 95,
            ColorScale = GetReflectivityColorScale()
        };

        /// <summary>
        /// Predefined Level 3 composite reflectivity product
        /// </summary>
        public static readonly RadarProduct CompositeReflectivity = new RadarProduct
        {
            Code = "NCZ",
            DisplayName = "Composite Reflectivity",
            Unit = "dBZ",
            IsLevel2 = false,
            Category = "Reflectivity",
            MinValue = -32,
            MaxValue = 95,
            ColorScale = GetReflectivityColorScale()
        };
        
        /// <summary>
        /// Predefined Level 3 super-resolution base reflectivity product
        /// </summary>
        public static readonly RadarProduct SuperResReflectivity = new RadarProduct
        {
            Code = "SR_BREF",
            DisplayName = "Super-Resolution Base Reflectivity",
            Unit = "dBZ",
            IsLevel2 = false,
            Category = "Reflectivity",
            MinValue = -32,
            MaxValue = 95,
            ColorScale = GetReflectivityColorScale()
        };

        /// <summary>
        /// Predefined Level 3 low-layer composite reflectivity product
        /// </summary>
        public static readonly RadarProduct LowLayerComposite = new RadarProduct
        {
            Code = "NLL",
            DisplayName = "Low-Layer Composite Reflectivity",
            Unit = "dBZ",
            IsLevel2 = false,
            Category = "Reflectivity",
            MinValue = -32,
            MaxValue = 95,
            ColorScale = GetReflectivityColorScale()
        };

        // Level 3 Products - Velocity
        /// <summary>
        /// Predefined Level 3 base velocity product
        /// </summary>
        public static readonly RadarProduct BaseVelocity = new RadarProduct
        {
            Code = "N0V",
            DisplayName = "Base Velocity",
            Unit = "m/s",
            IsLevel2 = false,
            Category = "Velocity",
            MinValue = -75,
            MaxValue = 75,
            ColorScale = GetVelocityColorScale()
        };

        /// <summary>
        /// Predefined Level 3 storm-relative motion product
        /// </summary>
        public static readonly RadarProduct StormRelativeMotion = new RadarProduct
        {
            Code = "N0S",
            DisplayName = "Storm-Relative Mean Velocity",
            Unit = "m/s",
            IsLevel2 = false,
            Category = "Velocity",
            MinValue = -75,
            MaxValue = 75,
            ColorScale = GetVelocityColorScale()
        };

        /// <summary>
        /// Predefined Level 3 VAD wind profile product
        /// </summary>
        public static readonly RadarProduct VADWindProfile = new RadarProduct
        {
            Code = "NVW",
            DisplayName = "VAD Wind Profile",
            Unit = "m/s",
            IsLevel2 = false,
            Category = "Velocity",
            MinValue = -75,
            MaxValue = 75,
            ColorScale = GetVelocityColorScale()
        };

        // Level 3 Products - Precipitation
        /// <summary>
        /// Predefined Level 3 one-hour precipitation product
        /// </summary>
        public static readonly RadarProduct OneHourPrecipitation = new RadarProduct
        {
            Code = "N1P",
            DisplayName = "One-Hour Precipitation",
            Unit = "in",
            IsLevel2 = false,
            Category = "Precipitation",
            MinValue = 0,
            MaxValue = 12,
            ColorScale = GetPrecipitationColorScale()
        };

        /// <summary>
        /// Predefined Level 3 storm-total precipitation product
        /// </summary>
        public static readonly RadarProduct StormTotalPrecipitation = new RadarProduct
        {
            Code = "NTP",
            DisplayName = "Storm-Total Precipitation",
            Unit = "in",
            IsLevel2 = false,
            Category = "Precipitation",
            MinValue = 0,
            MaxValue = 24,
            ColorScale = GetPrecipitationColorScale()
        };

        /// <summary>
        /// Predefined Level 3 digital precipitation array product
        /// </summary>
        public static readonly RadarProduct DigitalPrecipitationArray = new RadarProduct
        {
            Code = "DPA",
            DisplayName = "Digital Precipitation Array",
            Unit = "in",
            IsLevel2 = false,
            Category = "Precipitation",
            MinValue = 0,
            MaxValue = 12,
            ColorScale = GetPrecipitationColorScale()
        };

        // Level 3 Products - Severe Weather
        /// <summary>
        /// Predefined Level 3 enhanced echo tops product
        /// </summary>
        public static readonly RadarProduct EnhancedEchoTops = new RadarProduct
        {
            Code = "EET",
            DisplayName = "Enhanced Echo Tops",
            Unit = "kft",
            IsLevel2 = false,
            Category = "Severe Weather",
            MinValue = 0,
            MaxValue = 70,
            ColorScale = GetEchoTopsColorScale()
        };

        /// <summary>
        /// Predefined Level 3 mesocyclone detection product
        /// </summary>
        public static readonly RadarProduct MesocycloneDetection = new RadarProduct
        {
            Code = "NMD",
            DisplayName = "Mesocyclone Detection",
            Unit = "",
            IsLevel2 = false,
            Category = "Severe Weather",
            MinValue = 0,
            MaxValue = 1,
            ColorScale = GetBinaryColorScale()
        };

        /// <summary>
        /// Predefined Level 3 tornadic vortex signature product
        /// </summary>
        public static readonly RadarProduct TornadicVortexSignature = new RadarProduct
        {
            Code = "NTV",
            DisplayName = "Tornadic Vortex Signature",
            Unit = "",
            IsLevel2 = false,
            Category = "Severe Weather",
            MinValue = 0,
            MaxValue = 1,
            ColorScale = GetBinaryColorScale()
        };

        /// <summary>
        /// Alias for TornadicVortexSignature
        /// </summary>
        public static readonly RadarProduct TVS = TornadicVortexSignature;

        // Dual-Polarization Products
        /// <summary>
        /// Predefined dual-polarization differential reflectivity product
        /// </summary>
        public static readonly RadarProduct DifferentialReflectivity = new RadarProduct
        {
            Code = "ZDR",
            DisplayName = "Differential Reflectivity",
            Unit = "dB",
            IsLevel2 = true,
            Category = "Dual-Polarization",
            MinValue = -8,
            MaxValue = 8,
            ColorScale = GetZDRColorScale()
        };

        /// <summary>
        /// Alias for DifferentialReflectivity
        /// </summary>
        public static readonly RadarProduct ZDR = DifferentialReflectivity;

        /// <summary>
        /// Predefined dual-polarization correlation coefficient product
        /// </summary>
        public static readonly RadarProduct CorrelationCoefficient = new RadarProduct
        {
            Code = "CC",
            DisplayName = "Correlation Coefficient",
            Unit = "",
            IsLevel2 = true,
            Category = "Dual-Polarization",
            MinValue = 0,
            MaxValue = 1,
            ColorScale = GetCCColorScale()
        };

        /// <summary>
        /// Alias for CorrelationCoefficient
        /// </summary>
        public static readonly RadarProduct CC = CorrelationCoefficient;

        /// <summary>
        /// Alias for CorrelationCoefficient (official NEXRAD name)
        /// </summary>
        public static readonly RadarProduct RHO = CorrelationCoefficient;

        /// <summary>
        /// Predefined dual-polarization differential phase product
        /// </summary>
        public static readonly RadarProduct DifferentialPhase = new RadarProduct
        {
            Code = "PHI",
            DisplayName = "Differential Phase",
            Unit = "°",
            IsLevel2 = true,
            Category = "Dual-Polarization",
            MinValue = 0,
            MaxValue = 360,
            ColorScale = GetPhaseColorScale()
        };

        /// <summary>
        /// Alias for DifferentialPhase
        /// </summary>
        public static readonly RadarProduct PHI = DifferentialPhase;

        /// <summary>
        /// Predefined dual-polarization specific differential phase product
        /// </summary>
        public static readonly RadarProduct SpecificDifferentialPhase = new RadarProduct
        {
            Code = "KDP",
            DisplayName = "Specific Differential Phase",
            Unit = "°/km",
            IsLevel2 = true,
            Category = "Dual-Polarization",
            MinValue = -2,
            MaxValue = 7,
            ColorScale = GetKDPColorScale()
        };

        /// <summary>
        /// Alias for SpecificDifferentialPhase
        /// </summary>
        public static readonly RadarProduct KDP = SpecificDifferentialPhase;

        /// <summary>
        /// Hydrometeor classification product
        /// </summary>
        public static readonly RadarProduct HydrometeorClass = new RadarProduct
        {
            Code = "HC",
            DisplayName = "Hydrometeor Classification",
            Unit = "",
            IsLevel2 = true,
            Category = "Dual-Polarization",
            MinValue = 0,
            MaxValue = 10,
            ColorScale = GetHydrometeorClassColorScale()
        };

        /// <summary>
        /// Melting layer product
        /// </summary>
        public static readonly RadarProduct MeltingLayer = new RadarProduct
        {
            Code = "ML",
            DisplayName = "Melting Layer",
            Unit = "",
            IsLevel2 = true,
            Category = "Dual-Polarization",
            MinValue = 0,
            MaxValue = 3,
            ColorScale = GetMeltingLayerColorScale()
        };
        #endregion
    }
}
