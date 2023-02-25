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

print("Space Warp Mod Setup Wizard")
project_name = input("What would you like to name the project: ")
mod_id = input("What is the ID of the mod: ")
mod_author = input("Who is the author of the mod: ")
mod_name = input("What is the name of the mod: ")
mod_description = input("What is a short description of the mod: ")
mod_source = input("What is the source link of the mod: ")
mod_version = input("What is the starting version of the mod: ")
mod_ksp_min_version = input("What is the minimum version of KSP this mod will accept: ")
mod_ksp_max_version = input("What is the maximum version of KSP this mod will accept: ")

os.mkdir(project_name)
os.mkdir(project_name + "/" + mod_id)
os.mkdir(project_name + "/" + mod_id + "/assets")
os.mkdir(project_name + "/" + mod_id + "/assets/parts")
os.mkdir(project_name + "/" + mod_id + "/assets/models")
os.mkdir(project_name + "/" + mod_id + "/assets/resources")
os.mkdir(project_name + "/" + mod_id + "/bin")
os.mkdir(project_name + "/" + mod_id + "/config")
namespace = mod_id.replace("_", " ").title().replace(" ", "")
os.mkdir(project_name + "/" + project_name)
os.mkdir(project_name + "/" + project_name + "/" + namespace)
os.mkdir(project_name + "/external_dlls")

external_dlls = project_name + "/external_dlls"
release_folder = project_name + "/" + mod_id


# Now we copy all the game directories
space_warp_path = input("What is the path to spacewarp.dll: ")
managed_path = input("What is the path to the KSP managed dlls: ")

shutil.copy2(space_warp_path,external_dlls)

for filename in os.listdir(managed_path):
    if filename.endswith(".dll"):
        shutil.copy2(os.path.join(managed_path,filename),external_dlls)

with open(external_dlls + "/.gitignore","w") as external_gitignore:
    external_gitignore.write("*\n!.gitignore")

with open(project_name + "/.gitignore","w") as main_gitignore:
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

with open(release_folder + "/modinfo.json","w") as modinfo:
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
with open(release_folder + "README.json","w") as readme:
    readme.write("# Usage")
    readme.write("Any code compiled in the csproj's dll should be moved to the /bin folder of the mod")

with open(release_folder + "/README.json","w") as readme:
    readme.write("# Default Readme")
code_folder = project_name + "/" + project_name + "/" + namespace
with open(code_folder + "/" + namespace + "Mod.cs","w") as default_code:
    default_code.write("using SpaceWarp.API.Mods;\n\nnamespace " + namespace + "\n{\n    [MainMod]\n     public class " + namespace + "Mod : Mod\n    {\n        public override void Initialize()\n        {\n            Logger.Info(\"Mod is initialized\");\n        }\n    }\n}")

with open(code_folder + "/" + namespace + "Config.cs","w") as default_config:
    default_config.write("using SpaceWarp.API.Configuration;\nusing Newtonsoft.Json;\n\nnamespace " + namespace + "\n{\n    [JsonObject(MemberSerialization.OptOut)]\n    [ModConfig]\n    public class " + namespace + "Config\n    {\n         [ConfigField(\"pi\")] [ConfigDefaultValue(3.14159)] public double pi;\n    }\n}")


def quickCreateProperty(root,name,text):
    a = root.createElement(name)
    b = root.createTextNode(text)
    a.appendChild(b)
    return a



with open(project_name + "/" + project_name + "/" + project_name + ".csproj","w") as csproj:
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

    ref1 = root.createElement('Reference')
    ref1.setAttribute("Include","..\\external_dlls\\SpaceWarp.dll")
    itemGroup.appendChild(ref1)

    ref2 = root.createElement('Reference')
    ref2.setAttribute("Include","..\\external_dlls\\UnityEngine.dll")
    itemGroup.appendChild(ref2)

    ref3 = root.createElement('Reference')
    ref3.setAttribute("Include","..\\external_dlls\\UnityEngine.CoreModule.dll")
    itemGroup.appendChild(ref3)

    ref4 = root.createElement('Reference')
    ref4.setAttribute("Include","..\\external_dlls\\Assembly-CSharp.dll")
    itemGroup.appendChild(ref4)

    ref5 = root.createElement('Reference')
    ref5.setAttribute("Include","..\\external_dlls\\NewtonSoft.Json.dll")
    itemGroup.appendChild(ref5)
    
    ref6 = root.createElement('Reference')
    ref6.setAttribute("Include","..\\external_dlls\\NewtonSoft.Json.dll")
    itemGroup.appendChild(ref6)

    xml_str = root.toprettyxml(indent = '  ')
    csproj.write(xml_str)