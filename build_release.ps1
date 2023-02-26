Remove-Item ./build -Recurse -Force
mkdir build/SpaceWarp/core
Copy-Item ./Doorstop/* ./build
dotnet build SpaceWarp/SpaceWarp.csproj -c Release
Copy-Item ./SpaceWarp/bin/Release/SpaceWarp.dll ./build/SpaceWarp/core
Copy-Item ./SpaceWarp/bin/Release/0Harmony.dll ./build/SpaceWarp/core
Copy-Item ./SpaceWarp/bin/Release/Mono.Cecil.dll ./build/SpaceWarp/core
Copy-Item ./SpaceWarp/bin/Release/Mono.Cecil.Mdb.dll ./build/SpaceWarp/core
Copy-Item ./SpaceWarp/bin/Release/Mono.Cecil.Pdb.dll ./build/SpaceWarp/core
Copy-Item ./SpaceWarp/bin/Release/Mono.Cecil.Rocks.dll ./build/SpaceWarp/core
Copy-Item ./SpaceWarp/bin/Release/MonoMod.RuntimeDetour.dll ./build/SpaceWarp/core
Copy-Item ./SpaceWarp/bin/Release/MonoMod.Utils.dll ./build/SpaceWarp/core
mkdir ./build/SpaceWarp/assets/bundles
mkdir ./build/SpaceWarp/Mods
Get-ChildItem ./Bundles -Filter *.bundle | Copy-Item -Destination ./build/SpaceWarp/assets/bundles/ -Force -PassThru
