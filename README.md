# vATMS - virtual Air Traffic Management System

A basic simulation of the interim Future Area Control Tools (iFACTS) used in the London Area Control Centre.

## Running/building from the CLI

### Requirements
- .NET 8 or later
- NodeJS v22 or later

### Setup

Install the following NuGet packages in the `/vATMS.Server` directory

```bash
dotnet add package ElectronNET.Core
dotnet add package ElectronNET.Core.AspNet
```
To run the program, run the following command in the base directory or the `/vATMS.Server` directory
```bash
electronize start
```
The required npm packages should install automatically on the first build/run. If they do not install automatically, run the following command in the `/vatms.client` directory
```bash
npm i
```
To build and package the program for Windows, run the following command in the base directory or the `/vATMS.Server` directory
```bash
electronize build /target win
```
> Builds for othe platforms have not been tested

## Tutorial
