# TheClearWeather | NextGenRadar - User Manual

## Introduction

Welcome to TheClearWeather | NextGenRadar, a comprehensive radar visualization platform that provides powerful access to NEXRAD weather radar data. This application offers both nationwide radar coverage and detailed individual station displays with support for numerous radar products and weather alerts.

## Getting Started

### System Requirements
- Windows 10 or higher
- 8GB RAM minimum (16GB recommended for nationwide views)
- 4GB free disk space
- Graphics card with DirectX 11 support recommended

### Installation
1. Download the installer from the TheClearWeather website
2. Run the installer and follow the on-screen instructions
3. Launch the application from the desktop shortcut or Start menu

## Main Interface

The application interface is organized into several key areas:

![Main Interface](../Assets/Documentation/main_interface.png)

1. **Main Menu & Toolbar**: Access to core functionality and quick actions
2. **Map Display**: Primary radar visualization area
3. **Station Selection**: Select individual radar sites or nationwide view
4. **Product Controls**: Choose between different radar products
5. **Layer Controls**: Toggle and adjust various data layers
6. **Status Bar**: Display current status and data information

## Core Features

### Radar Views

#### Nationwide Radar
The nationwide radar view combines data from all NEXRAD stations to provide a comprehensive view of weather conditions across the entire United States.

**To access nationwide view:**
1. Select "Nationwide" from the station dropdown
2. Choose the desired product from the product dropdown
3. Adjust opacity and other display options as needed

> **Note**: The nationwide view is not loaded automatically to improve initial application performance. You must select it explicitly.

#### Individual Station View
For detailed analysis of a specific area, you can view data from individual NEXRAD radar stations.

**To access station view:**
1. Select the desired station from the station dropdown (stations are organized by state)
2. Choose the radar product you want to display
3. Adjust radar settings as needed

### Radar Products

#### Level 2 Base Products
- **Reflectivity**: Shows intensity of precipitation (default product)
- **Velocity**: Displays movement of precipitation toward or away from the radar
- **Spectrum Width**: Visualizes the variance in velocity measurements within the radar sample volume

#### Level 3 Derived Products

##### Reflectivity Products
- **Base Reflectivity (Various Tilts)**: Echo intensity at different elevation angles
- **Composite Reflectivity**: Maximum reflectivity in a vertical column
- **Layer Composite Reflectivity**: Maximum reflectivity within specific atmospheric layers
- **Digital Hybrid Scan Reflectivity**: Combined view from multiple elevation scans

##### Velocity Products
- **Base Velocity (Various Tilts)**: Radial velocity at different elevation angles
- **Storm Relative Mean Velocity (SRM)**: Velocity adjusted for storm motion
- **Velocity Azimuth Display (VAD) Wind Profile**: Vertical profile of horizontal winds

##### Precipitation Products
- **One-Hour Precipitation**: Estimated rainfall over the past hour
- **Three-Hour Precipitation**: Estimated rainfall over the past three hours
- **Storm Total Precipitation**: Estimated total rainfall since storm start
- **Digital Precipitation Array**: Gridded field of precipitation estimates
- **Hydrometeor Classification**: Identification of precipitation types
- **Vertically Integrated Liquid**: Estimate of liquid water content in a vertical column

##### Severe Weather Products
- **Echo Tops**: Maximum height of radar echoes
- **Hail Index**: Probability or potential size of hail
- **Mesocyclone Detection**: Identification of rotating updrafts
- **Tornadic Vortex Signature**: Detection of intense rotation indicative of possible tornados
- **Storm Structure**: Details on characteristics of identified storm cells
- **Storm Tracking Information**: Forecast positions of identified storm cells

##### Dual-Polarization Products
- **Differential Reflectivity (ZDR)**: Ratio of horizontal to vertical reflectivity
- **Correlation Coefficient (CC)**: Similarity between horizontal and vertical pulses
- **Specific Differential Phase (KDP)**: Phase shift between horizontal and vertical pulses

### Map Types

The application offers multiple map background options:

1. **Streets**: Standard street map for urban reference
2. **Satellite**: Aerial imagery for terrain reference
3. **Terrain**: Topographic map for elevation context
4. **Dark**: Low-light map ideal for nighttime use or to emphasize radar data

**To change map type:**
1. Click the "Map Type" dropdown in the toolbar
2. Select your preferred map type
3. The map will instantly update to reflect your selection

### Weather Alerts

The application displays NWS weather alerts color-coded according to official standards.

**Alert categories include:**
- **Warnings**: Immediate threat to life and property (red shades)
- **Watches**: Conditions favorable for hazardous weather (yellow/orange shades)
- **Advisories**: Less serious conditions that may cause inconvenience (blue shades)
- **Statements**: Updates on ongoing weather events (green shades)

**To manage alerts:**
1. Check/uncheck the alert categories in the Layers panel
2. Click on an alert on the map to view detailed information
3. Adjust alert opacity using the opacity slider

## Advanced Features

### Side-by-Side Comparison

The side-by-side view allows you to compare different products simultaneously.

**To enable side-by-side view:**
1. Click the "Side-by-Side" button in the toolbar
2. Select the products you want to compare in each view
3. Use the synchronize button to keep the maps aligned as you pan and zoom

> **Note**: Side-by-side view is only available when viewing a single radar station.

### Layer Opacity Control

Fine-tune the visibility of each data layer with independent opacity controls.

**To adjust opacity:**
1. Locate the opacity slider for the desired layer
2. Drag the slider left (more transparent) or right (more opaque)
3. The display updates in real-time as you adjust

### Time-Based Viewing

For supported products, you can review historical radar data.

**To access time-based viewing:**
1. Click the "Time" button in the toolbar
2. Use the timeline slider to move backward or forward in time
3. Click the play button to animate through available time steps

> **Note**: Time-based viewing may not be available for all products and is limited to recent data for nationwide views.

### GOES Satellite Imagery

View cloud cover and other atmospheric conditions from GOES satellite imagery.

**To access satellite imagery:**
1. Select "GOES Imagery" from the product dropdown
2. Choose the specific GOES product (Visible, Infrared, Water Vapor, etc.)
3. Use the time controls to view the most recent imagery

### Weather Data Maps

In addition to radar data, the application provides several weather data maps:

- **Temperature**: Current surface temperature
- **Wind Speed and Direction**: Surface wind conditions
- **Cloud Cover**: Current cloud coverage
- **Precipitation Forecast**: Expected precipitation over the next 48 hours

**To access weather data maps:**
1. Click the "Weather Maps" button in the toolbar
2. Select the desired data layer
3. Use the time slider to view forecast data where available

## Customization

### UI Customization

Personalize the interface to suit your preferences:

1. Click "Settings" in the menu bar
2. Select "UI Customization"
3. Choose from available themes or adjust individual elements
4. Save your custom layout for future use

### Alert Preferences

Configure how weather alerts are displayed:

1. Click "Settings" in the menu bar
2. Select "Alert Settings"
3. Choose which categories of alerts to display
4. Configure alert notification options

### Performance Settings

Optimize application performance for your system:

1. Click "Settings" in the menu bar
2. Select "Performance"
3. Adjust cache size, rendering quality, and update frequency
4. Click "Apply" to save your changes

## Troubleshooting

### Common Issues

**Application is slow when displaying nationwide view**
- Reduce the rendering quality in Settings > Performance
- Close other memory-intensive applications
- Ensure your system meets the recommended requirements

**Radar data is not updating**
- Check your internet connection
- Click the manual refresh button
- Verify the NWS data services are operational

**Map tiles are not loading properly**
- Try switching to a different map type
- Restart the application
- Check if offline maps are properly installed

### Support

If you encounter any issues not covered in this manual:

1. Visit [support.theclearweather.com](https://support.theclearweather.com)
2. Email support at support@theclearweather.com
3. Check for application updates that may resolve the issue

## Keyboard Shortcuts

| Function | Shortcut |
|----------|----------|
| Refresh Data | F5 |
| Toggle Fullscreen | F11 |
| Side-by-Side View | Ctrl+D |
| Nationwide View | Ctrl+N |
| Zoom In | Ctrl++ or Mouse Wheel Up |
| Zoom Out | Ctrl+- or Mouse Wheel Down |
| Pan Map | Click and Drag |
| Reset View | Home |
| Show/Hide Alerts | Ctrl+A |
| Show/Hide Legend | Ctrl+L |
| Application Settings | Ctrl+, |
| Help | F1 |
