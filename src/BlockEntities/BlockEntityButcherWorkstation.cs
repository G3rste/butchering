using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace Butchering
{
    public abstract class BlockEntityButcherWorkstation : BlockEntityDisplay
    {
        protected InventoryGeneric inventory;
        public override InventoryBase Inventory => inventory;

        public override string InventoryClassName => "butcherstation";
        public abstract string processesState { get; }
        public abstract string fitsState { get; }

        public BlockEntityButcherWorkstation()
        {
            inventory = new InventoryGeneric(1, "butcherstation-0", null, null, (index, self) => new ItemSlotUniversal(self));
        }

        internal bool OnInteract(IPlayer byPlayer, float secondsUsed = 0)
        {
            var activeSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (inventory.Empty)
            {
                foreach (var inventory in byPlayer.InventoryManager.Inventories.Values)
                {
                    if (inventory.ClassName == GlobalConstants.creativeInvClassName)
                    {
                        continue;
                    }
                    foreach (var slot in inventory)
                    {
                        if (slot.Itemstack?.Collectible is ItemButcherable butcherable && butcherable.ProcessingState == fitsState)
                        {
                            return TryPut(byPlayer, slot);
                        }
                    }
                }
            }
            if (!inventory.Empty)
            {
                if (activeSlot.Empty)
                {
                    return TryTake(byPlayer);
                }
                else if (activeSlot.Itemstack.Item is ItemKnife knife && secondsUsed >= 5)
                {
                    return processItem(byPlayer);
                }
            }
            return false;
        }

        protected abstract bool processItem(IPlayer byPlayer);

        protected virtual bool TryPut(IPlayer byPlayer, ItemSlot slot)
        {
            int index = 0;

            if (inventory[index].Empty)
            {
                int moved = slot.TryPutInto(Api.World, inventory[index]);

                if (moved > 0)
                {
                    Api.World.PlaySoundAt(new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                    updateMesh(index);

                    MarkDirty(true);
                }

                return moved > 0;
            }

            return false;
        }

        private bool TryTake(IPlayer byPlayer)
        {
            int index = 0;

            if (!inventory[index].Empty)
            {
                ItemStack stack = inventory[index].TakeOut(1);
                if (byPlayer.InventoryManager.TryGiveItemstack(stack))
                {
                    Api.World.PlaySoundAt(new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                }

                if (stack.StackSize > 0)
                {
                    Api.World.SpawnItemEntity(stack, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
                }

                updateMesh(index);
                MarkDirty(true);
                return true;
            }

            return false;
        }
    }
}