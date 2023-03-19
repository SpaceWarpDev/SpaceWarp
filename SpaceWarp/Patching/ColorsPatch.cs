using HarmonyLib;
using SpaceWarp.API.Assets;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using KSP.Modules;
using KSP.Game;
using System;
using BepInEx.Logging;

namespace SpaceWarp.Patching;

/// <summary>
/// This patch is meant to give modders a way to use the new colors system on KSP2.
/// 
/// How to: On unity, set the shader of your mesh to Standard. On your Plugin/{ModID} folder, add at assets/Images your textures,
/// Textures should be named {PartName}_{MapType}, check the suffixes for more information.
/// 
/// Parts with multiple sizes (such as XS, S, M, L, XL) are supported, just be sure that your part config is named accordingly.
/// Ie:partXS, partS, partM, partL, partXL.
/// this patch will import the textures to all part variants.
/// 
/// if you want your parts to be loaded before, just add them to partsToLoad.
/// 
/// Patch created by LuxStice.
/// </summary>
/// 
[HarmonyPatch]
class ColorsPatch
{
    private static bool LoadOnInit = true; ///TODO: Implement false behaviour
    public static Dictionary<string, string[]> DeclaredParts { get; private set; } = new();
    private static string[] allParts;

    private static Dictionary<string, Texture[]> partHash;
    private static int[] propertyIds;
    private static string[] textureSuffixes =
    {
        "d.png",
        "m.png",
        "n.png",
        "ao.png",
        "e.png",
        "pm.png"
    };
    private static string[] textureNames =
    {
        "diffuse",
        "mettalic",
        "normal",
        "ambient occlusion",
        "emission",
        "paint map"
    };

    private const int DIFFUSE = 0;
    private const int METTALLIC = 1;
    private const int BUMP = 2;
    private const int OCCLUSION = 3;
    private const int EMISSION = 4;
    private const int PAINT_MAP = 5;

    private static Shader ksp2Opaque;
    private static Shader ksp2Transparent;
    private static Shader unityStandard;
    private const string KSP2_OPAQUE_PATH = "KSP2/Scenery/Standard (Opaque)",
        KSP2_TRANSPARENT_PATH = "KSP2/Scenery/Standard (Transparent)",
        UNITY_STANDARD = "Standard";

    [HarmonyPrepare]
    static bool Init(MethodBase original)
    {
        if (original is null)
            return true;
        partHash = new Dictionary<string, Texture[]>();
        propertyIds = new int[]
        {
            Shader.PropertyToID("_MainTex"),
            Shader.PropertyToID("_MetallicGlossMap"),
            Shader.PropertyToID("_BumpMap"),
            Shader.PropertyToID("_OcclusionMap"),
            Shader.PropertyToID("_EmissionMap"),
            Shader.PropertyToID("_PaintMaskGlossMap")
        };

        ksp2Opaque = Shader.Find(KSP2_OPAQUE_PATH);
        ksp2Transparent = Shader.Find(KSP2_TRANSPARENT_PATH);
        unityStandard = Shader.Find(UNITY_STANDARD);

        Logger = BepInEx.Logging.Logger.CreateLogSource(displayName);

        return true;///TODO: add config to enable/disable this patch, if disabled return false.
    }

    /// <summary>
    /// Adds <paramref name="partNameList"/> to internal parts list under <paramref name="modGUID"/>
    /// allowing them to have the patch applied.
    /// </summary>
    /// <param name="modGUID">guid of the mod that owns the parts.</param>
    /// <param name="partNameList">Collection of partNames. Names that end in XS, S, M, L or XL will be counted as the same part,</param>
    public static void DeclareParts(string modGUID, params string[] partNameList) => DeclareParts(modGUID, partNameList.ToList());
    /// <summary>
    /// Adds <paramref name="partNameList"/> to internal parts list under <paramref name="modGUID"/>
    /// allowing them to have the patch applied.
    /// </summary>
    /// <param name="modGUID">guid of the mod that owns the parts.</param>
    /// <param name="partNameList">Collection of partNames. Names that end in XS, S, M, L or XL will be counted as the same part.</param>
    public static void DeclareParts(string modGUID, IEnumerable<string> partNameList)
    {
        if(DeclaredParts.ContainsKey(modGUID))
        {
            LogWarning($"{modGUID} tried to declare their parts twice. Ignoring second call.");
            return;
        }
        if(partNameList.Count() == 0)
        {
            LogWarning($"{modGUID} tried to declare no parts. Ignoring this call.");
            return;
        }
        DeclaredParts.Add(modGUID, partNameList.ToArray());
    }

    private static void LoadDeclaredParts()
    {
        List<string> allPartsTemp = new();

        if (DeclaredParts.Count == 0)
        {
            LogWarning("No parts were declared before load.");
            return;
        }
        if (LoadOnInit)
        {
            foreach (string modGUID in DeclaredParts.Keys)
            {
                LoadTextures(modGUID);

                foreach (string partName in DeclaredParts[modGUID])
                {
                    allPartsTemp.Add(TrimPartName(partName));
                }
            }
        }

        allParts = allPartsTemp.ToArray();
    }
    private static bool TryAddUnique(string partName)
    {
        if (partHash.ContainsKey(partName))
        {
            return false;
        }

        partHash.Add(partName, new Texture[6]);
        return true;
    }


    private static void LoadTextures(string modGUID)
    {
        LogMessage($">Loading parts from {modGUID}<");

        foreach (string partName in DeclaredParts[modGUID])
        {
            LogMessage($">Loading {partName}");
            if (!TryAddUnique(partName))
            {
                LogWarning($"{partName} already exists in hash map. Probably it already exists in another mod. Ignoring this part."); //this shows once per call... too much
                continue;
            }

            string trimmedPartName = TrimPartName(partName);
            string pathWithoutSuffix = $"{modGUID.ToLower()}/images/{trimmedPartName.ToLower()}/{trimmedPartName.ToLower()}";


            int count = 0; //already has diffuse
            if (AssetManager.TryGetAsset($"{pathWithoutSuffix}_{textureSuffixes[DIFFUSE]}", out Texture2D dTex))
            {
                partHash[trimmedPartName][DIFFUSE] = dTex;
                count++;
                LogMessage($"\t({count}/6) Loaded {textureNames[DIFFUSE]} texture");
            }
            else
            {
                LogWarning($"{partName} doesn't have a diffuse texture. Skipping this part.");
                return;
            }

            for (int i = 1; i < propertyIds.Length; i++)
            {
                if (AssetManager.TryGetAsset($"{pathWithoutSuffix}_{textureSuffixes[i]}", out Texture2D Tex))
                {
                    count++;

                    partHash[trimmedPartName][i] = Tex;
                    LogMessage($"\t({count}/6) Loaded {textureNames[i]} texture");
                }
            }
        }
    }
    private static void SetTexturesToMaterial(string partName, ref Material material)
    {
        string trimmedPartName = TrimPartName(partName);
        for (int i = 0; i < propertyIds.Length; i++)
        {
            Texture texture = partHash[trimmedPartName][i];
            if (texture is not null)
                material.SetTexture(propertyIds[i], texture);
        }
    }

    private static string TrimPartName(string partName)
    {
        if (partName.Length >= 3)
        {
            if (partName.EndsWith("XS")
                || partName.EndsWith("XL"))
                return partName.Remove(partName.Length - 2, 2);

            else if (partName.EndsWith("S") || partName.EndsWith("M")
                || partName.EndsWith("L"))
                return partName.Remove(partName.Length - 1);
        }
        return partName;
    }

    [HarmonyPatch(typeof(GameManager),
        nameof(GameManager.OnLoadingFinished))]
    public static void Prefix()
    {
        LoadDeclaredParts(); ///TODO: Move this to a more apropriate call, like the one loading parts or something like that.
    }
    
    [HarmonyPatch(typeof(Module_Color),
        nameof(Module_Color.OnInitialize))]
    public static void Postfix(Module_Color __instance)
    {
        if(DeclaredParts.Count == 0){ return; }

        string partName;
        if (__instance.OABPart is not null)
            partName = __instance.OABPart.PartName;
        else
            partName = __instance.part.Name;
        string trimmedPartName = TrimPartName(partName);

        if (!allParts.Contains(trimmedPartName))
        {
            //SpaceWarpManager.Logger.LogError($"{partName} is not declared and onlyDeclareParts is enabled. Skipping."); //This will generate a LOT of logs
            return;
        }

        foreach (MeshRenderer renderer in __instance.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (renderer.material.shader.name != unityStandard.name)
                continue;

            Material mat = new Material(ksp2Opaque);
            SetTexturesToMaterial(trimmedPartName, ref mat);

            renderer.material = mat;

            if (renderer.material.shader.name != ksp2Opaque.name)
                renderer.SetMaterial(mat); //Sometimes the material Set doesn't work, this seems to be more reliable.

        }
        __instance.SomeColorUpdated();
    }



    private const string displayName = "TTR"; //Taste the Rainbow - name by munix
    public static ManualLogSource Logger;
    private static void LogMessage(object data) => Logger.LogMessage($"{data}");
    private static void LogWarning(object data) => Logger.LogWarning($"{data}");
    private static void LogError(object data) => Logger.LogError($"{data}");
}
