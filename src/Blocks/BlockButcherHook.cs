using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Butchering
{
    public class BlockButcherHook : BlockButcherWorkstation
    {
        public override string processesState => "dead";

        protected override string langCodePlace => "butchering:put-creature-on-hook";

        protected override string langCodeTake => "butchering:take-creature-from-hook";

        protected override string langCodeProcess => "butchering:skin-creature";

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            var workstation = world.BlockAccessor.GetBlockEntity(selection.Position) as BlockEntityButcherWorkstation;

            if (workstation.Inventory[0]?.Itemstack?.Collectible.Variant["state"] == "skinned")
            {
                return new WorldInteraction[0];
            }
            else
            {
                return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);
            }
        }
    }
}