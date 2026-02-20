#!/bin/bash

# Exit on error
set -e

echo "Compiling OFIQ C++ Bridge..."
# We need to link against libofiq_lib.so.
# libonnxruntime is likely a dependency of libofiq_lib.so.
g++ -shared -fPIC ofiq_bridge.cpp -o libofiq_bridge.so -L. -lofiq_lib -I. -Wl,-rpath,.

echo "Restoring and Building C# Console App..."
cd OFIQConsoleApp
dotnet publish -c Release -r linux-x64 --self-contained false
cd ..

# Copy libraries to the publish directory
echo "Copying libraries to publish folder..."
PUBLISH_DIR="OFIQConsoleApp/bin/Release/net10.0/linux-x64/publish/"
cp libofiq_lib.so "$PUBLISH_DIR"
cp libonnxruntime.so.1.18.1 "$PUBLISH_DIR"
cp libofiq_bridge.so "$PUBLISH_DIR"

# Create symlink for onnxruntime if needed
ln -sf libonnxruntime.so.1.18.1 "$PUBLISH_DIR/libonnxruntime.so"

echo "------------------------------------------------"
echo "Setup complete!"
echo "To run the application:"
echo "cd $PUBLISH_DIR"
echo "./OFIQConsoleApp <path_to_image> <config_dir> <config_file>"
echo "------------------------------------------------"
