using BepInEx.Configuration;

namespace SpaceWarp.API.Configuration;

public interface IValueConstraint
{
    public bool IsConstrained(object o);
    public AcceptableValueBase ToAcceptableValueBase();
}