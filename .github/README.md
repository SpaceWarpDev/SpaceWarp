![Cool Banner](assets/cool_banner.png)


# Build Status
[![Main Branch](https://github.com/SpaceWarpDev/SpaceWarp/actions/workflows/build_spacewarp.yml/badge.svg?branch=main)](https://github.com/SpaceWarpDev/SpaceWarp/actions/workflows/build_spacewarp.yml)

# Space Warp
[![Curseforge](http://cf.way2muchnoise.eu/full_831005_downloads.svg?badge_style=plastic)](https://www.curseforge.com/kerbal-space-program-2/mods/space-warp)
![Downloads](https://img.shields.io/github/downloads/X606/SpaceWarp/latest/total.svg?label=%E2%A4%93%20Downloads&style=plastic)  
![SpaceDock Downloads](https://img.shields.io/badge/dynamic/json?color=blueviolet&label=SpaceDock%20Downloads&query=downloads&url=https%3A%2F%2Fspacedock.info%2Fapi%2Fmod%2F3277)

[Documentation](https://docs.spacewarp.org)

Space Warp is a modding API for Kerbal Space Program 2 with support for BepInEx and the official mod loader.*

*\*Note: The official mod loader is unfinished, so BepInEx is currently still required to enable it.*

## Installation

**It is highly recommended to use CKAN [Download Here](https://github.com/KSP-CKAN/CKAN) to install Space Warp**

If you for some reason want to manually install it, here are the instructions:

Note: *Don't worry about the BepInEx installation, it already comes in the zip folder!*

Download the latest release of [UITK for KSP 2](https://github.com/jan-bures/UitkForKsp2/releases).

Drag the contents of UitkForKsp2's .zip file into your KSP2 directory, commonly located at `C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program 2`.

Download the latest Space Warp release from this page under the [Releases](https://github.com/SpaceWarpDev/SpaceWarp/releases) section above.

Drag the contents of Space Warp's .zip file into your KSP2 directory, commonly located at `C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program 2`.

In the case of installing on Linux under Wine/Proton, you need to override winhttp DLL in winecfg, as described [in BepInEx documentation](https://docs.bepinex.dev/articles/advanced/proton_wine.html).

To install the downloaded mods, simply drag them into the plugins folder location: `C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program 2\BepInEx\plugins`. Remember that mods don't go into the SpaceWarp plugin folder, they go along side with it in the `BepInEx\plugins` folder.

That should be it, you can now launch the game and enjoy!

## Compiling

To compile this project, you will need to follow these steps:

1. Install .NET 7+ SDK
2. Run `dotnet restore` inside the top directory to install the packages.
3. Run `dotnet build -c <Configuration>` to build the project, where `<Configuration>` is one of the following:
   - `Debug` - Builds the project in debug mode
   - `Deploy` - Builds the project in debug mode and copies the output to the KSP2 directory
   - `DeployAndRun` - Builds the project in debug mode, copies the output to the KSP2 directory, and runs the game
   - `Release` - Builds the project in release mode, zips the output, and builds a NuGet package

or you can use Visual Studio 2022 or JetBrains Rider to build the project.

There are also scripts in the `scripts` folder that can be used to build the project in each of the configurations, they simply run the `dotnet build` command with the correct arguments.

The outputs can be found in `dist/<Configuration>`. The Release zip will be in the `dist` folder, and the NuGet package will be in the `nuget` folder.

## Mod Structure

The mod structure is still a work in progress. However, the current structure is as follows:

```
KSP2_Root_Folder/
├── BepInEx/
│   ├── Plugins/
│   │   ├── mod_id_folder_name/
│   │   │   ├── swinfo.json
│   │   │   ├── README.md
│   │   │   ├── assets/
│   │   │   │   ├── bundles/
│   │   │   │   │   ├── *.bundle
│   │   │   │   ├── images/
│   │   │   │   │   ├── *
│   │   │   │   ├── soundbanks/
│   │   │   │   │   ├── *.bnk
│   │   │   ├── localizations/
│   │   │   │   ├── *.csv
│   │   │   ├── addressables/
│   │   │   │   ├── catalog.json
│   │   │   │   ├── *
│   │   │   ├── *.dll 
```