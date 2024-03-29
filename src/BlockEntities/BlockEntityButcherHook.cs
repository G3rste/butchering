using System;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace Butchering
{
    public class BlockEntityButcherHook : BlockEntityButcherWorkstation
    {
        private long bleedingListenerId;

        public double skinnedAt;

        const double hoursToBleedOut = 2;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            bleedingListenerId = Api.World.RegisterGameTickListener(dropBloodDroplets, 1200);
            configuredEfficiency = Api.ModLoader.GetModSystem<Butchering>().Config.SkinningRackLootMultiplier;
        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            Api.World.UnregisterGameTickListener(bleedingListenerId);
        }

        private void dropBloodDroplets(float obj)
        {
            if (skinnedAt != 0 && Api.World.Calendar.ElapsedHours - skinnedAt > hoursToBleedOut && Api.Side == EnumAppSide.Server)
            {
                SwitchToBledOut();
            }
            if (Api.Side == EnumAppSide.Client && inventory[0]?.Itemstack?.Item is ItemButcherable item && item.Variant["state"] == "skinned" && Api.World.Rand.NextDouble() < 0.3f)
            {
                SimpleParticleProperties blood = new SimpleParticleProperties(
                        0, 9,
                        ColorUtil.ToRgba(255, 75, 0, 0),
                        new Vec3d(),
                        new Vec3d(),
                        new Vec3f(-0.55f, 0f, -0.57f),
                        new Vec3f(0.60f, 0f, 0.49f),
                        3f,
                        1f,
                        0.2f,
                        0.4f,
                        EnumParticleModel.Cube
                    );

                blood.MinPos = Pos.ToVec3d().AddCopy(0.5, 0.3, 0.5);
                Api.World.SpawnParticles(blood);
            }
        }

        protected override bool TryTake(IPlayer byPlayer)
        {
            if (inventory[0]?.Itemstack?.Item is ItemButcherable item && item.Variant["state"] == "skinned")
            {
                return false;
            }
            else
            {
                return base.TryTake(byPlayer);
            }
        }

        private void SwitchToBledOut()
        {
            if (inventory[0]?.Itemstack?.Item is ItemButcherable item && item.Variant["state"] == "skinned")
            {
                inventory[0].Itemstack = cloneStack(inventory[0].Itemstack, Api.World.GetItem(item.CodeWithVariant("state", "bledout")));
                MarkDirty(true);
            }
        }

        protected override string getMeshCacheKey(ItemStack stack)
        {
            return string.Format("hangingShape:{0}:{1}", stack.Item.Code.Domain, stack.Item.Code.Path);
        }
        protected override MeshData getOrCreateMesh(ItemStack stack, int index)
        {
            if (Api is ICoreClientAPI capi && stack.Item is ItemButcherable item)
            {
                nowTesselatingObj = item;
                string key = getMeshCacheKey(stack);

                MeshCache.TryGetValue(key, out var mesh);
                if (mesh != null) return mesh;

                capi.Tesselator.TesselateShape("hangingshape", item.GetHangingShape(Api), out mesh, this);

                MeshCache[key] = mesh;

                return mesh;
            }
            return null;
        }

        protected override float[][] genTransformationMatrices()
        {
            switch (Block.Variant["side"])
            {
                case "north":
                    return new float[][] { new Matrixf().Translate(0, 0, 1).RotateYDeg(90).Values };
                case "east":
                    return new float[][] { new Matrixf().Values };
                case "south":
                    return new float[][] { new Matrixf().Translate(1, 0, 0).RotateYDeg(270).Values };
                case "west":
                    return new float[][] { new Matrixf().Translate(1, 0, 1).RotateYDeg(180).Values };
                default:
                    return new float[][] { new Matrixf().Values };
            }
        }

        protected override bool processItem(IPlayer byPlayer)
        {
            var offset = new Vec3d(0.5, 0.5, 0.5);
            var item = inventory[0].Itemstack.Item as ItemButcherable;

            var loot = item.SkinningRewards
                .Append(GetHarvestableProps())
                .Where(drop => SkinningRackExclusives.Any(exclusive => drop.Code.Path.StartsWith(exclusive)))
                .Where(drop => !item.ExcludeRewards.Any(exclusive => drop.Code.Path.Equals(exclusive)))
                .ToArray();

            DropLoot(byPlayer, offset, loot);
            
            inventory[0].Itemstack = cloneStack(inventory[0].Itemstack, Api.World.GetItem(item.CodeWithVariant("state", "skinned")));
            skinnedAt = Api.World.Calendar.ElapsedHours;
            updateMesh(0);
            return base.processItem(byPlayer);
        }

        private ItemStack cloneStack(ItemStack oldStack, CollectibleObject item)
        {
            var newStack = new ItemStack(item);
            if (oldStack.Attributes.HasAttribute("AnimalWeight"))
            {
                newStack.Attributes.SetFloat("AnimalWeight", oldStack.Attributes.GetFloat("AnimalWeight"));
            }
            if (oldStack.Attributes.HasAttribute("AnimalCarcass"))
            {
                newStack.Attributes.SetString("AnimalCarcass", oldStack.Attributes.GetString("AnimalCarcass"));
            }
            if (oldStack.Attributes.HasAttribute("AnimalDrops"))
            {
                newStack.Attributes.SetString("AnimalDrops", oldStack.Attributes.GetString("AnimalDrops"));
            }
            return newStack;
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            skinnedAt = tree.GetDouble("skinnedat");
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetDouble("skinnedat", skinnedAt);
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            if (skinnedAt > 0 && inventory[0]?.Itemstack?.Item is ItemButcherable item && item.Variant["state"] == "skinned")
            {
                double timeLeft = Math.Max(hoursToBleedOut - (Api.World.Calendar.ElapsedHours - skinnedAt), 0);
                int hoursLeft = (int)timeLeft;
                int minutesLeft = (int)((timeLeft - hoursLeft) * 60);
                dsc.AppendLine(Lang.Get("butchering:needs-to-be-hanging-for", hoursLeft, minutesLeft));
            }
        }
    }
}