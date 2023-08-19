using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Butchering
{
    public class BlockEntityButcherHook : BlockEntityButcherWorkstation
    {
        private long bleedingListenerId;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            // small little backdoor if the server gets shutdown before the entity is bled out
            SwitchToBledOut(0);
            if (Api.Side == EnumAppSide.Client)
            {
                bleedingListenerId = Api.World.RegisterGameTickListener(dropBloodDroplets, 50);
            }
        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            if (Api.Side == EnumAppSide.Client)
            {
                Api.World.UnregisterGameTickListener(bleedingListenerId);
            }
        }

        private void dropBloodDroplets(float obj)
        {
            if (inventory[0]?.Itemstack?.Item is ItemButcherable item && item.Variant["state"] == "skinned" && Api.World.Rand.NextDouble() < 0.3f)
            {
                SimpleParticleProperties blood = new SimpleParticleProperties(
                        0, 9,
                        ColorUtil.ToRgba(255, 75, 0, 0),
                        new Vec3d(),
                        new Vec3d(),
                        new Vec3f(-0.27f, 0f, -0.23f),
                        new Vec3f(0.24f, 0f, 0.26f),
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

        private void SwitchToBledOut(float t)
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

                capi.Tesselator.TesselateShape("hangingshape", item.GetHangingShape(), out mesh, this);

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
            var offset = new Vec3d(0, 1.5, 0);
            switch (Block.Variant["side"])
            {
                case "north":
                    offset.Add(0.5, 0, 0.5);
                    break;
                case "east":
                    offset.Add(0.5, 0, 0.5);
                    break;
                case "south":
                    offset.Add(-0.5 + 1, 0, 0.5);
                    break;
                case "west":
                    offset.Add(0.5, 0, -0.5 + 1);
                    break;
            }
            var item = inventory[0].Itemstack.Item as ItemButcherable;
            MarkDirty(true);
            float efficiency = (Block as BlockButcherHook).ButcheringEfficiency;
            foreach (var loot in item.SkinningRewards)
            {
                Api.World.PlaySoundAt(new AssetLocation("sounds/thud"), byPlayer.Entity, byPlayer, false);
                int lootAmount = (int)(getNextRandomDoubleBetween(Api.World.Rand, loot.MinAmount, loot.MaxAmount + 1) * efficiency * inventory[0].Itemstack.Attributes.GetFloat("AnimalWeight", 1));
                if (lootAmount > 0)
                {
                    Api.World.SpawnItemEntity(
                        new ItemStack(Api.World.GetItem(new AssetLocation(loot.Code)), lootAmount),
                        Pos.ToVec3d().Add(offset));
                }
            }
            inventory[0].Itemstack = cloneStack(inventory[0].Itemstack, Api.World.GetItem(item.CodeWithVariant("state", "skinned")));
            Api.World.RegisterCallback(SwitchToBledOut, 15000);
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
            return newStack;
        }
    }
}