using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using KSP.Game;
using KSP.Sim.Definitions;

namespace SpaceWarp.API.Game.Extensions;

[PublicAPI]
public static class PartProviderExtensions
{
    public static IEnumerable<PartCore> WithModule<T>(this PartProvider provider) where T : ModuleData
    {
        return provider._partData.Values.Where(part => part.modules.OfType<T>().Any());
    }
}