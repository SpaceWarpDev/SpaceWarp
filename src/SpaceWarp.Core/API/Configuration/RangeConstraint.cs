using System;
using BepInEx.Configuration;
using JetBrains.Annotations;

namespace SpaceWarp.API.Configuration;

public class RangeConstraint<T> : ValueConstraint<T> where T : IComparable<T>, IComparable
{
    [PublicAPI]
    public T Minimum;
    [PublicAPI]
    public T Maximum;

    public RangeConstraint(T minimum, T maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    public override bool IsValid(T o) => Minimum.CompareTo(o) <= 0 && Maximum.CompareTo(o) >= 0;
    public override AcceptableValueBase ToAcceptableValueBase()
    {
        return new AcceptableValueRange<T>(Minimum, Maximum);
    }

    public override string ToString()
    {
        return $"{Minimum} - {Maximum}";
    }
}