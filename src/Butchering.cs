using Vintagestory.API.Common;

namespace Butchering
{
    public class Butchering : ModSystem
    {
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
        }
    }
}
