using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Butchering
{
    public class BlockEntityButcherTable : BlockEntityButcherWorkstation
    {
        public float configuredEfficiency;

        private float tableWidth => (Block as BlockButcherTable).TableWith;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            configuredEfficiency = Api.ModLoader.GetModSystem<Butchering>().Config.butcheringTableLootMultiplier;
        }
        protected override float[][] genTransformationMatrices()
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
                    return new float[][] { new Matrixf().Translate(sideOffset, heightOffset, 0).Values };
                case "east":
                    return new float[][] { new Matrixf().Translate(1, heightOffset, sideOffset).RotateYDeg(270).Values };
                case "south":
                    return new float[][] { new Matrixf().Translate(-sideOffset + 1, heightOffset, 1).RotateYDeg(180).Values };
                case "west":
                    return new float[][] { new Matrixf().Translate(0, heightOffset, -sideOffset + 1).RotateYDeg(90).Values };
                default:
                    return new float[][] { new Matrixf().Values };
            }
        }

        protected override bool processItem(IPlayer byPlayer)
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
            float efficiency = (Block as BlockButcherTable).ButcheringEfficiency * configuredEfficiency;
            foreach (var loot in item.ButcheringRewards)
            {
                Api.World.PlaySoundAt(new AssetLocation("sounds/thud"), byPlayer.Entity, byPlayer, false);
                int lootAmount = Math.Max((int)(getNextRandomDoubleBetween(Api.World.Rand, loot.MinAmount, loot.MaxAmount + 1) * efficiency * inventory[0].Itemstack.Attributes.GetFloat("AnimalWeight", 1) * byPlayer.Entity.Stats.GetBlended("animalLootDropRate")), loot.AbsoluteMinAmount);
                if (lootAmount > 0)
                {
                    Api.World.SpawnItemEntity(
                        new ItemStack(Api.World.GetItem(new AssetLocation(loot.Code)), lootAmount),
                        Pos.ToVec3d().Add(offset));
                }
            }
            if (inventory[0].Itemstack.Attributes.HasAttribute("AnimalCarcass"))
            {
                Block carcass = Api.World.GetBlock(new AssetLocation(inventory[0].Itemstack.Attributes.GetString("AnimalCarcass")));
                Api.World.BlockAccessor.SetBlock(carcass.Id, Pos.ToVec3d().Add(offset).AsBlockPos);
            }
            inventory[0].TakeOutWhole();
            updateMesh(0);
            return base.processItem(byPlayer);
        }
    }
}