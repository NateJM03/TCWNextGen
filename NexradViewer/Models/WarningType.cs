namespace NexradViewer.Models
{
    /// <summary>
    /// Represents types of weather warnings
    /// </summary>
    public enum WarningType
    {
        /// <summary>
        /// Unknown warning type
        /// </summary>
        Unknown = 0,
        
        /// <summary>
        /// Tornado warning
        /// </summary>
        Tornado = 1,
        
        /// <summary>
        /// Tornado watch
        /// </summary>
        TornadoWatch = 2,
        
        /// <summary>
        /// Severe thunderstorm warning
        /// </summary>
        SevereThunderstorm = 3,
        
        /// <summary>
        /// Severe thunderstorm watch
        /// </summary>
        SevereThunderstormWatch = 4,
        
        /// <summary>
        /// Flash flood warning
        /// </summary>
        FlashFlood = 5,
        
        /// <summary>
        /// Flash flood watch
        /// </summary>
        FlashFloodWatch = 6,
        
        /// <summary>
        /// Flood warning
        /// </summary>
        FloodWarning = 7,
        
        /// <summary>
        /// Flood watch/advisory
        /// </summary>
        Flood = 8,
        
        /// <summary>
        /// Special marine warning
        /// </summary>
        Marine = 9,
        
        /// <summary>
        /// Extreme wind warning
        /// </summary>
        ExtremeWind = 10,
        
        /// <summary>
        /// Snow squall warning
        /// </summary>
        SnowSquall = 11,
        
        /// <summary>
        /// Dust storm warning
        /// </summary>
        DustStorm = 12,
        
        /// <summary>
        /// Blizzard warning
        /// </summary>
        Blizzard = 13,
        
        /// <summary>
        /// Ice storm warning
        /// </summary>
        IceStorm = 14,
        
        /// <summary>
        /// Winter storm warning
        /// </summary>
        WinterStorm = 15,
        
        /// <summary>
        /// Winter storm watch
        /// </summary>
        WinterStormWatch = 16,
        
        /// <summary>
        /// Frost warning or freeze warning
        /// </summary>
        FrostFreeze = 17,
        
        /// <summary>
        /// Wind advisory or high wind warning
        /// </summary>
        WindAdvisory = 18,
        
        /// <summary>
        /// Heat advisory or excessive heat warning
        /// </summary>
        HeatAdvisory = 19,
        
        /// <summary>
        /// Air quality alert
        /// </summary>
        AirQuality = 20,
        
        /// <summary>
        /// Coastal flood advisory/warning
        /// </summary>
        CoastalFlood = 21,
        
        /// <summary>
        /// Rip current statement
        /// </summary>
        RipCurrent = 22,
        
        /// <summary>
        /// Dense fog advisory
        /// </summary>
        DenseFog = 23,
        
        /// <summary>
        /// Special weather statement
        /// </summary>
        SpecialWeatherStatement = 24,
        
        /// <summary>
        /// Hazardous weather outlook
        /// </summary>
        HazardousWeatherOutlook = 25,
        
        /// <summary>
        /// Other non-specific type of warning
        /// </summary>
        Other = 99
    }
}
