using System;
using System.Collections.Generic;
using I2.Loc;
using KSP.Game;
using KSP.Game.Flow;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class LoadAddressablesLocalizationsAction : FlowAction
{
    public LoadAddressablesLocalizationsAction() : base("Loading localizations from Addressables")
    {
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            GameManager.Instance.Assets.LoadByLabel(
                "language_source",
                OnLanguageSourceAssetLoaded,
                delegate(IList<LanguageSourceAsset> languageAssetLocations)
                {
                    if (languageAssetLocations != null)
                    {
                        Addressables.Release(languageAssetLocations);
                    }

                    resolve();
                }
            );
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            reject(null);
        }
    }

    private static void OnLanguageSourceAssetLoaded(LanguageSourceAsset asset)
    {
        if (!asset || LocalizationManager.Sources.Contains(asset.mSource))
        {
            return;
        }

        asset.mSource.owner = asset;
        LocalizationManager.AddSource(asset.mSource);
    }
}