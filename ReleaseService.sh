systemctl stop Sergen.service
rm /lib/systemd/system/Sergen.service
cp Sergen.service /lib/systemd/system/Sergen.service
systemctl daemon-reload
systemctl start Sergen.service
