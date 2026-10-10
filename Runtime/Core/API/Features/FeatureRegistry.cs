using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SpaceWarp2.API.Mods.JSON;

namespace SpaceWarp2.API.Features;

/// <summary>
/// Every feature the installed mods declare, and the rules that turn a requested set into an enabled one.
/// </summary>
/// <remarks>
/// Each mod gets a default feature, <c>ModId:ModId</c>, which requires the default features of its dependencies and
/// conflicts with those of its conflicts. Every other feature of a mod requires that mod's default feature. Ids
/// match case-insensitively and resolve to the casing they were declared with.
/// </remarks>
[PublicAPI]
public class FeatureRegistry
{
    /// <summary>
    /// Gets every declared feature.
    /// </summary>
    public IEnumerable<FeatureDefinition> All => throw new NotImplementedException();

    /// <summary>
    /// Declares a mod's default feature and the features in its mod info.
    /// </summary>
    /// <param name="info">The mod info.</param>
    public void AddMod(ModInfo info) => throw new NotImplementedException();

    /// <summary>
    /// Declares one feature of a mod outside its mod info.
    /// </summary>
    /// <param name="modId">The declaring mod.</param>
    /// <param name="featureId">The feature id, bare or qualified.</param>
    /// <param name="requires">The required feature ids, bare ones relative to <paramref name="modId" />.</param>
    /// <param name="conflicts">The conflicting feature ids, bare ones relative to <paramref name="modId" />.</param>
    /// <returns>The declared feature.</returns>
    public FeatureDefinition Define(
        string modId,
        string featureId,
        IEnumerable<string>? requires = null,
        IEnumerable<string>? conflicts = null
    ) => throw new NotImplementedException();

    /// <summary>
    /// Looks a feature up by id.
    /// </summary>
    /// <param name="featureId">The qualified feature id, in any casing.</param>
    /// <param name="definition">The feature, when found.</param>
    /// <returns>True if the feature is declared, false otherwise.</returns>
    public bool TryGet(string featureId, out FeatureDefinition definition) => throw new NotImplementedException();

    /// <summary>
    /// Checks whether a feature and everything it requires, transitively, is declared.
    /// </summary>
    /// <remarks>
    /// An unsatisfiable feature is hidden from players, for example one whose mod depends on a mod that is not
    /// installed.
    /// </remarks>
    /// <param name="featureId">The qualified feature id.</param>
    /// <returns>True if the feature can be turned on, false otherwise.</returns>
    public bool IsSatisfiable(string featureId) => throw new NotImplementedException();

    /// <summary>
    /// Turns a requested set of features into the set that ends up on.
    /// </summary>
    /// <param name="requested">The requested qualified feature ids.</param>
    /// <returns>The enabled closure, the unknown ids, and the conflicts between enabled features.</returns>
    public FeatureResolution Resolve(IEnumerable<string> requested) => throw new NotImplementedException();
}
