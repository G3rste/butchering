using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace Butchering
{
    public class BlockEntityButcherTable : BlockEntityButcherWorkstation
    {

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

            var loot = item.ButcheringRewards
                .Append(GetHarvestableProps())
                .Where(drop => !SkinningRackExclusives.Any(exclusive => drop.Code.Path.StartsWith(exclusive))).ToArray();

            DropLoot(byPlayer, offset, loot);

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