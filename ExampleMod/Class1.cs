using SpaceWarp.API;

namespace ExampleMod
{
    [MainMod]
    public class ExampleModRunner : Mod
    {
        public override void Initialize()
        {
            Logger.Info("Mod is initialized");
        }
    }
}