#!/bin/bash
echo "======================================"
echo "Building and Running NEXRAD Viewer..."
echo "======================================"

# Create Assets directory if it doesn't exist
if [ ! -d "Assets" ]; then
    mkdir -p Assets
fi

# Create a blank icon file if it doesn't exist
if [ ! -f "Assets/nexrad-icon.ico" ]; then
    echo "Creating placeholder icon file..."
    touch "Assets/nexrad-icon.ico"
fi

# Create Data directory if it doesn't exist
if [ ! -d "Data" ]; then
    mkdir -p Data
fi

# Ensure stations.json is in the Data directory
if [ ! -f "Data/stations.json" ]; then
    echo "Copying stations.json to Data directory..."
    cp "stations.json" "Data/stations.json" 2>/dev/null
fi

echo ""
echo "Building application..."
dotnet build -c Debug

if [ $? -ne 0 ]; then
    echo ""
    echo "Build failed with error code $?"
    echo ""
    read -p "Press Enter to continue..."
    exit $?
fi

echo ""
echo "Starting NEXRAD Viewer..."
echo ""
dotnet run -c Debug

read -p "Press Enter to continue..."
