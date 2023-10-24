import argparse
import os
import platform
import shutil
import subprocess
import zipfile

TEMPLATE_DIR = os.path.abspath("SpaceWarpBuildTemplate")
SPACEWARP_DIR = os.path.abspath("SpaceWarp")
SPACEWARP_CORE_DIR = os.path.abspath("SpaceWarp.Core")
PATCHER_DIR = os.path.abspath("SpaceWarpPatcher")
PATCHER_LIB_DIR = os.path.abspath("SpaceWarpPatcherLibraries")
BUILD_DIR = os.path.abspath("build")
THIRD_PARTY = os.path.abspath("ThirdParty")

MODULES = ["Game", "Sound", "UI", "VersionChecking", "Messaging"]

parser = argparse.ArgumentParser()
parser.add_argument("-r", "--release", help="Build a release version", action="store_true")


def clean():
    # Need to add modules and such to this
    if os.path.exists(BUILD_DIR):
        shutil.rmtree(BUILD_DIR)

    if os.path.exists(os.path.join(SPACEWARP_DIR, "bin")):
        shutil.rmtree(os.path.join(SPACEWARP_DIR, "bin"))

    if os.path.exists(os.path.join(SPACEWARP_DIR, "obj")):
        shutil.rmtree(os.path.join(SPACEWARP_DIR, "obj"))

    if os.path.exists(os.path.join(PATCHER_DIR, "bin")):
        shutil.rmtree(os.path.join(PATCHER_DIR, "bin"))

    if os.path.exists(os.path.join(PATCHER_DIR, "obj")):
        shutil.rmtree(os.path.join(PATCHER_DIR, "obj"))


def build(release=False):
    dotnet_args = ["dotnet", "build", os.path.join(SPACEWARP_DIR, "SpaceWarp.csproj"), "-c",
                   "Release" if release else "Debug"]
    dotnet_core_args = ["dotnet", "build", os.path.join(SPACEWARP_CORE_DIR, "SpaceWarp.Core.csproj"), "-c",
                        "Release" if release else "Debug"]
    build_output_dir = os.path.join(SPACEWARP_DIR, "bin", "Release" if release else "Debug")
    build_output_dir_core = os.path.join(SPACEWARP_CORE_DIR, "bin", "Release" if release else "Debug")
    output_dir = os.path.join(BUILD_DIR, "SpaceWarp", "BepInEx", "plugins", "SpaceWarp")
    nuget_temp_dir = os.path.join(BUILD_DIR, "nuget_temp")
    # copy over the internals of the template
    print("=> Creating build directory...")
    os.makedirs(os.path.join(BUILD_DIR, "SpaceWarp"), True)
    os.makedirs(nuget_temp_dir, True)
    print("=> Copying BepInEx")
    os.makedirs(os.path.join(BUILD_DIR, "SpaceWarp", "BepInEx", "plugins"), True)
    os.makedirs(os.path.join(BUILD_DIR, "SpaceWarp", "BepInEx", "patchers", "SpaceWarp"), True)
    shutil.copytree(os.path.join(THIRD_PARTY, "BepInEx 5.4.21"), os.path.join(BUILD_DIR, "SpaceWarp"),
                    dirs_exist_ok=True)
    print("=> Copying BepInEx.ConfigurationManager")
    os.makedirs(os.path.join(BUILD_DIR, "SpaceWarp", "BepInEx", "plugins", "ConfigurationManager"), True)
    shutil.copytree(os.path.join(THIRD_PARTY, "ConfigurationManager"),
                    os.path.join(BUILD_DIR, "SpaceWarp", "BepInEx", "plugins", "ConfigurationManager"),
                    dirs_exist_ok=True)

    print("=> Copying output template")
    shutil.copytree(TEMPLATE_DIR, output_dir)

    print(f"=> Executing: {' '.join(dotnet_args)}")

    output = subprocess.run(args=dotnet_args, capture_output=True)
    print("    |=>| STDOUT")

    for line in str(output.stdout, "utf-8").splitlines():
        print(f"        {line}")

    print("    |=>| STDERR")

    for line in str(output.stderr, "utf-8").splitlines():
        print(f"        {line}")

    print(f"=> Executing: {' '.join(dotnet_core_args)}")

    output = subprocess.run(args=dotnet_core_args, capture_output=True)
    print("    |=>| STDOUT")

    for line in str(output.stdout, "utf-8").splitlines():
        print(f"        {line}")

    print("    |=>| STDERR")

    for line in str(output.stderr, "utf-8").splitlines():
        print(f"        {line}")

    print("=> Compiling Modules")
    os.makedirs(os.path.join(BUILD_DIR, "SpaceWarp", "BepInEx", "plugins", "SpaceWarp", "modules"), True)
    for module in MODULES:
        path = os.path.abspath("SpaceWarp." + module)
        csproj = os.path.join(path, "SpaceWarp." + module + ".csproj")
        module_args = ["dotnet", "build", csproj, "-c", "Release" if release else "Debug"]
        module_build_output_dir = os.path.join(path, "bin", "Release" if release else "Debug")
        module_output_dir = os.path.join(BUILD_DIR, "SpaceWarp", "BepInEx", "plugins", "SpaceWarp", "modules")

        print(f"=> Executing: {' '.join(module_args)}")

        output = subprocess.run(args=module_args, capture_output=True)
        print("    |=>| STDOUT")

        for line in str(output.stdout, "utf-8").splitlines():
            print(f"        {line}")

        print("    |=>| STDERR")

        for line in str(output.stderr, "utf-8").splitlines():
            print(f"        {line}")

        print("=> Copying module")

        def shutil_copy_module(file):
            shutil.copyfile(os.path.join(module_build_output_dir, file), os.path.join(module_output_dir, file))

        def shutil_copy_nuget_module(file):
            shutil.copyfile(os.path.join(module_build_output_dir, file), os.path.join(nuget_temp_dir, file))

        if not release and os.path.exists(os.path.join(module_build_output_dir, "SpaceWarp." + module + ".pdb")):
            shutil_copy_module("SpaceWarp." + module + ".pdb")

        shutil_copy_module("SpaceWarp." + module + ".dll")
        shutil_copy_nuget_module("SpaceWarp." + module + ".dll")
        shutil_copy_nuget_module("SpaceWarp." + module + ".xml")

    # patcher libraries
    patcher_library_dir = os.path.join(BUILD_DIR, "SpaceWarp", "BepInEx", "patchers", "SpaceWarp", "lib")
    print(f"=> Copying Patcher Libraries")
    shutil.copytree(PATCHER_LIB_DIR, patcher_library_dir)

    # patcher build
    patcher_dotnet_args = ["dotnet", "build", os.path.join(PATCHER_DIR, "SpaceWarpPatcher.csproj"), "-c",
                           "Release" if release else "Debug"]
    patcher_build_output_dir = os.path.join(PATCHER_DIR, "bin", "Release" if release else "Debug")
    patcher_output_dir = os.path.join(BUILD_DIR, "SpaceWarp", "BepInEx", "patchers", "SpaceWarp")

    print(f"=> Executing: {' '.join(patcher_dotnet_args)}")

    patcher_output = subprocess.run(args=patcher_dotnet_args, capture_output=True)
    print("    |=>| STDOUT")

    for line in str(patcher_output.stdout, "utf-8").splitlines():
        print(f"        {line}")

    print("    |=>| STDERR")

    for line in str(patcher_output.stderr, "utf-8").splitlines():
        print(f"        {line}")

    print("=> Copying build outputs...")

    def shutil_copy(file):
        shutil.copyfile(os.path.join(build_output_dir, file), os.path.join(output_dir, file))

    def shutil_copy_core(file):
        shutil.copyfile(os.path.join(build_output_dir_core, file), os.path.join(output_dir, file))

    def shutil_copy_patcher(file):
        shutil.copyfile(os.path.join(patcher_build_output_dir, file), os.path.join(patcher_output_dir, file))

    def shutil_nuget_copy(file):
        shutil.copyfile(os.path.join(build_output_dir, file), os.path.join(nuget_temp_dir, file))

    def shutil_nuget_copy_core(file):
        shutil.copyfile(os.path.join(build_output_dir_core, file), os.path.join(nuget_temp_dir, file))

    def shutil_nuget_copy_patcher(file):
        shutil.copyfile(os.path.join(patcher_build_output_dir, file), os.path.join(nuget_temp_dir, file))

    if not release and os.path.exists(os.path.join(build_output_dir_core, "SpaceWarp.Core.pdb")):
        shutil_copy_core("SpaceWarp.Core.pdb")
    if not release and os.path.exists(os.path.join(build_output_dir, "SpaceWarp.pdb")):
        shutil_copy("SpaceWarp.pdb")
    if not release and os.path.exists(os.path.join(patcher_build_output_dir, "SpaceWarpPatcher.pdb")):
        shutil_copy_patcher("SpaceWarpPatcher.pdb")

    shutil_copy("SpaceWarp.dll")
    shutil_nuget_copy("SpaceWarp.dll")
    shutil_nuget_copy("SpaceWarp.xml")

    shutil_copy_core("SpaceWarp.Core.dll")
    shutil_nuget_copy_core("SpaceWarp.Core.dll")
    shutil_nuget_copy_core("SpaceWarp.Core.xml")

    shutil_copy_patcher("SpaceWarpPatcher.dll")
    shutil_nuget_copy_patcher("SpaceWarpPatcher.dll")
    shutil_nuget_copy_patcher("SpaceWarpPatcher.xml")


def get_console_encoding():
    command = 'powershell -command "[Console]::OutputEncoding.WebName"'
    result = subprocess.run(command, shell=True, capture_output=True, text=True)
    if result.returncode == 0:
        encoding_name = result.stdout.strip()
        return encoding_name
    else:
        raise RuntimeError(f'Failed to get console encoding: {result.stderr}')


def nuget_pack():
    nuget_args = [
        "nuget",
        "pack",
        os.path.abspath("SpaceWarp.nuspec"),
        "-OutputDirectory",
        BUILD_DIR
    ]
    command_str = subprocess.list2cmdline(nuget_args)
    print(f"=> Executing: {command_str}")

    encoding = get_console_encoding() if platform.system() == 'Windows' else 'utf-8'

    output = subprocess.run(args=command_str, capture_output=True)
    print("    |=>| STDOUT")

    for line in str(output.stdout, encoding).splitlines():
        print(f"        {line}")

    print("    |=>| STDERR")

    for line in str(output.stderr, encoding).splitlines():
        print(f"        {line}")

    shutil.rmtree(os.path.join(BUILD_DIR, "nuget_temp"))


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


def compress(release=False):
    release_target = "Release" if release else "Debug"
    mkzip(os.path.join(BUILD_DIR, "SpaceWarp"), create_zip_name(f"SpaceWarp-{release_target}"))


def main():
    args = parser.parse_args()
    total_steps = 4
    print(f"====> [1/{total_steps}] Cleaning...")

    clean()

    print(f"====> [2/{total_steps}] Building...")

    build(args.release)

    print(f"====> [3/{total_steps}] Packing NuGet...")

    nuget_pack()

    print(f"====> [4/{total_steps}] Compressing...")

    compress(args.release)


main()
