using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace Butchering
{
    public class BlockEntityMeatHook : BlockEntityDisplay
    {
        protected InventoryGeneric inventory;
        public override InventoryBase Inventory => inventory;

        public override string InventoryClassName => "meathook";

        public override string AttributeTransformCode => "onHookTransform";

        const int rows = 4;
        const int cols = 4;

        private double lastCheck;
        private long listenerId;
        public BlockEntityMeatHook()
        {
            inventory = new InventoryGeneric(rows * cols, "meathook-0", null, null, (index, self) => new ItemSlotUniversal(self));
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            listenerId = api.World.RegisterGameTickListener(smokeContents, 5000);
            lastCheck = api.World.Calendar.ElapsedHours;
        }
        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            Api.World.UnregisterGameTickListener(listenerId);
        }
        private void smokeContents(float dt)
        {
            double newCheck = Api.World.Calendar.ElapsedHours;
            if (Api.World.BlockAccessor.GetBlockEntity(Pos.DownCopy().ToVec3i().ToBlockPos()) is BlockEntityFirepit fire && fire.IsBurning)
            {
                foreach (var slot in inventory)
                {
                    if (slot.Empty || string.IsNullOrEmpty(slot.Itemstack.Item.Attributes?["transformsWhenSmoked"].AsString())) continue;

                    double deltaHours = slot.Itemstack.Attributes.GetDouble("smokingTime", 0);
                    deltaHours += newCheck - lastCheck;
                    slot.Itemstack.Attributes.SetDouble("smokingTime", deltaHours);
                    if (deltaHours > 2)
                    {
                        slot.Itemstack = new ItemStack(Api.World.GetItem(new AssetLocation(slot.Itemstack.Item.Attributes["transformsWhenSmoked"].AsString())));
                        MarkDirty(true);
                    }
                }
            }
            else
            {
                foreach (var slot in inventory)
                {
                    if (slot.Empty) continue;
                    slot.Itemstack.Attributes.SetDouble("smokingTime", 0);
                }
            }
            lastCheck = newCheck;
        }

        internal bool OnInteract(IPlayer byPlayer, float secondsUsed = 0)
        {
            var activeSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (!string.IsNullOrEmpty(activeSlot?.Itemstack?.Item?.Attributes?["transformsWhenSmoked"].AsString()))
            {
                return TryPut(byPlayer, activeSlot);
            }
            if (activeSlot.Empty)
            {
                return TryTake(byPlayer);
            }
            return false;
        }

        protected virtual bool TryPut(IPlayer byPlayer, ItemSlot activeSlot)
        {
            foreach (var slot in inventory)
            {
                if (slot.Empty)
                {
                    int moved = activeSlot.TryPutInto(Api.World, slot);

                    if (moved > 0)
                    {
                        Api.World.PlaySoundAt(new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);

                        MarkDirty(true);
                        return true;
                    }
                }
            }

            return false;
        }

        private bool TryTake(IPlayer byPlayer)
        {
            foreach (var slot in inventory)
            {
                if (!slot.Empty)
                {
                    ItemStack stack = slot.TakeOut(1);
                    stack.Attributes.RemoveAttribute("smokingTime");
                    if (byPlayer.InventoryManager.TryGiveItemstack(stack))
                    {
                        Api.World.PlaySoundAt(new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                    }

                    if (stack.StackSize > 0)
                    {
                        Api.World.SpawnItemEntity(stack, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
                    }

                    MarkDirty(true);
                    return true;
                }
            }

            return false;
        }

        protected override float[][] genTransformationMatrices()
        {
            var list = new List<Matrixf>();
            for (int i = 0; i < rows; i++)
            {
                for (int k = 0; k < cols; k++)
                {
                    switch (Block.Variant["side"])
                    {
                        case "north":
                            list.Add(new Matrixf().Translate(-4f * i / 16, (i == 1 || i == 2 ? 1f : 0f) / 16, -4f * k / 16).Translate(0, 0, 1).RotateYDeg(90));
                            break;
                        case "east":
                            list.Add(new Matrixf().Translate(4f * k / 16, (i == 1 || i == 2 ? 1f : 0f) / 16, -4f * i / 16));
                            break;
                        case "south":
                            list.Add(new Matrixf().Translate(4f * i / 16, (i == 1 || i == 2 ? 1f : 0f) / 16, 4f * k / 16).Translate(1, 0, 0).RotateYDeg(270));
                            break;
                        case "west":
                            list.Add(new Matrixf().Translate(-4f * k / 16, (i == 1 || i == 2 ? 1f : 0f) / 16, 4f * i / 16).Translate(1, 0, 1).RotateYDeg(180));
                            break;
                    }

                }
            }
            return list.ConvertAll<float[]>(matrix => matrix.Values).ToArray();
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            double minDuration = 0;
            foreach(var slot in inventory){
                if (slot.Empty) continue;
                minDuration = Math.Max(minDuration, slot.Itemstack.Attributes.GetDouble("smokingTime", 0));
            }
            if(minDuration > 0){
                double timeLeft = 2 - minDuration;
                int hoursLeft = (int)timeLeft;
                int minutesLeft = (int)((timeLeft - hoursLeft) * 60);
                dsc.AppendLine(Lang.Get("butchering:needs-to-be-smoke-for", hoursLeft, minutesLeft));
            }
        }
    }
}