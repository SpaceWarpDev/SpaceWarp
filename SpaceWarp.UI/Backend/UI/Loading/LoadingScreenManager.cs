using System.Collections.Generic;
using System.IO;
using KSP.Game;
using SpaceWarp.API.Mods;
using UniLinq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace SpaceWarp.Backend.UI.Loading;

internal class LoadingScreenManager : ResourceProviderBase
{
    private Dictionary<string, Sprite> _loadingScreens = new();
    private List<ResourceLocationData> _locations = new();
    
    public void SetupResourceLocator()
    {
        Addressables.AddResourceLocator(new ResourceLocationMap("sw-loading-screen-map", _locations));
        Addressables.ResourceManager.ResourceProviders.Add(this);
    }
    public void LoadScreens()
    {
        List<string> allKeysToAdd = new();
        foreach (var mod in PluginList.AllEnabledAndActivePlugins.Where(x => x.DoLoadingActions))
        {
            var directoryInfo = new DirectoryInfo(Path.Combine(mod.Folder.FullName, "assets", "loading_screens"));
            if (!directoryInfo.Exists) continue;
            foreach (var file in directoryInfo.EnumerateFiles())
            {
                var loc = ("loading-screen-" + mod.Guid + "-" + file.FullName).ToLower();
                RegisterScreen(loc, file);
                allKeysToAdd.Add(loc);
            }
        }

        foreach (var screen in GameManager.Instance.Game.UI.Curtain.LoadingScreens.Values.Distinct())
        {
            screen.ScreenKeys.AddRange(allKeysToAdd);
        }
    }

    private void RegisterScreen(string location, FileInfo file)
    {
        var bytes = File.ReadAllBytes(file.FullName);
        var tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        sprite.name = location;
        _loadingScreens.Add(location,sprite);
        _locations.Add(new ResourceLocationData(new[] { location }, location, typeof(Sprite), typeof(Sprite)));
    }

    public override void Provide(ProvideHandle provideHandle)
    {
        if (!_loadingScreens.ContainsKey(provideHandle.Location.PrimaryKey))
        {
            provideHandle.Complete<Sprite>(null, false,
                new ProviderException($"Unknown loading screen {provideHandle.Location.PrimaryKey}"));
        }

        provideHandle.Complete(_loadingScreens[provideHandle.Location.PrimaryKey], true, null);
    }
}