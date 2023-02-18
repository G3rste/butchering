using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Butchering
{
    public class BlockEntityButcherTable : BlockEntityDisplay
    {
        protected InventoryGeneric inventory;
        public override InventoryBase Inventory => inventory;

        public override string InventoryClassName => "butchertable";

        public BlockEntityButcherTable()
        {
            inventory = new InventoryGeneric(1, "butchertable-0", null, null, (index, self) => new ItemSlotUniversal(self));
            meshes = new MeshData[1];
        }

        public override void TranslateMesh(MeshData mesh, int index)
        {
            float sideOffset = 8f / 16f;
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

        internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            var activeSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (inventory.Empty && activeSlot.Itemstack?.Collectible is ItemButcherable)
            {
                return TryPut(byPlayer, blockSel);
            }
            if (!inventory.Empty)
            {
                if (activeSlot.Empty)
                {
                    return TryTake(byPlayer, blockSel);
                }
            }
            return false;
        }

        private bool TryPut(IPlayer byPlayer, BlockSelection blockSel)
        {
            var slot = byPlayer.InventoryManager.ActiveHotbarSlot;
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