namespace NexradViewer.Models
{
    /// <summary>
    /// Enumeration of radar product types
    /// </summary>
    public enum RadarProductType
    {
        /// <summary>
        /// Base Reflectivity (Level 2)
        /// </summary>
        BaseReflectivity,
        
        /// <summary>
        /// Base Velocity (Level 2)
        /// </summary>
        BaseVelocity,
        
        /// <summary>
        /// Spectrum Width (Level 2)
        /// </summary>
        SpectrumWidth,
        
        /// <summary>
        /// Correlation Coefficient (Level 2, Dual-Pol)
        /// </summary>
        CorrelationCoefficient,
        
        /// <summary>
        /// Differential Reflectivity (Level 2, Dual-Pol)
        /// </summary>
        DifferentialReflectivity,
        
        /// <summary>
        /// Specific Differential Phase (Level 2, Dual-Pol)
        /// </summary>
        SpecificDifferentialPhase,
        
        /// <summary>
        /// Composite Reflectivity (Level 3)
        /// </summary>
        CompositeReflectivity,
        
        /// <summary>
        /// Super-Resolution Base Reflectivity (Level 3)
        /// </summary>
        SuperResolutionBaseReflectivity,
        
        /// <summary>
        /// Low-Layer Composite (Level 3)
        /// </summary>
        LowLayerComposite,
        
        /// <summary>
        /// Storm-Relative Mean Velocity (Level 3)
        /// </summary>
        StormRelativeMeanVelocity,
        
        /// <summary>
        /// VAD Wind Profile (Level 3)
        /// </summary>
        VADWindProfile,
        
        /// <summary>
        /// One-Hour Precipitation (Level 3)
        /// </summary>
        OneHourAccumulation,
        
        /// <summary>
        /// Storm Total Precipitation (Level 3)
        /// </summary>
        StormTotalPrecipitation,
        
        /// <summary>
        /// Digital Precipitation Array (Level 3)
        /// </summary>
        DigitalPrecipitationArray,
        
        /// <summary>
        /// Enhanced Echo Tops (Level 3)
        /// </summary>
        EnhancedEchoTops,
        
        /// <summary>
        /// Mesocyclone Detection (Level 3)
        /// </summary>
        MesocycloneDetection,
        
        /// <summary>
        /// Tornadic Vortex Signature (Level 3)
        /// </summary>
        TornadicVortexSignature,
        
        /// <summary>
        /// Hydrometeor Classification (Level 3, Dual-Pol)
        /// </summary>
        HydrometeorClassification
    }
}
