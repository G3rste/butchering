using Vintagestory.API.Common;

namespace Butchering
{
    public class Butchering : ModSystem
    {
        public ButcheringConfig Config;
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterItemClass("butcherable", typeof(ItemButcherable));

            api.RegisterBlockClass("BlockButcherTable", typeof(BlockButcherTable));
            api.RegisterBlockClass("BlockButcherHook", typeof(BlockButcherHook));
            api.RegisterBlockClass("BlockMeatHook", typeof(BlockMeatHook));

            api.RegisterBlockEntityClass("ButcherTable", typeof(BlockEntityButcherTable));
            api.RegisterBlockEntityClass("ButcherHook", typeof(BlockEntityButcherHook));
            api.RegisterBlockEntityClass("MeatHook", typeof(BlockEntityMeatHook));

            api.RegisterEntityBehaviorClass("butcherable", typeof(EntityBehaviorButcherable));

            try
            {
                Config = api.LoadModConfig<ButcheringConfig>("ButcheringConfig.json");
                if (Config != null)
                {
                    api.Logger.Notification("Mod Config successfully loaded.");
                }
                else
                {
                    api.Logger.Notification("No Mod Config specified. Falling back to default settings");
                    Config = new ButcheringConfig();
                }
            }
            catch
            {
                api.Logger.Error("Failed to load custom mod configuration. Falling back to default settings!");
                Config = new ButcheringConfig();
            }
            finally
            {
                api.StoreModConfig(Config, "ButcheringConfig.json");
            }
        }
    }

    public class ButcheringConfig
    {
        public float butcheringTableLootMultiplier = 1;
        public float SkinningRackLootMultiplier = 1;
    }
}
