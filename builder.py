import argparse
import os
import shutil
import subprocess
import zipfile

DOORSTOP_DIR = os.path.abspath("Doorstop")
BUILD_DIR = os.path.abspath("build")
BUNDLES_DIR = os.path.abspath("Bundles")
SPACEWARP_DIR = os.path.abspath("SpaceWarp")

parser = argparse.ArgumentParser()

parser.add_argument("-r", "--release", help="Build a release version", action="store_true")
parser.add_argument("-t", "--target", help="Build target", choices=["bepinex", "doorstop"], default="doorstop")
parser.add_argument("-a", "--all", help="Build all targets", action="store_true")

def clean():
    if os.path.exists(BUILD_DIR):
        shutil.rmtree(BUILD_DIR)
    
    if os.path.exists(os.path.join(SPACEWARP_DIR, "bin")):
        shutil.rmtree(os.path.join(SPACEWARP_DIR, "bin"))
    
    if os.path.exists(os.path.join(SPACEWARP_DIR, "obj")):
        shutil.rmtree(os.path.join(SPACEWARP_DIR, "obj"))

def do_nuget_source():
    sources_output = subprocess.run(["nuget", "sources", "list"], capture_output=True)
    
    if not "bepinex" in str(sources_output.stdout, "utf-8").lower():
        print("=> Adding BepInEx source to Nuget config...")
        
        nuget_src_out = subprocess.run(["nuget", "sources", "add", "-Name", "BepInEx", "-Source", "https://nuget.bepinex.dev/v3/index.json"], capture_output=True)
        
        print("    |=>| STDOUT")
    
        for line in str(nuget_src_out.stdout, "utf-8").splitlines():
            print(f"        {line}")
        
        print("    |=>| STDERR")
    
        for line in str(nuget_src_out.stderr, "utf-8").splitlines():
            print(f"        {line}")
    
    print("=> Restoring project dependencies...")
    
    nuget_out = subprocess.run(["nuget", "restore"], capture_output=True)
    
    print("    |=>| STDOUT")
    
    for line in str(nuget_out.stdout, "utf-8").splitlines():
        print(f"        {line}")
        
    print("    |=>| STDERR")
    
    for line in str(nuget_out.stderr, "utf-8").splitlines():
        print(f"        {line}")

def build(release = False, doorstop = False):
    build_type = "Doorstop" if doorstop else "BepInEx"
    dotnet_args = ["dotnet", "build", os.path.join(SPACEWARP_DIR, "SpaceWarp.csproj"), "-c", "Release" if release else "Debug"]
    build_output_dir = os.path.join(SPACEWARP_DIR, "bin", "Release" if release else "Debug")
    output_dir = os.path.join(BUILD_DIR, build_type, "BepInEx", "plugins", "SpaceWarp")
    
    do_nuget_source()
    
    if doorstop:
        dotnet_args.append("-p:DefineConstants=\"DOORSTOP_BUILD\"")
        output_dir = os.path.join(BUILD_DIR, build_type, "SpaceWarp", "core")
    
    print("=> Creating build directory...")
    
    os.makedirs(output_dir)
    
    if doorstop:
        print("=> Copying doorstop DLLs...")
        
        for file in os.listdir(DOORSTOP_DIR):
            shutil.copyfile(os.path.join(DOORSTOP_DIR, file), os.path.join(BUILD_DIR, build_type, file))
    
    print(f"=> Executing: {' '.join(dotnet_args)}")
    
    output = subprocess.run(args=dotnet_args, capture_output=True)
    
    print("    |=>| STDOUT")
    
    for line in str(output.stdout, "utf-8").splitlines():
        print(f"        {line}")
        
    print("    |=>| STDERR")
    
    for line in str(output.stderr, "utf-8").splitlines():
        print(f"        {line}")
    
    print("=> Copying build outputs...")
    
    if doorstop:
        shutil.copyfile(os.path.join(build_output_dir, "0Harmony.dll"), os.path.join(output_dir, "0Harmony.dll"))
        shutil.copyfile(os.path.join(build_output_dir, "0Harmony.xml"), os.path.join(output_dir, "0Harmony.xml"))
        
        shutil.copyfile(os.path.join(build_output_dir, "Mono.Cecil.dll"), os.path.join(output_dir, "Mono.Cecil.dll"))
        shutil.copyfile(os.path.join(build_output_dir, "Mono.Cecil.Mdb.dll"), os.path.join(output_dir, "Mono.Cecil.Mdb.dll"))
        shutil.copyfile(os.path.join(build_output_dir, "Mono.Cecil.Pdb.dll"), os.path.join(output_dir, "Mono.Cecil.Pdb.dll"))
        shutil.copyfile(os.path.join(build_output_dir, "Mono.Cecil.Rocks.dll"), os.path.join(output_dir, "Mono.Cecil.Rocks.dll"))
        shutil.copyfile(os.path.join(build_output_dir, "MonoMod.RuntimeDetour.dll"), os.path.join(output_dir, "MonoMod.RuntimeDetour.dll"))
        shutil.copyfile(os.path.join(build_output_dir, "MonoMod.Utils.dll"), os.path.join(output_dir, "MonoMod.Utils.dll"))
        
    if not release and os.path.exists(os.path.join(build_output_dir, "SpaceWarp.pdb")):
        shutil.copyfile(os.path.join(build_output_dir, "SpaceWarp.pdb"), os.path.join(output_dir, "SpaceWarp.pdb"))
    
    shutil.copyfile(os.path.join(build_output_dir, "SpaceWarp.dll"), os.path.join(output_dir, "SpaceWarp.dll"))
    
    print("=> Finalizing build...")
    
    os.makedirs(os.path.join(BUILD_DIR, build_type, "SpaceWarp", "assets", "bundles"))
    os.makedirs(os.path.join(BUILD_DIR, build_type, "SpaceWarp", "Mods"))
    
    for bundle in os.listdir(BUNDLES_DIR):
        if bundle.endswith(".bundle"):
            shutil.copyfile(os.path.join(BUNDLES_DIR, bundle), os.path.join(BUILD_DIR, build_type, "SpaceWarp", "assets", "bundles", bundle))


def create_zip_name(prefix):
    git_output = subprocess.run(["git", "rev-parse", "HEAD"], capture_output=True)
    commit_full = str(git_output.stdout, "utf-8")
    commit = commit_full[:7]

    return os.path.join(BUILD_DIR, f"{prefix}-{commit}.zip")

def mkzip(in_dir, out_zip):
    with zipfile.ZipFile(out_zip, "w", zipfile.ZIP_DEFLATED) as handle:
        for root, dirs, files in os.walk(in_dir):
            for file in files:
                handle.write(os.path.join(root, file), os.path.relpath(os.path.join(root, file), in_dir))
        
        handle.close()

def compress(doorstop = False, release = False):
    target_name = "Doorstop" if doorstop else "BepInEx"
    release_target = "Release" if release else "Debug"
    
    mkzip(os.path.join(BUILD_DIR, target_name), create_zip_name(f"SpaceWarp-{target_name}-{release_target}"))
    
def main():
    args = parser.parse_args()

    doorstop = args.target == "doorstop"
    target_name = "Doorstop" if args.target == "doorstop" else "BepInEx"
    total_steps = 5 if args.all else 3

    print(f"====> [1/{total_steps}] Cleaning...")

    clean()
    
    if args.all:
        print(f"====> [2/{total_steps}] Building for BepInEx...")
        
        build(args.release, False)
        
        print(f"====> [3/{total_steps}] Building for Doorstop...")
        
        build(args.release, True)
        
        print(f"====> [4/{total_steps}] Compressing BepInEx build...")
        
        compress(False, args.release)
        
        print(f"====> [5/{total_steps}] Compressing Doorstop build...")
        
        compress(True, args.release)
    else:
        print(f"====> [2/{total_steps}] Building for {target_name}...")
        
        build(args.release, doorstop)
        
        print(f"====> [3/{total_steps}] Compressing {target_name} build...")
        
        compress(doorstop, args.release)

main()
