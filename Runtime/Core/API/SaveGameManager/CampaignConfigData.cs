using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace SpaceWarp2.API.SaveGameManager;

/// <summary>
/// The mod settings a campaign stores, kept with the rest of its campaign data.
/// </summary>
[PublicAPI]
public class CampaignConfigData
{
    /// <summary>
    /// Each mod's campaign config values by section and key, by mod id. Mods not installed now keep theirs.
    /// </summary>
    public Dictionary<string, JObject> Mods = new();
}
