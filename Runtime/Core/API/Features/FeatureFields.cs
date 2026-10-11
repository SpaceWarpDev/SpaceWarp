using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace SpaceWarp2.API.Features;

/// <summary>
/// Writes the <see cref="FeatureAttribute" /> fields of an object from the active set.
/// </summary>
[PublicAPI]
public static class FeatureFields
{
    private const BindingFlags FIELD_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    // Every set activation binds every plugin, so each type is scanned once
    private static readonly Dictionary<Type, (FieldInfo field, string featureId)[]> FieldsByType = new();

    /// <summary>
    /// Sets each <see cref="FeatureAttribute" /> bool field of an object to whether its feature is on.
    /// </summary>
    /// <param name="target">The object, normally a mod's plugin.</param>
    /// <param name="modId">The mod bare feature ids belong to.</param>
    public static void Bind(object target, string modId)
    {
        foreach (var (field, featureId) in FieldsOf(target.GetType()))
        {
            field.SetValue(target, ActiveFeatures.IsEnabled(FeatureIds.Qualify(modId, featureId)));
        }
    }

    private static (FieldInfo field, string featureId)[] FieldsOf(Type type)
    {
        if (FieldsByType.TryGetValue(type, out var fields))
            return fields;

        fields = HierarchyOf(type)
            .SelectMany(level => level.GetFields(FIELD_FLAGS | BindingFlags.DeclaredOnly))
            .Where(field => field.FieldType == typeof(bool))
            .Select(field => (field, attribute: field.GetCustomAttribute<FeatureAttribute>()))
            .Where(pair => pair.attribute != null)
            .Select(pair => (pair.field, pair.attribute!.FeatureId))
            .ToArray();
        FieldsByType[type] = fields;
        return fields;
    }

    // Private fields of base classes are only reachable from the class that declares them
    private static IEnumerable<Type> HierarchyOf(Type type)
    {
        for (Type? level = type; level != null && level != typeof(object); level = level.BaseType)
        {
            yield return level;
        }
    }
}
