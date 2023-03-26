using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Butchering
{
    public abstract class BlockButcherWorkstation : Block
    {
        public List<ItemButcherable> butcherableList = new List<ItemButcherable>();
        public List<ItemKnife> knifeList = new List<ItemKnife>();

        public abstract string processesState { get; }
        protected abstract string langCodePlace { get; }
        protected abstract string langCodeTake { get; }
        protected abstract string langCodeProcess { get; }

        public float ButcheringEfficiency => Attributes["butcheringEfficiency"].AsFloat(1f);

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            foreach (var item in api.World.Collectibles)
            {
                if (item is ItemButcherable butcherable && butcherable.ProcessingState == processesState)
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
            BlockEntityButcherWorkstation workstationEntity = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityButcherWorkstation;
            if (byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack?.Item is ItemKnife
                && workstationEntity?.Inventory?[0]?.Itemstack?.Item is ItemButcherable butcherable
                && butcherable.ProcessingState == processesState)
            {
                world.PlaySoundAt(new AssetLocation("sounds/player/scrape"), byPlayer.Entity, byPlayer, false, 12);
            }
            return true;
        }

        public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BlockEntityButcherWorkstation workstationEntity = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityButcherWorkstation;
            return secondsUsed < 5.5
                && byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack?.Item is ItemKnife
                && workstationEntity?.Inventory?[0]?.Itemstack?.Item is ItemButcherable butcherable
                && butcherable.ProcessingState == processesState;
        }

        public override void OnBlockInteractStop(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BlockEntityButcherWorkstation workstationEntity = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityButcherWorkstation;
            if (workstationEntity != null)
            {
                workstationEntity.OnInteract(byPlayer, secondsUsed);
            }
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            var workstation = world.BlockAccessor.GetBlockEntity(selection.Position) as BlockEntityButcherWorkstation;
            if (workstation?.Inventory.Empty == true)
            {
                return new WorldInteraction[]{
                    new WorldInteraction(){
                        ActionLangCode = langCodePlace,
                        Itemstacks = butcherableList.ConvertAll<ItemStack>(item => new ItemStack(item)).ToArray(),
                        MouseButton = EnumMouseButton.Right
                    }
                };
            }
            if (workstation?.Inventory.Empty == false)
            {
                var interactions = new List<WorldInteraction>(){new WorldInteraction(){
                        ActionLangCode = langCodeTake,
                        MouseButton = EnumMouseButton.Right,
                        RequireFreeHand = true
                    }};
                if (workstation?.Inventory[0]?.Itemstack?.Item is ItemButcherable item && item.ProcessingState == processesState)
                {
                    interactions.Add(new WorldInteraction()
                    {
                        ActionLangCode = langCodeProcess,
                        Itemstacks = knifeList.ConvertAll<ItemStack>(knife => new ItemStack(knife)).ToArray(),
                        MouseButton = EnumMouseButton.Right
                    });
                }
                return interactions.ToArray();
            };

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