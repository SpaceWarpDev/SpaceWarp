using AK.Wwise;
using SpaceWarp.API.Sound;
using SpaceWarp.Backend.Sound;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace SpaceWarp.Patching.LoadingActions;

internal static class FunctionalLoadingActions
{
    internal static List<(string name, UnityObject asset)> AssetBundleLoadingAction(string internalPath, string filename)
    {
        var assetBundle = AssetBundle.LoadFromFile(filename);
        if (assetBundle == null)
        {
            throw new Exception(
                $"Failed to load AssetBundle {internalPath}");
        }

        internalPath = internalPath.Replace(".bundle", "");
        var names = assetBundle.GetAllAssetNames();
        List<(string name, UnityObject asset)> assets = new();
        foreach (var name in names)
        {
            var assetName = name;

            if (assetName.ToLower().StartsWith("assets/"))
            {
                assetName = assetName["assets/".Length..];
            }

            if (assetName.ToLower().StartsWith(internalPath + "/"))
            {
                assetName = assetName[(internalPath.Length + 1)..];
            }

            var path = internalPath + "/" + assetName;
            path = path.ToLower();
            var asset = assetBundle.LoadAsset(name);
            assets.Add((path, asset));
        }

        return assets;
    }

    internal static List<(string name, UnityObject asset)> AssetSoundbankLoadingAction(string internalPath, string filename)
    {
        var fileData = File.ReadAllBytes(filename);
        //Since theres no UnityObject that relates to soundbanks it passes null, saving only the internalpath
        List<(string name, UnityObject asset)> assets = new() { ($"soundbanks/{internalPath}", null) };

        //Banks are identified under Bank.soundbanks with their internal path
        if (SoundAPI.LoadBank($"soundbanks/{internalPath}", fileData, out var bank))
        {
            return assets;
        }
        else
        throw new Exception(
            $"Failed to load soundbank {internalPath}");
    }

    internal static List<(string name, UnityObject asset)> ImageLoadingAction(string internalPath, string filename)
    {
        var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Point
        };
        var fileData = File.ReadAllBytes(filename);
        tex.LoadImage(fileData); // Will automatically resize
        List<(string name, UnityObject asset)> assets = new() { ($"images/{internalPath}", tex) };
        return assets;
    }
}