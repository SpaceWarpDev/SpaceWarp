using SpaceWarp.API;

namespace ExampleMod
{
    [MainMod]
    public class ExampleModRunner : Mod
    {
        public override void Initialize()
        {
            base.Initialize();

            Logger.Info("Mod is initialized");
        }
    }
}