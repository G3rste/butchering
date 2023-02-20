using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Butchering
{
    public class BlockEntityButcherTable : BlockEntityDisplay
    {
        protected InventoryGeneric inventory;
        public override InventoryBase Inventory => inventory;

        public override string InventoryClassName => "butchertable";

        private float tableWidth => (Block as BlockButcherTable).TableWith;

        public BlockEntityButcherTable()
        {
            inventory = new InventoryGeneric(1, "butchertable-0", null, null, (index, self) => new ItemSlotUniversal(self));
            meshes = new MeshData[1];
        }

        public override void TranslateMesh(MeshData mesh, int index)
        {
            var sideOffset = tableWidth / 2;
            if (inventory[0].Itemstack?.Item is ItemButcherable butcherable)
            {
                sideOffset -= butcherable.Size / 2;
            }
            float heightOffset = 1;

            switch (Block.Variant["side"])
            {
                case "north":
                    mesh.Translate(sideOffset, heightOffset, 0);
                    break;
                case "east":
                    mesh.Translate(0, heightOffset, sideOffset);
                    break;
                case "south":
                    mesh.Translate(-sideOffset, heightOffset, 0);
                    break;
                case "west":
                    mesh.Translate(0, heightOffset, -sideOffset);
                    break;
            }
        }

        internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel, float secondsUsed = 0)
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
                        if (slot.Itemstack?.Collectible is ItemButcherable)
                        {
                            return TryPut(byPlayer, blockSel, slot);
                        }
                    }
                }
            }
            if (!inventory.Empty)
            {
                if (activeSlot.Empty)
                {
                    return TryTake(byPlayer, blockSel);
                }
                else if (activeSlot.Itemstack.Item is ItemKnife knife && secondsUsed >= 5)
                {
                    var offset = new Vec3d(0, 1.5, 0);
                    switch (Block.Variant["side"])
                    {
                        case "north":
                            offset.Add(tableWidth / 2, 0, 0.5);
                            break;
                        case "east":
                            offset.Add(0.5, 0, tableWidth / 2);
                            break;
                        case "south":
                            offset.Add(-tableWidth / 2 + 1, 0, 0.5);
                            break;
                        case "west":
                            offset.Add(0.5, 0, -tableWidth / 2 + 1);
                            break;
                    }
                    var item = inventory[0].Itemstack.Item as ItemButcherable;
                    MarkDirty(true);
                    float efficiency = (Block as BlockButcherTable).ButcheringEfficiency;
                    foreach (var loot in item.ButcheringRewards)
                    {
                        Api.World.PlaySoundAt(new AssetLocation("sounds/thud"), byPlayer.Entity, byPlayer, false);
                        int lootAmount = (int)(Api.World.Rand.Next(loot.MinAmount, loot.MaxAmount + 1) * efficiency * inventory[0].Itemstack.Attributes.GetFloat("AnimalWeight", 1));
                        if (lootAmount > 0)
                        {
                            Api.World.SpawnItemEntity(
                                new ItemStack(Api.World.GetItem(new AssetLocation(loot.Code)), lootAmount),
                                Pos.ToVec3d().Add(offset));
                        }
                    }
                    inventory[0].TakeOutWhole();
                    updateMesh(0);
                    return true;
                }
            }
            return false;
        }

        private bool TryPut(IPlayer byPlayer, BlockSelection blockSel, ItemSlot slot)
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

        private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
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