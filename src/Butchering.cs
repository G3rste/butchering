using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Butchering
{
    public class Butchering : ModSystem
    {
        Harmony harmony = new Harmony("gerste.butchering");
        public ButcheringConfig Config;
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            HarvestablePatch.Patch(harmony);

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

    public class HarvestablePatch
    {

        public static void Patch(Harmony harmony)
        {
            harmony.Patch(methodInfo()
                , prefix: new HarmonyMethod(typeof(HarvestablePatch).GetMethod("Postfix", BindingFlags.Static | BindingFlags.Public)));
        }

        public static void Unpatch(Harmony harmony)
        {
            harmony.Unpatch(methodInfo()
                , HarmonyPatchType.Prefix, "gerste.petai");
        }

        public static MethodInfo methodInfo()
        {
            return typeof(EntityBehaviorHarvestable).GetMethod("get_dropQuantityMultiplier", BindingFlags.Instance | BindingFlags.NonPublic);
        }
        public static void Postfix(ref float __result, EntityBehaviorHarvestable __instance)
        {
            if(__instance.entity.HasBehavior<EntityBehaviorButcherable>()){
                __result *= 0.5f;
            }
        }
    }
}
