Remove-Item ./build -Recurse -Force
mkdir build/SpaceWarp/core
Copy-Item ./Doorstop/* ./build
dotnet build SpaceWarp/SpaceWarp.csproj -c Release
Copy-Item ./SpaceWarp/bin/Release/SpaceWarp.dll ./build/SpaceWarp/core
Copy-Item ./SpaceWarp/bin/Release/0Harmony.dll ./build/SpaceWarp/core
mkdiR ./build/SpaceWarp/assets/bundles
Get-ChildItem ./Bundles -Filter *.bundle | Copy-Item -Destination ./build/SpaceWarp/assets/bundles/ -Force -PassThru