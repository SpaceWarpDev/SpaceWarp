import argparse
import os
import shutil
import subprocess
import zipfile


TEMPLATE_DIR = os.path.abspath("SpaceWarpBuildTemplate")
SPACEWARP_DIR = os.path.abspath("SpaceWarp")
BUILD_DIR = os.path.abspath("build")


parser = argparse.ArgumentParser()
parser.add_argument("-r", "--release", help="Build a release version", action="store_true")

def clean():
    if os.path.exists(BUILD_DIR):
        shutil.rmtree(BUILD_DIR)
    
    if os.path.exists(os.path.join(SPACEWARP_DIR, "bin")):
        shutil.rmtree(os.path.join(SPACEWARP_DIR, "bin"))
    
    if os.path.exists(os.path.join(SPACEWARP_DIR, "obj")):
        shutil.rmtree(os.path.join(SPACEWARP_DIR, "obj"))

def build(release = False):
    dotnet_args = ["dotnet", "build", os.path.join(SPACEWARP_DIR,"SpaceWarp.csproj"),  "-c", "Release" if release else "Debug"]
    build_output_dir = os.path.join(SPACEWARP_DIR, "bin", "Release" if release else "Debug")
    output_dir = os.path.join(BUILD_DIR, "SpaceWarp", "BepInEx", "plugins", "SpaceWarp")
    # copy over the internals of the template
    print("=> Creating build directory...")
    os.makedirs(os.path.join(BUILD_DIR, "SpaceWarp", "BepInEx", "plugins"))

    print("=> Copying output template")
    shutil.copytree(TEMPLATE_DIR,output_dir)
    
    print(f"=> Executing: {' '.join(dotnet_args)}")
    
    output = subprocess.run(args=dotnet_args, capture_output=True)
    print("    |=>| STDOUT")
    
    for line in str(output.stdout, "utf-8").splitlines():
        print(f"        {line}")
        
    print("    |=>| STDERR")
    
    for line in str(output.stderr, "utf-8").splitlines():
        print(f"        {line}")
    
    print("=> Copying build outputs...")
    def shutil_copy(file):
        shutil.copyfile(os.path.join(build_output_dir,file),os.path.join(output_dir,file))
    if not release and os.path.exists(os.path.join(build_output_dir, "SpaceWarp.pdb")):
        shutil_copy("SpaceWarp.pdb")
    shutil_copy("SpaceWarp.dll")

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

def compress(release = False):
    release_target = "Release" if release else "Debug"
    mkzip(os.path.join(BUILD_DIR,"SpaceWarp"),create_zip_name(f"SpaceWarp-{release_target}"))

def main():
    args = parser.parse_args()
    total_steps = 3
    print(f"====> [1/{total_steps}] Cleaning...")

    clean()

    print(f"====> [2/{total_steps}] Building...")
    
    build(args.release)
        
    print(f"====> [3/{total_steps}] Compressing...")

    compress(args.release)

main()