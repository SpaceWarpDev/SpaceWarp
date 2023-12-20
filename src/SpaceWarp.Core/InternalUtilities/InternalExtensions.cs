using System.Reflection;
using System;
using UnityEngine;
using System.Collections;

namespace SpaceWarp.InternalUtilities;

internal static class InternalExtensions
{
    internal static void Persist(this UnityObject obj)
    {
        UnityObject.DontDestroyOnLoad(obj);
        obj.hideFlags |= HideFlags.HideAndDontSave;
    }

    internal static void CopyFieldAndPropertyDataFromSourceToTargetObject(object source, object target)
    {
        // check if it's a dictionary
        if (source is IDictionary sourceDictionary && target is IDictionary targetDictionary)
        {
            // copy dictionary items
            foreach (DictionaryEntry entry in sourceDictionary)
            {
                targetDictionary[entry.Key] = entry.Value;
            }
        }
        else
        {
            // copy fields
            foreach (FieldInfo field in source.GetType().GetFields())
            {
                object value = field.GetValue(source);

                try
                {
                    field.SetValue(target, value);
                }
                catch (FieldAccessException)
                { /* some fields are constants */ }
            }

            // copy properties
            foreach (PropertyInfo property in source.GetType().GetProperties())
            {
                object value = property.GetValue(source);
                property.SetValue(target, value);
            }
        }
    }
}