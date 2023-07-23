using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using KSP.Game;
using KSP.Modules;
using KSP.OAB;
using SpaceWarp.API.Assets;
using UnityEngine;

namespace SpaceWarp.Patching;

/// <summary>
///     This patch is meant to give modders a way to use the new colors system on KSP2.
///     The patch will replace any renderer that has a "Parts Replace" or a "KSP2/Parts/Paintable" shader on it.
///     It will copy all its values onto the new material, including the material name.
///     Note: "Parts Replace" is obsolete and might be deleted on a later version.
///     Patch created by LuxStice.
/// </summary>
[HarmonyPatch]
internal class ColorsPatch
{
    private const string KSP2_OPAQUE_PATH = "KSP2/Scenery/Standard (Opaque)",
        KSP2_TRANSPARENT_PATH = "KSP2/Scenery/Standard (Transparent)",
        UNITY_STANDARD = "Standard";

    [HarmonyPatch(typeof(ObjectAssemblyPartTracker), nameof(ObjectAssemblyPartTracker.OnPartPrefabLoaded))]
    public static void Prefix(IObjectAssemblyAvailablePart obj, ref GameObject prefab)
    {
        foreach(var renderer in prefab.GetComponentsInChildren<Renderer>())
        {
            string shaderName = renderer.material.shader.name;
            if (shaderName == "Parts Replace" || shaderName == "KSP2/Parts/Paintable")
            {
                var mat = new Material(Shader.Find(KSP2_OPAQUE_PATH));
                mat.name = renderer.material.name;
                mat.CopyPropertiesFromMaterial(renderer.material);
                renderer.material = mat;
            }
        }
    }

    //Everything below this point will be removed in the next patch
    private const int DIFFUSE = 0;
    private const int METTALLIC = 1;
    private const int BUMP = 2;
    private const int OCCLUSION = 3;
    private const int EMISSION = 4;
    private const int PAINT_MAP = 5;


    private const string displayName = "TTR"; //Taste the Rainbow - name by munix
    private const bool LoadOnInit = true;
    private static string[] _allParts;

    private static Dictionary<string, Texture[]> _partHash;
    private static int[] _propertyIds;

    private static readonly string[] TextureSuffixes =
    {
        "d.png",
        "m.png",
        "n.png",
        "ao.png",
        "e.png",
        "pm.png"
    };

    private static readonly string[] TextureNames =
    {
        "diffuse",
        "metallic",
        "normal",
        "ambient occlusion",
        "emission",
        "paint map"
    };

    private static Shader _ksp2Opaque;
    private static Shader _ksp2Transparent;
    private static Shader _unityStandard;
    internal static ManualLogSource Logger;

    ///TODO: Implement false behaviour
    public static Dictionary<string, string[]> DeclaredParts { get; } = new();

    [HarmonyPrepare]
    private static bool Init(MethodBase original)
    {
        if (original is null)
        {
            return true;
        }

        _partHash = new Dictionary<string, Texture[]>();
        _propertyIds = new[]
        {
            Shader.PropertyToID("_MainTex"),
            Shader.PropertyToID("_MetallicGlossMap"),
            Shader.PropertyToID("_BumpMap"),
            Shader.PropertyToID("_OcclusionMap"),
            Shader.PropertyToID("_EmissionMap"),
            Shader.PropertyToID("_PaintMaskGlossMap")
        };

        _ksp2Opaque = Shader.Find(KSP2_OPAQUE_PATH);
        _ksp2Transparent = Shader.Find(KSP2_TRANSPARENT_PATH);
        _unityStandard = Shader.Find(UNITY_STANDARD);

        Logger = BepInEx.Logging.Logger.CreateLogSource(displayName);

        return true; // TODO: add config to enable/disable this patch, if disabled return false.
    }

    /// <summary>
    ///     Adds <paramref name="partNameList" /> to internal parts list under <paramref name="modGUID" />
    ///     allowing them to have the patch applied.
    /// </summary>
    /// <param name="modGUID">guid of the mod that owns the parts.</param>
    /// <param name="partNameList">
    ///     Collection of partNames. Names that end in XS, S, M, L or XL will be counted as the same
    ///     part,
    /// </param>
    internal static void DeclareParts(string modGUID, params string[] partNameList)
    {
        DeclareParts(modGUID, partNameList.ToList());
    }

    /// <summary>
    ///     Adds <paramref name="partNameList" /> to internal parts list under <paramref name="modGUID" />
    ///     allowing them to have the patch applied.
    /// </summary>
    /// <param name="modGUID">guid of the mod that owns the parts.</param>
    /// <param name="partNameList">
    ///     Collection of partNames. Names that end in XS, S, M, L or XL will be counted as the same
    ///     part.
    /// </param>
    internal static void DeclareParts(string modGUID, IEnumerable<string> partNameList)
    {
        if (DeclaredParts.ContainsKey(modGUID))
        {
            LogWarning($"{modGUID} tried to declare their parts twice. Ignoring second call.");
            return;
        }

        var nameList = partNameList as string[] ?? partNameList.ToArray();
        if (!nameList.Any())
        {
            LogWarning($"{modGUID} tried to declare no parts. Ignoring this call.");
            return;
        }

        DeclaredParts.Add(modGUID, nameList.ToArray());
    }

    internal static Texture[] GetTextures(string partName)
    {
        if (_partHash.ContainsKey(partName))
            return _partHash[partName];
        else
        {
            LogError($"Requested textures from {partName} but part doesn't exist on declared parts!");
            return null;
        }
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
            foreach (var modGUID in DeclaredParts.Keys)
            {
                LoadTextures(modGUID);

                allPartsTemp.AddRange(DeclaredParts[modGUID].Select(partName => TrimPartName(partName)));
            }
        }

        _allParts = allPartsTemp.ToArray();
    }

    private static bool TryAddUnique(string partName)
    {
        if (_partHash.ContainsKey(partName))
        {
            return false;
        }

        _partHash.Add(partName, new Texture[6]);
        return true;
    }


    private static void LoadTextures(string modGuid)
    {
        LogMessage($">Loading parts from {modGuid}");

        foreach (var partName in DeclaredParts[modGuid])
        {
            LogMessage($"\t>Loading {partName}");
            if (!TryAddUnique(partName))
            {
                LogWarning(
                    $"{partName} already exists in hash map. Probably it already exists in another mod. Ignoring this part."); //this shows once per call... too much
                continue;
            }

            var trimmedPartName = TrimPartName(partName);
            var pathWithoutSuffix =
                $"{modGuid.ToLower()}/images/{trimmedPartName.ToLower()}/{trimmedPartName.ToLower()}";


            var count = 0; //already has diffuse
            if (AssetManager.TryGetAsset($"{pathWithoutSuffix}_{TextureSuffixes[DIFFUSE]}", out Texture2D dTex))
            {
                _partHash[trimmedPartName][DIFFUSE] = dTex;
                count++;
                LogMessage($"\t\t>({count}/6) Loaded {TextureNames[DIFFUSE]} texture");
            }
            else
            {
                LogWarning($"{partName} doesn't have a diffuse texture. Skipping this part.");
                return;
            }

            for (int i = 1; i < _propertyIds.Length; i++)
            {
                if (AssetManager.TryGetAsset($"{pathWithoutSuffix}_{TextureSuffixes[i]}", out Texture2D Tex))
                {
                    count++;

                    if (i == ColorsPatch.BUMP) //Converting texture to Bump texture
                    {
                        Texture2D normalTexture = new Texture2D(Tex.width, Tex.height, TextureFormat.RGBA32, false, true);
                        Graphics.CopyTexture(Tex, normalTexture);
                        Tex = normalTexture;
                    }
                    _partHash[trimmedPartName][i] = Tex;
                    LogMessage($"\t\t>({count}/6) Loaded {TextureNames[i]} texture");
                }
            }

            if (count == 6)
                LogMessage($"\t\tWoW Much Textures!");
        }
    }

    private static void SetTexturesToMaterial(string partName, ref Material material)
    {
        var trimmedPartName = TrimPartName(partName);
        material.SetFloat("_MetallicGlossMap", 1f);
        material.SetFloat("_Metallic", 1f);
        material.SetFloat("_PaintGlossMapScale", 1f);
        for (var i = 0; i < _propertyIds.Length; i++)
        {
            var texture = _partHash[trimmedPartName][i];
            if (texture is not null)
            {
                material.SetTexture(_propertyIds[i], texture);
            }
        }
    }

    private static string TrimPartName(string partName)
    {
        if (partName.Length < 3)
        {
            return partName;
        }

        if (partName.EndsWith("XS")
            || partName.EndsWith("XL"))
        {
            return partName.Remove(partName.Length - 2, 2);
        }

        if (partName.EndsWith("S") || partName.EndsWith("M")
                                   || partName.EndsWith("L"))
        {
            return partName.Remove(partName.Length - 1);
        }

        return partName;
    }

    [HarmonyPatch(typeof(GameManager),
        nameof(GameManager.OnLoadingFinished))]
    internal static void Prefix()
    {
        LoadDeclaredParts(); // TODO: Move this to a more apropriate call, like the one loading parts or something like that.
    }

    [HarmonyPatch(typeof(Module_Color),
        nameof(Module_Color.OnInitialize))]
    internal static void Postfix(Module_Color __instance)
    {
        var partName = __instance.OABPart is not null ? __instance.OABPart.PartName : __instance.part.Name;
        var trimmedPartName = TrimPartName(partName);
        if (DeclaredParts.Count > 0 && _allParts.Contains(trimmedPartName))
        {
            var mat = new Material(_ksp2Opaque);
            mat.name = __instance.GetComponentInChildren<MeshRenderer>().material.name;

            foreach (var renderer in __instance.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (renderer.material.shader.name != _unityStandard.name)
                {
                    continue;
                }

                SetTexturesToMaterial(trimmedPartName, ref mat);

                renderer.material = mat;

                if (renderer.material.shader.name != _ksp2Opaque.name)
                {
                    renderer.SetMaterial(mat); //Sometimes the material Set doesn't work, this seems to be more reliable.
                }
            }
            __instance.SomeColorUpdated();
        }
    }

    private static void LogMessage(object data)
    {
        Logger.LogMessage($"{data}");
    }

    private static void LogWarning(object data)
    {
        Logger.LogWarning($"{data}");
    }

    private static void LogError(object data)
    {
        Logger.LogError($"{data}");
    }
}
