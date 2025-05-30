# NEXRAD Radar Viewer

A cross-platform C# desktop application for viewing NEXRAD weather radar data directly from the National Weather Service (NWS) API.

![NEXRAD Radar Viewer Screenshot](Assets/screenshot.png)

## Features

- View live NEXRAD radar data directly from NWS sources
- No pre-generated or cached radar data - always displays the latest information
- Multiple radar products (Reflectivity, Velocity)
- View nationwide weather warnings and alerts
- Station selection from across the United States
- Auto-refresh capability to keep data current
- Cross-platform support (Windows, macOS, Linux) using .NET and Avalonia UI

## Technical Details

This application is built with:

- C# / .NET 6
- Avalonia UI for cross-platform desktop support
- Mapsui library for map rendering
- Direct integration with NWS APIs
- AWS SDK for accessing NEXRAD data from public S3 buckets

## Installation

### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later

### Setup

1. Clone the repository or download the source code
2. Navigate to the project directory
3. Run the appropriate build script:
   - Windows: `BuildAndRun.bat`
   - macOS/Linux: `./build.sh` (make executable with `chmod +x build.sh` if needed)

## Usage

1. Launch the application
2. Select a radar station from the dropdown list
3. Choose the radar product type (Reflectivity or Velocity)
4. Click "Refresh" to load the latest data
5. Toggle "Weather Warnings" to display active weather alerts
6. Enable "Auto Refresh" to automatically update radar data at regular intervals

## Key Components

- `MainWindow.axaml/.cs`: The primary user interface
- `NexradService.cs`: Core service for accessing radar data
- `RadarDataProcessor.cs`: Processes radar data into displayable format
- `Models/`: Contains data models for radar stations, products, and warnings

## Data Sources

This application fetches all data directly from official NWS sources:

- Radar data: NWS API and NOAA's public S3 buckets
- Station information: Built-in station database
- Weather warnings: NWS Alerts API

## Development

To set up the development environment:

1. Install [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
2. Install the [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later
3. Open the `NexradViewer.csproj` file in your IDE

## Building

- Debug build: `dotnet build`
- Release build: `dotnet build -c Release`
- Run application: `dotnet run`
- Create distributable: `dotnet publish -c Release -r <runtime>` where `<runtime>` is your target platform (e.g., `win-x64`, `osx-x64`, `linux-x64`)

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Data provided by the [National Weather Service](https://www.weather.gov/)
- [NEXRAD Technical Information](https://www.roc.noaa.gov/WSR88D/)
- [Avalonia UI Framework](https://avaloniaui.net/)
- [Mapsui Library](https://mapsui.com/)
