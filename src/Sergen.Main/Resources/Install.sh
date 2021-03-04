#!/bin/bash
cd "$(dirname "$0")"

systemctl stop Sergen.service
rm /lib/systemd/system/Sergen.service
cp Sergen.service /lib/systemd/system/Sergen.service

mkdir -p /opt/Sergen

yes | cp -rf ../* /opt/Sergen

systemctl daemon-reload
systemctl start Sergen.service
