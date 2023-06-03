using System.Reflection;

namespace SpaceWarp.API.Patching;

public abstract class SimplePatch : PatchBase
{
    public override Selector Selector
    {
        get
        {
            var selfType = this.GetType();
            var attributes = selfType.GetCustomAttributes();
            var selector = new Selector();
            foreach (var attribute in attributes)
            {
                switch (attribute)
                {
                    case WithModuleAttribute { ModuleName: not null } withModuleAttribute:
                        selector.WithModule(withModuleAttribute.ModuleName);
                        break;
                    case WithModuleAttribute withModuleAttribute:
                        selector.WithModule(withModuleAttribute.ModuleType);
                        break;
                    case WithoutModuleAttribute { ModuleName: not null } withoutModuleAttribute:
                        selector.WithoutModule(withoutModuleAttribute.ModuleName);
                        break;
                    case WithoutModuleAttribute withoutModuleAttribute:
                        selector.WithoutModule(withoutModuleAttribute.ModuleType);
                        break;
                    case WithNameAttribute withNameAttribute:
                        selector.WithName(withNameAttribute.Name);
                        break;
                }
            }
            return selector;
        }
    }
}