using SpaceWarp.API.Mods;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace SpaceWarp.Backend.UI.Loading;

internal class LoadingScreenManager : ResourceProviderBase
{
    public Dictionary<string, Sprite> LoadingScreens = new();
    private List<ResourceLocationData> _locations = new();
    
    public void SetupResourceLocator()
    {
        Addressables.AddResourceLocator(new ResourceLocationMap("sw-loading-screen-map", _locations));
        Addressables.ResourceManager.ResourceProviders.Add(this);
    }
    public void LoadScreens(Curtain curtain)
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

        foreach (var screen in curtain.LoadingScreens.Values.Distinct())
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
        LoadingScreens.Add(location,sprite);
        _locations.Add(new ResourceLocationData(new[] { location }, location, typeof(Sprite), typeof(Sprite)));
    }

    public override void Provide(ProvideHandle provideHandle)
    {
        if (!LoadingScreens.ContainsKey(provideHandle.Location.PrimaryKey))
        {
            provideHandle.Complete<Sprite>(null, false,
                new ProviderException($"Unknown loading screen {provideHandle.Location.PrimaryKey}"));
        }

        provideHandle.Complete(LoadingScreens[provideHandle.Location.PrimaryKey], true, null);
    }
}