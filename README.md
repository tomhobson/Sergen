[![CodeScene Code Health](https://codescene.io/projects/9083/status-badges/code-health)](https://codescene.io/projects/9083) ![.NET Build And Release](https://github.com/tomhobson/Sergen/workflows/.NET%20Build%20And%20Release/badge.svg) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

# Sergen
Game server generation with curated images orchestrated by discord chat.

<img src="https://github.com/tomhobson/Sergen/blob/master/screenshots/startandstop.gif" alt="Animation of Sergen working" width="45%">

## Requirements
* Docker
* Dotnet 3.1 runtime
* completely open firewall (or at least all the ports you want to use open)

## Installation
A systemd service is the recommended way to run Sergen. I create and run the service within the install scripts. If you don't want it setup this way, don't use the install scripts.

* Download the latest build from the releases section. Extract it, then run ./Install.sh
* Clone the git repo and run BuildAndInstall.sh

## Setup

You'll need to setup a bot with discord, found here: https://discord.com/developers/applications

Here's an article on how to do it: https://www.howtogeek.com/364225/how-to-make-your-own-discord-bot/ (just do up before the Node.js part)

Get your token and update src/Sergen.Master/appsettings.json.
Replace PUTBOTTOKENHERE with the token generated from https://discord.com/developers/applications.

You'll need to restart the service and invite your bot, which can be done with this link:
https://discord.com/oauth2/authorize?client_id=123456789&scope=bot

Replace 123456789 with your bot's client id.

