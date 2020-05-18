![.NET Build And Release](https://github.com/tomhobson/Sergen/workflows/.NET%20Build%20And%20Release/badge.svg) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

# Sergen
Game server generation with curated images orchestrated by discord chat.

<img src="https://github.com/tomhobson/Sergen/blob/master/screenshots/screenshot.png" alt="Screenshot of Sergen working" width="45%">

## Requirements
* Docker
* Dotnet 3.1 runtime
* completely open firewall (or at least all the ports you want to use open)

## Installation
A systemd service is the recommended way to run Sergen. I create and run the service within the install scripts. If you don't want it setup this way, don't use the install scripts.

* Download the latest build from the releases section. Extract it, then run ./Install.sh
* Clone the git repo and run BuildAndInstall.sh

