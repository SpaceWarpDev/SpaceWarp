using BepInEx.Configuration;

namespace SpaceWarp.API.Configuration;

public abstract class ValueConstraint<T> : IValueConstraint
{
    public abstract bool IsValid(T o);
    public bool IsValid(object o)
    {
        return IsValid((T)o);
    }

    public bool IsConstrained(object o)
    {
        return IsValid((T)o);
    }

    public abstract AcceptableValueBase ToAcceptableValueBase();
}