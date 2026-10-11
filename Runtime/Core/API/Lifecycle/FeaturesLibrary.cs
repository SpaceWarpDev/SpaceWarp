using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using SpaceWarp2.API.Features;

namespace SpaceWarp2.API.Lifecycle;

/// <summary>
/// The <c>Features</c> global: a read-only view of the active feature set.
/// </summary>
[PublicAPI]
[MoonSharpUserData]
public class FeaturesLibrary
{
    /// <summary>
    /// Gets the one instance every environment shares.
    /// </summary>
    [MoonSharpHidden]
    public static readonly FeaturesLibrary Instance = new();

    static FeaturesLibrary()
    {
        UserData.RegisterType<FeaturesLibrary>();
    }

    /// <summary>
    /// Checks whether a feature is on.
    /// </summary>
    /// <param name="context">The calling script, whose <c>ModId</c> a bare id belongs to.</param>
    /// <param name="featureId">The feature id, bare or qualified.</param>
    /// <returns>True if the feature is on.</returns>
    public bool Has(ScriptExecutionContext context, string featureId)
    {
        string modId = context.CurrentGlobalEnv?.Get("ModId").CastToString();
        return ActiveFeatures.IsEnabled(modId == null ? featureId : FeatureRegistry.Qualify(modId, featureId));
    }

    /// <summary>
    /// Lists the enabled features as a set.
    /// </summary>
    /// <param name="context">The calling script.</param>
    /// <returns>A new table with each enabled id as a key mapped to true, or nil before any set is active.</returns>
    public DynValue All(ScriptExecutionContext context)
    {
        if (ActiveFeatures.Enabled == null)
            return DynValue.Nil;

        var set = new Table(context.GetScript());
        foreach (string id in ActiveFeatures.Enabled)
        {
            set[id] = true;
        }

        return DynValue.NewTable(set);
    }
}
