using System;
using System.Collections.Generic;
using System.Drawing;

namespace NexradViewer.Models
{
    /// <summary>
    /// Static class containing mapping data for NEXRAD products
    /// </summary>
    public static class MappingTypes
    {
        /// <summary>
        /// Maps product types to their display names
        /// </summary>
        public static readonly Dictionary<RadarProductType, string> ProductDisplayNames = new Dictionary<RadarProductType, string>
        {
            { RadarProductType.BaseReflectivity, "Base Reflectivity" },
            { RadarProductType.BaseVelocity, "Base Velocity" },
            { RadarProductType.CompositeReflectivity, "Composite Reflectivity" },
            { RadarProductType.SuperResolutionBaseReflectivity, "Super-Resolution Base Reflectivity" },
            { RadarProductType.LowLayerComposite, "Low Layer Composite Reflectivity" },
            { RadarProductType.StormRelativeMeanVelocity, "Storm-Relative Mean Velocity" },
            { RadarProductType.VADWindProfile, "VAD Wind Profile" },
            { RadarProductType.SpectrumWidth, "Spectrum Width" },
            { RadarProductType.OneHourAccumulation, "One-Hour Precipitation" },
            { RadarProductType.StormTotalPrecipitation, "Storm Total Precipitation" },
            { RadarProductType.DigitalPrecipitationArray, "Digital Precipitation Array" },
            { RadarProductType.EnhancedEchoTops, "Enhanced Echo Tops" },
            { RadarProductType.MesocycloneDetection, "Mesocyclone Detection" },
            { RadarProductType.TornadicVortexSignature, "Tornadic Vortex Signature" },
            { RadarProductType.CorrelationCoefficient, "Correlation Coefficient" },
            { RadarProductType.DifferentialReflectivity, "Differential Reflectivity" },
            { RadarProductType.SpecificDifferentialPhase, "Specific Differential Phase" },
            { RadarProductType.HydrometeorClassification, "Hydrometeor Classification" }
        };

        /// <summary>
        /// Maps product types to whether they are Level 2 products
        /// </summary>
        public static readonly Dictionary<RadarProductType, bool> IsLevel2Product = new Dictionary<RadarProductType, bool>
        {
            { RadarProductType.BaseReflectivity, true },
            { RadarProductType.BaseVelocity, true },
            { RadarProductType.SpectrumWidth, true },
            { RadarProductType.CorrelationCoefficient, true },
            { RadarProductType.DifferentialReflectivity, true },
            { RadarProductType.SpecificDifferentialPhase, true },
            { RadarProductType.CompositeReflectivity, false },
            { RadarProductType.SuperResolutionBaseReflectivity, false },
            { RadarProductType.LowLayerComposite, false },
            { RadarProductType.StormRelativeMeanVelocity, false },
            { RadarProductType.VADWindProfile, false },
            { RadarProductType.OneHourAccumulation, false },
            { RadarProductType.StormTotalPrecipitation, false },
            { RadarProductType.DigitalPrecipitationArray, false },
            { RadarProductType.EnhancedEchoTops, false },
            { RadarProductType.MesocycloneDetection, false },
            { RadarProductType.TornadicVortexSignature, false },
            { RadarProductType.HydrometeorClassification, false }
        };

        /// <summary>
        /// Maps product types to their Level 3 product codes
        /// </summary>
        public static readonly Dictionary<RadarProductType, string> Level3ProductCodes = new Dictionary<RadarProductType, string>
        {
            { RadarProductType.CompositeReflectivity, "NCZ" },
            { RadarProductType.SuperResolutionBaseReflectivity, "SR_BREF" },
            { RadarProductType.LowLayerComposite, "NLL" },
            { RadarProductType.BaseVelocity, "N0V" },
            { RadarProductType.StormRelativeMeanVelocity, "N0S" },
            { RadarProductType.VADWindProfile, "NVW" },
            { RadarProductType.OneHourAccumulation, "N1P" },
            { RadarProductType.StormTotalPrecipitation, "NTP" },
            { RadarProductType.DigitalPrecipitationArray, "DPA" },
            { RadarProductType.EnhancedEchoTops, "EET" },
            { RadarProductType.MesocycloneDetection, "NMD" },
            { RadarProductType.TornadicVortexSignature, "NTV" }
        };

        /// <summary>
        /// Maps product types to their units
        /// </summary>
        public static readonly Dictionary<RadarProductType, string> ProductUnits = new Dictionary<RadarProductType, string>
        {
            { RadarProductType.BaseReflectivity, "dBZ" },
            { RadarProductType.BaseVelocity, "m/s" },
            { RadarProductType.CompositeReflectivity, "dBZ" },
            { RadarProductType.SuperResolutionBaseReflectivity, "dBZ" },
            { RadarProductType.LowLayerComposite, "dBZ" },
            { RadarProductType.StormRelativeMeanVelocity, "m/s" },
            { RadarProductType.VADWindProfile, "m/s" },
            { RadarProductType.SpectrumWidth, "m/s" },
            { RadarProductType.OneHourAccumulation, "in" },
            { RadarProductType.StormTotalPrecipitation, "in" },
            { RadarProductType.DigitalPrecipitationArray, "in" },
            { RadarProductType.EnhancedEchoTops, "ft" },
            { RadarProductType.MesocycloneDetection, "" },
            { RadarProductType.TornadicVortexSignature, "" },
            { RadarProductType.CorrelationCoefficient, "" },
            { RadarProductType.DifferentialReflectivity, "dB" },
            { RadarProductType.SpecificDifferentialPhase, "°" },
            { RadarProductType.HydrometeorClassification, "" }
        };

        /// <summary>
        /// Maps product categories - added for compatibility
        /// </summary>
        public static readonly Dictionary<RadarProductType, string> ProductCategories = new Dictionary<RadarProductType, string>
        {
            { RadarProductType.BaseReflectivity, "Reflectivity" },
            { RadarProductType.BaseVelocity, "Velocity" },
            { RadarProductType.CompositeReflectivity, "Reflectivity" },
            { RadarProductType.SuperResolutionBaseReflectivity, "Reflectivity" },
            { RadarProductType.LowLayerComposite, "Reflectivity" },
            { RadarProductType.StormRelativeMeanVelocity, "Velocity" },
            { RadarProductType.VADWindProfile, "Velocity" },
            { RadarProductType.SpectrumWidth, "Velocity" },
            { RadarProductType.OneHourAccumulation, "Precipitation" },
            { RadarProductType.StormTotalPrecipitation, "Precipitation" },
            { RadarProductType.DigitalPrecipitationArray, "Precipitation" },
            { RadarProductType.EnhancedEchoTops, "Severe Weather" },
            { RadarProductType.MesocycloneDetection, "Severe Weather" },
            { RadarProductType.TornadicVortexSignature, "Severe Weather" },
            { RadarProductType.CorrelationCoefficient, "Dual-Pol" },
            { RadarProductType.DifferentialReflectivity, "Dual-Pol" },
            { RadarProductType.SpecificDifferentialPhase, "Dual-Pol" },
            { RadarProductType.HydrometeorClassification, "Dual-Pol" }
        };

        /// <summary>
        /// Gets color for weather warnings based on warning type
        /// </summary>
        public static Color GetWarningColor(WarningType warningType)
        {
            switch (warningType)
            {
                case WarningType.TornadoWatch:
                    return Color.FromArgb(255, 255, 0); // Yellow
                case WarningType.Tornado:
                    return Color.FromArgb(255, 0, 0); // Red
                case WarningType.SevereThunderstormWatch:
                    return Color.FromArgb(0, 255, 255); // Cyan
                case WarningType.SevereThunderstorm:
                    return Color.FromArgb(255, 165, 0); // Orange
                case WarningType.Flood:
                    return Color.FromArgb(0, 255, 0); // Green
                case WarningType.FloodWarning:
                    return Color.FromArgb(0, 128, 0); // Dark Green
                case WarningType.FlashFlood:
                    return Color.FromArgb(139, 0, 0); // Dark Red
                case WarningType.FlashFloodWatch:
                    return Color.FromArgb(144, 238, 144); // Light Green
                default:
                    return Color.FromArgb(128, 128, 128); // Gray
            }
        }
    }
}
