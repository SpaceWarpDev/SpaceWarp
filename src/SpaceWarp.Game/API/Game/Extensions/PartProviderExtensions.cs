using JetBrains.Annotations;
using KSP.Game;
using KSP.Sim.Definitions;

namespace SpaceWarp.API.Game.Extensions;

/// <summary>
/// Extensions for <see cref="PartProvider" />.
/// </summary>
[PublicAPI]
public static class PartProviderExtensions
{
    /// <summary>
    /// Gets all parts with a module of type <typeparamref name="T" />.
    /// </summary>
    /// <param name="provider">The part provider.</param>
    /// <typeparam name="T">The module type.</typeparam>
    /// <returns>All parts with a module of type <typeparamref name="T" />.</returns>
    public static IEnumerable<PartCore> WithModule<T>(this PartProvider provider) where T : ModuleData
    {
        return provider._partData.Values.Where(part => part.modules.OfType<T>().Any());
    }
}