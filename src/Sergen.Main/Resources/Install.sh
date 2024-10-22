#!/bin/bash
cd "$(dirname "$0")"

# Stop the service if it is running
systemctl stop Sergen.service

# Check if the systemd service file exists before removing it
if [ -f /lib/systemd/system/Sergen.service ]; then
    rm /lib/systemd/system/Sergen.service
    echo "Removed existing Sergen.service file."
else
    echo "Sergen.service file does not exist, skipping removal."
fi

# Copy the new service file
cp Sergen.service /lib/systemd/system/Sergen.service

# Ensure the target directory exists
mkdir -p /opt/Sergen

# Copy all files to /opt/Sergen
yes | cp -rf ../* /opt/Sergen

# Reload systemd services and start the service
systemctl daemon-reload
systemctl start Sergen.service
