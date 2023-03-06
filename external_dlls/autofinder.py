import os
import re
import shutil
import platform

def get_steam_dir():
    if platform.system() == "Windows":
        return os.path.join(os.getenv("ProgramFiles(x86)"), "Steam")
    elif platform.system() == "Darwin":
        return os.path.expanduser("~/Library/Application Support/Steam")
    else:
        return os.path.expanduser("~/.steam/steam")

def find_game_dir(game_name):
    steam_dir = get_steam_dir()
    library_folders_path = os.path.join(steam_dir, "steamapps", "libraryfolders.vdf")
    app_manifest_pattern = re.compile(r"appmanifest_\d+\.acf")
    for library_folder in [steam_dir] + get_library_folders(library_folders_path):
        manifest_dir = os.path.join(library_folder, "steamapps")
        for manifest_file in os.listdir(manifest_dir):
            if app_manifest_pattern.match(manifest_file):
                with open(os.path.join(manifest_dir, manifest_file), "r") as f:
                    manifest_data = f.read()
                    if game_name in manifest_data:
                        return os.path.join(library_folder, "steamapps", "common", game_name)
    epic_dir = os.path.join(os.getenv("ProgramData"), "Epic", "EpicGamesLauncher", "Data", "Manifests")
    app_info_pattern = re.compile(r"([^/]+)\.item")
    for app_info_file in os.listdir(epic_dir):
        with open(os.path.join(epic_dir, app_info_file), "r") as f:
            app_info_data = f.read()
            match = app_info_pattern.search(app_info_data)
            if match is not None and match.group(1) == game_name:
                return os.path.join(os.path.dirname(epic_dir), "Installation", match.group(0), "KSP2")
    private_division_dir = os.path.join(os.getenv("LOCALAPPDATA"), "PrivateDivision", game_name)
    if os.path.exists(private_division_dir):
        return private_division_dir
    return None

def get_library_folders(vdf_path):
    library_folders = []
    with open(vdf_path, "r") as f:
        vdf_data = f.read()
        for match in re.finditer(r'"[A-Za-z]:\\[^"]*"', vdf_data):
            library_folders.append(match.group(0)[1:-1])
    return library_folders

game_name = "Kerbal Space Program 2"
game_dir = find_game_dir(game_name)
if game_dir is not None:
    print(f"Found {game_name} installed at {game_dir}")
    managed_dir = os.path.join(game_dir, "KSP2_x64_Data", "Managed")
    if os.path.exists(managed_dir):
        for file_name in os.listdir(managed_dir):
            file_path = os.path.join(managed_dir, file_name)
            if os.path.isfile(file_path):
                shutil.copy(file_path, os.path.join(os.path.dirname(os.path.abspath(__file__)), file_name))
        print("Files copied successfully!")
    else:
        print(f"Could not find {game_name} Managed directory")
else:
    print(f"{game_name} not found")
