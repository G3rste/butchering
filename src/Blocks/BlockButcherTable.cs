using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Butchering
{
    public class BlockButcherTable : Block
    {
        public List<ItemButcherable> butcherableList = new List<ItemButcherable>();
        public List<ItemKnife> knifeList = new List<ItemKnife>();

        public float ButcheringEfficiency => Attributes["butcheringEfficiency"].AsFloat(1f);

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            foreach (var item in api.World.Collectibles)
            {
                if (item is ItemButcherable butcherable)
                {
                    butcherableList.Add(butcherable);
                }
                if (item is ItemKnife knife)
                {
                    knifeList.Add(knife);
                }
            }
        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if(byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack?.Item is ItemKnife){
                world.PlaySoundAt(new AssetLocation("sounds/player/scrape"), byPlayer.Entity, byPlayer, false, 12);
            }
            return true;
        }

        public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            return secondsUsed < 4 && byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack?.Item is ItemKnife;
        }

        public override void OnBlockInteractStop(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BlockEntityButcherTable tableEntity = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityButcherTable;
            if (tableEntity != null)
            {
                tableEntity.OnInteract(byPlayer, blockSel, secondsUsed);
            }
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            var table = world.BlockAccessor.GetBlockEntity(selection.Position) as BlockEntityButcherTable;
            if (table?.Inventory[0].Empty == true)
            {
                return new WorldInteraction[]{
                    new WorldInteraction(){
                        ActionLangCode = "butchering:put-creature-on-table",
                        Itemstacks = butcherableList.ConvertAll<ItemStack>(item => new ItemStack(item)).ToArray(),
                        MouseButton = EnumMouseButton.Right
                    }
                };
            }
            if (table?.Inventory[0].Empty == false)
            {
                return new WorldInteraction[]{
                    new WorldInteraction(){
                        ActionLangCode = "butchering:take-creature-from-table",
                        MouseButton = EnumMouseButton.Right,
                        RequireFreeHand = true
                    },
                    new WorldInteraction(){
                        ActionLangCode = "butchering:butcher-creature",
                        Itemstacks = knifeList.ConvertAll<ItemStack>(item => new ItemStack(item)).ToArray(),
                        MouseButton = EnumMouseButton.Right
                    }
                };
            }
            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);
        }
        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            ItemStack[] items = new ItemStack[Drops.Length];
            for (int i = 0; i < Drops.Length; i++)
            {
                items[i] = Drops[i].GetNextItemStack();
            }
            return items;
        }
    }
}