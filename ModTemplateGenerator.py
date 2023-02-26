# -------------------------------------- #
# KSP2 Space Warp Mod Template Generator #
# -------------------------------------- #
import shutil, json
from xml.dom import minidom
# Generates a structure like the following
# mod_project
#   mod_id
#       modinfo.json
#       README.md
#       assets/
#           parts/
#           models/
#           resources/
#       bin/
#       config/
#   ModId
#       Mod.cs
#       Config.cs
#   mod_project.csproj
#   external_dlls/
#       .gitignore
#   .gitignore
#
#
#.gitignore
# *.rsuser
# *.suo
# *.user
# *.userosscache
# *.sln.docstates
# *.userprefs
# mono_crash.*
# [Dd]ebug/
# [Dd]ebugPublic/
# [Rr]elease/
# [Rr]eleases/
# x64/
# x86/
# [Aa][Rr][Mm]/
# [Aa][Rr][Mm]64/
# bld/
# [Bb]in/
# [Oo]bj/
# [Ll]og/
# [Ll]ogs/
#
#external_dlls/.gitignore
# *
# !.gitignore

import os
from os.path import expanduser


def find_ksp2_install_path():
    # Look for the game in Steam library folders
    steam_path = os.path.join(os.getenv("ProgramFiles(x86)"), "Steam")
    steam_library_folders_file = os.path.join(steam_path, "steamapps", "libraryfolders.vdf")
    steam_install_folder = os.path.join(steam_path, "steamapps", "common", "Kerbal Space Program 2")

    if os.path.exists(steam_library_folders_file):
        with open(steam_library_folders_file) as f:
            for line in f:
                if "BaseInstallFolder" in line:
                    steam_library_path = line.strip().split("\"")[3]
                    if os.path.exists(os.path.join(steam_library_path, "steamapps", "appmanifest_1406800.acf")):
                        steam_install_folder = os.path.join(steam_library_path, "steamapps", "common", "Kerbal Space Program 2")
                        break

    # Look for the game in default installation path
    if not os.path.exists(steam_install_folder):
        default_install_folder = os.path.join(os.getenv("ProgramFiles"), "Private Division", "Kerbal Space Program 2")
        if os.path.exists(default_install_folder):
            steam_install_folder = default_install_folder

    return steam_install_folder

print("Space Warp Mod Setup Wizard")

# while True:
#     project_name = input("What would you like to name the project: ")
#     if not project_name:
#         print("Project name cannot be empty, please try again.")
#     else:
#         break

while True:
    mod_id = input("What is the ID of the mod (This should be in snake_case): ")
    if not mod_id:
        print("Mod ID cannot be empty, please try again.")
    else:
        break

while True:
    mod_author = input("Who is the author of the mod: ")
    if not mod_author:
        print("Mod author cannot be empty, please try again.")
    else:
        break

while True:
    mod_name = input("What is the name of the mod: ")
    if not mod_name:
        print("Mod name cannot be empty, please try again.")
    else:
        break

mod_description = input("What is a short description of the mod: ")
mod_source = input("What is the source link of the mod: ")
mod_version = input("What is the starting version of the mod: ")
mod_ksp_min_version = input("What is the minimum version of KSP2 this mod will accept: ")
mod_ksp_max_version = input("What is the maximum version of KSP2 this mod will accept: ")

steam_install_folder = find_ksp2_install_path()

if os.path.exists(steam_install_folder):
    print(f"Kerbal Space Program 2 is installed at {steam_install_folder}")
else:
    managed_path = input("Could not find the installation path for Kerbal Space Program 2.\n Please enter the path to the KSP2 installation folder manually: ")

managed_path = os.path.join(steam_install_folder, "KSP2_x64_Data", "Managed")


mod_id_title = mod_id.replace("_", " ").title().replace(" ", "")
os.mkdir(mod_id)
os.mkdir(f"{mod_id}/{mod_id}")
os.mkdir(f"{mod_id}/{mod_id}/assets")
os.mkdir(f"{mod_id}/{mod_id}/assets/parts")
os.mkdir(f"{mod_id}/{mod_id}/assets/models")
os.mkdir(f"{mod_id}/{mod_id}/assets/resources")
os.mkdir(f"{mod_id}/{mod_id}/bin")
os.mkdir(f"{mod_id}/{mod_id}/config")
os.mkdir(f"{mod_id}/{mod_id_title}")
os.mkdir(f"{mod_id}/{mod_id_title}/{mod_id_title}")
os.mkdir(f"{mod_id}/external_dlls")

external_dlls = f"{mod_id}/external_dlls"
release_folder = f"{mod_id}/{mod_id}"

space_warp_path = os.path.join(managed_path, "SpaceWarp.dll")

shutil.copy2(space_warp_path,external_dlls)

for filename in os.listdir(managed_path):
    if filename.endswith(".dll"):
        shutil.copy2(os.path.join(managed_path,filename),external_dlls)

with open(f"{external_dlls}/.gitignore","w") as external_gitignore:
    external_gitignore.write("*\n!.gitignore")

with open(f"{mod_id}/.gitignore","w") as main_gitignore:
    main_gitignore.writelines(
        [
            "*.rsuser",
            "*.suo",
            "*.user",
            "*.userosscache",
            "*.sln.docstates",
            "*.userprefs",
            "mono_crash.*",
            "[Dd]ebug/",
            "[Dd]ebugPublic/",
            "[Rr]elease/",
            "[Rr]eleases/",
            "x64/",
            "x86/",
            "[Aa][Rr][Mm]/",
            "[Aa][Rr][Mm]64/",
            "bld/",
            "[Bb]in/",
            "[Oo]bj/",
            "[Ll]og/",
            "[Ll]ogs/",
        ]
    )

with open(f"{release_folder}/modinfo.json","w") as modinfo:
    modinfo.write(
        json.dumps({
            "mod_id": mod_id,
            "author": mod_author,
            "name": mod_name,
            "description": mod_description,
            "source": mod_source,
            "version": mod_version,
            "dependencies": [],
            "ksp2_version": {
                "min": mod_ksp_min_version,
                "max": mod_ksp_max_version
            }
        },indent=4)
    )
with open(f"{release_folder}README.json","w") as readme:
    readme.write("# Usage")
    readme.write("Any code compiled in the csproj's dll should be moved to the /bin folder of the mod")

with open(f"{release_folder}/README.json","w") as readme:
    readme.write("# Default Readme")

code_folder = f"{mod_id}/{mod_id_title}/{mod_id_title}"

with open(f"{code_folder}/{mod_id_title}Mod.cs","w") as default_code:
    default_code.write("using SpaceWarp.API.Mods;\n\nnamespace " + mod_id_title + "\n{\n    [MainMod]\n     public class " + mod_id_title + "Mod : Mod\n    {\n        public override void OnInitialized()\n        {\n            Logger.Info(\"Mod is initialized\");\n        }\n    }\n}")

with open(f"{code_folder}/{mod_id_title}Config.cs","w") as default_config:
    default_config.write("using SpaceWarp.API.Configuration;\nusing Newtonsoft.Json;\n\nnamespace " + mod_id_title + "\n{\n    [JsonObject(MemberSerialization.OptOut)]\n    [ModConfig]\n    public class " + mod_id_title + "Config\n    {\n         [ConfigField(\"pi\")] [ConfigDefaultValue(3.14159)] public double pi;\n    }\n}")


def quickCreateProperty(root,name,text):
    a = root.createElement(name)
    b = root.createTextNode(text)
    a.appendChild(b)
    return a


with open(f"{mod_id}/{mod_id_title}/{mod_id_title}.csproj","w") as csproj:
    root = minidom.Document()
    xml = root.createElement('Project')
    xml.setAttribute('Sdk','Microsoft.NET.Sdk')
    root.appendChild(xml)
    propertyGroup = root.createElement('PropertyGroup')
    xml.appendChild(propertyGroup)
    propertyGroup.appendChild(quickCreateProperty(root,"TargetFramework","netstandard2.0"))
    propertyGroup.appendChild(quickCreateProperty(root,"AllowUnsafeBlocks","true"))
    propertyGroup.appendChild(quickCreateProperty(root,"LangVersion","11"))
    propertyGroup.appendChild(quickCreateProperty(root,"ImplicitUsings","true"))
    
    itemGroup = root.createElement('ItemGroup')
    xml.appendChild(itemGroup)

    refs = [
        "..\\external_dlls\\SpaceWarp.dll",
        "..\\external_dlls\\UnityEngine.dll",
        "..\\external_dlls\\UnityEngine.CoreModule.dll",
        "..\\external_dlls\\Assembly-CSharp.dll",
        "..\\external_dlls\\NewtonSoft.Json.dll",
        "..\\external_dlls\\NewtonSoft.Json.dll"
    ]

    for ref in refs:
        element = root.createElement('Reference')
        element.setAttribute("Include", ref[0])
        itemGroup.appendChild(element)

    xml_str = root.toprettyxml(indent = '  ')
    csproj.write(xml_str)
