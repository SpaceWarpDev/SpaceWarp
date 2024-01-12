using UnityEditor;
using System.IO;

public static class CreateAssetBundles
{
    private const string AssetBundleDirectory = "Assets/AssetBundles";

    // Relative path from the Unity project directory to the target directory
    private const string TargetDirectory = "../../../plugin_template/BepInEx/plugins/SpaceWarp/assets/bundles";

    [MenuItem("Assets/Build AssetBundles")]
    private static void BuildAllAssetBundles()
    {
        // Ensure the AssetBundle directory exists
        if (!Directory.Exists(AssetBundleDirectory))
        {
            Directory.CreateDirectory(AssetBundleDirectory);
        }

        // Build the asset bundles
        BuildPipeline.BuildAssetBundles(
            AssetBundleDirectory,
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows
        );

        if (!Directory.Exists(TargetDirectory))
        {
            Directory.CreateDirectory(TargetDirectory);
        }

        // Copy the newly built bundles to the target directory
        var newBundles = Directory.GetFiles(AssetBundleDirectory, "*.bundle");
        foreach (var bundle in newBundles)
        {
            var destFile = Path.Combine(TargetDirectory, Path.GetFileName(bundle));
            File.Copy(bundle, destFile, overwrite: true);
        }
    }
}