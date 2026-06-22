# Clevo Fan Control

<p align="center">
  <img src="https://i.ibb.co/KxQbYSC2/Clevo-Fan-Control-22-06-2026-07-47-45.png" alt="Clevo Fan Control Screenshot" />
</p>

A lightweight fan control utility for Clevo laptops.

This project is a streamlined fork of the original Clevo Fan Control developed by djsubtronic and sukh-consultdolphin, focusing on simplicity.

## Features

- Manual fan speed control
- Lightweight and minimal interface
- No unnecessary background services

## Changes & Improvements
Updated to .NET Framework 4.8

Updated solution to Visual Studio 18

Added more static fan profiles

Removed unnecessary features, bloated code, and unused functionality

improved EC error handling

Stability / Reliability Improvements

Increased EC polling interval to 10s to reduce timeouts

Serialized EC operations using SemaphoreSlim to prevent conflicts

Switched CPU temperature readings from EC to WMI for improved reliability

Removed fan ramping logic and now applies fan speed changes directly with a delay

Refactored FanTable into a class with a CreateConstant initializer

Removed CPU/GPU safety temperature controls and related variables

Adjusted async fan ramping behavior to discard unawaited tasks, eliminating compiler warnings

Removed timerTickCount and related usage

Deleted CalcFanPercentage

Removed unused/commented fan ramping and shutdown code

Removed max CPU/GPU temperature tracking and display

Increase GUI update interval, optimize WMI temperature reads

Skip GUI updates when window is minimized or hidden.

## Download

Precompiled releases are available from the project's Releases page:

https://github.com/Zyth88/ClevoFanControl/releases

## Installation

### 1. Install the NTPort Driver

Download and install:

https://github.com/Zyth88/ClevoFanControl/blob/master/ClevoFanControl/NTPortDrvSetup.exe

### 2. Install Clevo Fan Control

1. Download the latest release.
2. Extract the archive to a folder of your choice.
3. Run `ClevoFanControl.exe`.
4. The application will start minimized in the system tray.

## Credits

Original project by:
- djsubtronic
- sukh-consultdolphin

Maintained by:
- Zyth