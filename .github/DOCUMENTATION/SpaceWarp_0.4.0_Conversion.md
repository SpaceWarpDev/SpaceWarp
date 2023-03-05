# SpaceWarp 1.0.0 Conversion Document

[SpaceWarpDev/SpaceWarp](https://github.com/SpaceWarpDev/SpaceWarp)

### Folder Structure
The previous mod structure has been replaced by the standard BepInEx layout.

>[KSP2_ROOT]/BepInEx/Plugins

    mod_id_folder_name
        swinfo.json
        README.md
        assets/
            bundles/
                *.bundle
            images/
                *
        localization/
            *.csv
        addressables/
            catalog.json
            *
        *.dll
####
### swinfo.json (Your new modinfo.json)
```
{
  "mod_id": "mod_folder_name_here",
  "name": "full_title_here",
  "author": "your_name",
  "description": "description",
  "source": "link_to_github_if_you_want",
  "version": "x.x.x",
  "dependencies": [],
  "ksp2_version": {
    "min": "0.1.0",
    "max": "0.1.0"
  }
}
```
####
### Required References
* We now REQUIRE BepInEx 5.4.21 referenced when building your package. This can be included by added this reference line to your .csproj or other build reference manager
```
<PackageReference Include="BepInEx.BaseLib" Version="5.4.21" Publicize="true" />
```
* You will need to update your SpaceWarp.dll file in your DLL folder to include the newest features, abstractions, and the UI changeover


### Conversion Process
1. At the top of your main class file, you will need to add ```using BepInEx;```
####
2. Replace the ```[Mod]``` tag with two lines containing the correct BepInEx mod information
a. 
    ```[BepInPlugin("com.location.name.modid", "modID"", "x.x.x")]``` (This should match the swinfo.json file)
    ```[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]``` (You do not need to change this line)
####
3. Change ```public class yourModName : Mod``` to the new plugin type ```public class yourModName : BaseSpaceWarpPlugin```
####
4. Create an Instance of the mod as a variable at the top of your mod function
```
public class youModName : BaseSpaceWarpPlugin
    {
        private static yourModName Instance { get; set; }
        
        ...all your other code...
    }
```
####
5. Override the BaseSpaceWarpPlugin OnInitialized function can set your instance
```
public override void OnInitialized()
        {
            base.OnInitialized();
            Instance = this;
        }
```
####
6. If you are using the SpaceWarpUI Skin, you will need to add ```using SpaceWarp.API.UI;``` to the top of your file and set ```GUI.skin = Skins.ConsoleSkin;``` in the ```OnGUI()``` function
####
7. If you are using the AppBar
a. Add your icon to the assets/images folder in the file tree as a .png file
b. Add ```using SpaceWarp.API.UI.Appbar``` to the top of your project
c. Set your UI elements to enable with the ```drawGUI``` bool
d. In the ```OnItialized()``` function add the code to draw the Appbar button and a new ```ToggleButton`` function
```
Appbar.RegisterAppButton(
            "Mod Name Text", 
            "BTN-ButtonReferenceName",
            AssetManager.GetAsset<Texture2D>($"{SpaceWarpMetadata.ModID}/images/icon.png"),
            ToggleButton
            );
```
```
private void ToggleButton(bool toggle)
    {
        drawUI = toggle;
        GameObject.Find("BTN-ExampleMod")?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(toggle);
    }
```

#
#### Feel free to add comments and feedback!

v1.0
```By AdmiralRadish#420```
**Free Software, Hell Yeah!**
