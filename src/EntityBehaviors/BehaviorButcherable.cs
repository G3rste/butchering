using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Butchering
{
    public class EntityBehaviorButcherable : EntityBehavior
    {
        CollectibleObject item;
        public EntityBehaviorButcherable(Entity entity) : base(entity)
        {
        }
        public override void Initialize(EntityProperties properties, JsonObject attributes)
        {
            base.Initialize(properties, attributes);
            item = entity.World.GetItem(new AssetLocation(attributes["item"].AsString()));
            if (item == null)
            {
                item = entity.World.GetBlock(new AssetLocation(attributes["item"].AsString()));
            }
        }

        public override void OnInteract(EntityAgent byEntity, ItemSlot itemslot, Vec3d hitPosition, EnumInteractMode mode, ref EnumHandling handled)
        {
            if (!entity.Alive
                && byEntity is EntityPlayer player
                && mode == EnumInteractMode.Interact
                && player.RightHandItemSlot.Empty
                && entity.GetBehavior<EntityBehaviorHarvestable>()?.IsHarvested != true)
            {
                handled = EnumHandling.PreventDefault;
                var itemWithTexture = string.IsNullOrEmpty(item.Variant["texture"])
                    ? item
                    : entity.World.GetItem(item.CodeWithVariant("texture", (entity.WatchedAttributes.GetInt("textureIndex", 0) + 1).ToString()));
                if (itemWithTexture == null)
                {
                    throw new Exception(string.Format("Could not find butchering item {0}", item.CodeWithVariant("texture", (entity.WatchedAttributes.GetInt("textureIndex", 0) + 1).ToString()).Path));
                }
                var stack = new ItemStack(itemWithTexture);
                if (entity.HasBehavior<EntityBehaviorHarvestable>())
                {
                    stack.Attributes.SetFloat("AnimalWeight", entity.GetBehavior<EntityBehaviorHarvestable>().AnimalWeight);
                }
                if (player.Player.InventoryManager.TryGiveItemstack(stack))
                {
                    entity.Die(EnumDespawnReason.PickedUp);
                }
            }
        }

        public override WorldInteraction[] GetInteractionHelp(IClientWorldAccessor world, EntitySelection es, IClientPlayer player, ref EnumHandling handled)
        {
            if (!entity.Alive
                && entity.GetBehavior<EntityBehaviorHarvestable>()?.IsHarvested != true)
            {
                return new WorldInteraction[]{
                    new WorldInteraction(){
                        ActionLangCode = item.StorageFlags == EnumItemStorageFlags.Backpack ? Lang.Get("butchering:right-click-pick-up-empty-backpack-slot") : Lang.Get("butchering:right-click-pick-up-empty-slot"),
                        MouseButton = EnumMouseButton.Right,
                        RequireFreeHand = true
                    }
                };
            }
            else { return base.GetInteractionHelp(world, es, player, ref handled); }
        }

        public override string PropertyName()
        {
            return "butcherable";
        }
    }
}