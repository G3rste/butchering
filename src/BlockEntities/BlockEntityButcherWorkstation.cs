using System;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Butchering
{
    public abstract class BlockEntityButcherWorkstation : BlockEntityDisplay
    {
        public readonly string[] SkinningRackExclusives = new string[] { "hide-", "fat" };
        protected InventoryGeneric inventory;
        public override InventoryBase Inventory => inventory;

        public override string InventoryClassName => "butcherstation";
        public string processesState => (Block as BlockButcherWorkstation).processesState;
        public float configuredEfficiency;

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
                        if (slot.Itemstack?.Collectible is ItemButcherable butcherable && butcherable.ProcessingState == processesState)
                        {
                            return TryPut(byPlayer, slot);
                        }
                    }
                }
            }
            else
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

        protected virtual bool processItem(IPlayer byPlayer)
        {
            byPlayer.InventoryManager.ActiveHotbarSlot?.Itemstack?.Collectible?.DamageItem(byPlayer.Entity.World, byPlayer.Entity, byPlayer.InventoryManager.ActiveHotbarSlot, 10);
            return true;
        }

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

        protected virtual bool TryTake(IPlayer byPlayer)
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

        public static double getNextRandomDoubleBetween(Random random, double minAmount, double maxAmount)
        {
            return (random.NextDouble() * (maxAmount - minAmount)) + minAmount;
        }

        protected void DropLoot(IPlayer byPlayer, Vec3d offset, BlockDropItemStack[] drops)
        {
            var item = inventory[0].Itemstack.Item as ItemButcherable;
            MarkDirty(true);
            float efficiency = (Block as BlockButcherWorkstation).ButcheringEfficiency * configuredEfficiency;
            foreach (var loot in drops)
            {
                Api.World.PlaySoundAt(new AssetLocation("sounds/thud"), byPlayer.Entity, byPlayer, false);
                loot.Resolve(Api.World, "Butchering ", item.Code);
                var stack = loot.GetNextItemStack(efficiency * inventory[0].Itemstack.Attributes.GetFloat("AnimalWeight", 1) * byPlayer.Entity.Stats.GetBlended("animalLootDropRate"));
                if (stack != null && stack.StackSize > 0)
                {
                    Api.World.SpawnItemEntity(
                        stack,
                        Pos.ToVec3d().Add(offset));
                }
            }
        }

        protected BlockDropItemStack[] GetHarvestableProps()
        {
            return JsonConvert.DeserializeObject<BlockDropItemStack[]>(inventory[0].Itemstack.Attributes.GetString("AnimalDrops", "[]"));
            //return SerializerUtil.Deserialize<BlockDropItemStack[]>(inventory[0].Itemstack.Attributes.GetBytes("AnimalDrops"), new BlockDropItemStack[0]);
        }
    }
}