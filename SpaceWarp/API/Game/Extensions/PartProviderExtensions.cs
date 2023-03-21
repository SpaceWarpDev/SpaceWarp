using KSP.Game;
using KSP.Sim.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceWarp.API.Game.Extensions;

public static class PartProviderExtensions
{
    public static IEnumerable<PartCore> WithModule<T>(this PartProvider provider) where T : ModuleData
    {
        return provider._partData.Values.Where(part => part.modules.OfType<T>().Any());
    }
    
    
}
