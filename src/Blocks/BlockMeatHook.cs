using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Butchering
{
    public class BlockMeatHook : Block
    {
        public List<CollectibleObject> storableList = new List<CollectibleObject>();

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            foreach (var item in api.World.Collectibles)
            {
                if (!string.IsNullOrEmpty(item.Attributes?["transformsWhenSmoked"].AsString()))
                {
                    storableList.Add(item);
                }
            }
        }


        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BlockEntityMeatHook workstationEntity = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityMeatHook;
            if (workstationEntity != null)
            {
                workstationEntity.OnInteract(byPlayer);
            }
            return true;
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            var hook = world.BlockAccessor.GetBlockEntity(selection.Position) as BlockEntityMeatHook;
            var interactions = new List<WorldInteraction>();
            if(hook == null) return interactions.ToArray();

            if (!hook.Inventory.Empty)
            {
                interactions.Add(new WorldInteraction()
                {
                    ActionLangCode = "butchering:take-item-from-hook",
                    MouseButton = EnumMouseButton.Right,
                    RequireFreeHand = true
                });
            }

            bool fitsMoreMeat = false;
            foreach (var slot in hook.Inventory)
            {
                fitsMoreMeat |= slot.Empty;
            }
            if (fitsMoreMeat)
            {
                interactions.Add(new WorldInteraction()
                {
                    ActionLangCode = "butchering:put-item-on-hook",
                    Itemstacks = storableList.ConvertAll<ItemStack>(item => new ItemStack(item)).ToArray(),
                    MouseButton = EnumMouseButton.Right
                });
            };

            return interactions.ToArray();
        }
    }
}