using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Butchering
{
    public class Butchering : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterItemClass("butcherable", typeof(ItemButcherable));

            api.RegisterBlockClass("BlockButcherTable", typeof(BlockButcherTable));

            api.RegisterBlockEntityClass("ButcherTable", typeof(BlockEntityButcherTable));

            api.RegisterEntityBehaviorClass("butcherable", typeof(EntityBehaviorButcherable));
        }
    }
}
