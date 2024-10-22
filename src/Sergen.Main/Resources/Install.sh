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

# Check if apt package manager exists on the system
if command -v apt > /dev/null; then
    echo "apt found, checking for podman and dotnet6."

    # Update package lists
    apt update

    # Check if podman is installed
    if ! dpkg -l | grep -q podman; then
        echo "podman not found, installing."
        apt install -y podman
    else
        echo "podman is already installed, skipping installation."
    fi

    # Check if dotnet6 is installed
    if ! dpkg -l | grep -q dotnet-runtime-6.0; then
        echo "dotnet-runtime-6.0 not found, installing."
        apt install -y dotnet-runtime-6.0
    else
        echo "dotnet-runtime-6.0 is already installed, skipping installation."
    fi

    echo "podman and dotnet-runtime-6.0 installation checks complete."
else
    echo "apt not found. Skipping podman and dotnet6 installation."
fi
