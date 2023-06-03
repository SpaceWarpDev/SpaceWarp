using System;
using System.Collections.Generic;
using KSP.Sim.Definitions;

namespace SpaceWarp.API.Patching;

public class Selector
{
    public List<Func<PartCore, bool>> SelectionFunctions = new();


    // TODO: Make this more efficient for multiple module requirements
    public Selector WithModule(string moduleName) =>
        Where(part =>
            part.modules.Any(x => x.GetType().Name == moduleName || x.GetType().FullName == moduleName));

    public Selector WithModule<T>() => WithModule(typeof(T));

    public Selector WithModule(Type moduleType) => Where(part => part.modules.Any(x => x.GetType() == moduleType));

    public Selector WithName(string name) => Where(part => part.data.partName == name);

    public Selector Where(Func<PartCore, bool> action)
    {
        SelectionFunctions.Add(action);
        return this;
    }

    public bool Select(PartCore partCore)
    {
        return SelectionFunctions.All(x => x(partCore));
    }


    public static Selector operator |(Selector lhs, Selector rhs) =>
        new Selector().Where(part => lhs.Select(part) || rhs.Select(part));

    public static Selector operator &(Selector lhs, Selector rhs)
    {
        Selector selector = new Selector();
        selector.SelectionFunctions.AddRange(lhs.SelectionFunctions);
        selector.SelectionFunctions.AddRange(rhs.SelectionFunctions);
        return selector;
    }
}