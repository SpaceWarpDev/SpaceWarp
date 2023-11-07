using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;
using JetBrains.Annotations;

namespace SpaceWarp.API.Configuration;

public class ListConstraint<T> : ValueConstraint<T> where T : IEquatable<T>
{
    [PublicAPI]
    public List<T> AcceptableValues;

    public ListConstraint(IEnumerable<T> acceptableValues)
    {
        AcceptableValues = acceptableValues.ToList();
    }

    public ListConstraint(params T[] acceptableValues)
    {
        AcceptableValues = acceptableValues.ToList();
    }

    public override bool IsValid(T o) => AcceptableValues.Any(x => x.Equals(o));
    public override AcceptableValueBase ToAcceptableValueBase()
    {
        return new AcceptableValueList<T>(AcceptableValues.ToArray());
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append("[");
        for (int i = 0; i < AcceptableValues.Count; i++)
        {
            if (i != 0)
            {
                sb.Append(", ");
            }

            sb.Append(AcceptableValues[i]);
        }

        sb.Append("]");
        return sb.ToString();
    }
}