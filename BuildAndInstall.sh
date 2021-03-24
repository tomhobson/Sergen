#!/bin/bash
cd "$(dirname "$0")"

systemctl stop Sergen.service
rm /lib/systemd/system/Sergen.service
cp Sergen.service /lib/systemd/system/Sergen.service

mkdir -p /opt/Sergen
dotnet build Sergen.sln -c Release

yes | cp -rf src/Sergen.Master/bin/Release/netcoreapp5.0/* /opt/Sergen

systemctl daemon-reload
systemctl start Sergen.service
