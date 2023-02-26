Remove-Item ./build -Recurse -Force
mkdir build/SpaceWarp/core
Copy-Item ./Doorstop/* ./build
dotnet build SpaceWarp/SpaceWarp.csproj -c Debug
Copy-Item ./SpaceWarp/bin/Debug/SpaceWarp.dll ./build/SpaceWarp/core
Copy-Item ./SpaceWarp/bin/Debug/SpaceWarp.pdb ./build/SpaceWarp/core
Copy-Item ./SpaceWarp/bin/Debug/0Harmony.dll ./build/SpaceWarp/core
mkdir ./build/SpaceWarp/assets/bundles
mkdir ./build/SpaceWarp/Mods
Get-ChildItem ./Bundles -Filter *.bundle | Copy-Item -Destination ./build/SpaceWarp/assets/bundles/ -Force -PassThru