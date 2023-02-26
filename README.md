![Cool Banner](Cool_Banner.png)

# Space Warp
![Downloads](https://img.shields.io/github/downloads/X606/SpaceWarp/latest/total.svg?label=%E2%A4%93Download&style=plastic)

Space Warp is a mod loader for Kerbal Space Program 2.

Note: Use at your own risk, as this is an early version that is expected to undergo many changes.

## Compiling

To compile this project, you will need to follow these steps:

1. Copy everything in the ``Kerbal Space Program 2\KSP2_x64_Data\Managed`` folder into the ``external_dlls/`` folder.
2. Run one of the build scripts and copy the contents from the build to the KSP2 root directory.
3. Launch KSP2 and wait until the title screen appears. You should see a mods folder under the `KSP2_X64_data` folder.
4. Drag any mods that follow the structure below into that mods folder.

Mods are currently implemented as monobehaviours with two fields: a `Logger` for logging and a `Manager` that points to Spacewarp. A mod template generator exists as a Python script.

## Mod Structure

The mod structure is still a work in progress. However, the current structure is as follows:

* [KSP_ROOT]/SpaceWarp/Mods
  * example_mod
    * modinfo.json
    * README.md
    * assets/
      * assembly/
        * parts/
            * *.json
        * models/
            * *.json
        * resources/
            * *.json
        * bundles/
            * *.bundle
    * bin/
    * config/
    * .ignore (optional, if this file is present, the mod will be skipped!)
