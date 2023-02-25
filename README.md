![Top Banner](Cool Banner.png)

# Space Warp
Space Warp is a mod loader for kerbal space program 2

Note: Use at your own risk, this is an early version which is expected to have a lot of changes
# Compiling
In order to compile this project you need the code from kerbal space program 2, so before you can build anything, copy everything in ``Kerbal Space Program 2\KSP2_x64_Data\Managed`` into ``external_dlls/``

Then compile the `spacewarp` project, then run the `injector` project.

Then run KSP2, and wait until the title screen, there should then be a mods folder under the `KSP2_X64_data` folder

Then drag any mods into that folder that follow the structure below

Mods are just monobehaviours for now, with 2 fields, Logger for Logging, and Manager that points to Spacewarp

A mod template generator exists as a python script


## Mods structure
Still in progress
* KSP2_directory/KSP2_x64_Data/Mods
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
    * bin/
    * config/

