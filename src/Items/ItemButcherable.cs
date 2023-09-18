using Vintagestory.API.Common;

namespace Butchering
{
    public class ItemButcherable : Item
    {
        public BlockDropItemStack[] ButcheringRewards => Attributes["butcheringRewards"].AsObject<BlockDropItemStack[]>(new BlockDropItemStack[0]);
        public BlockDropItemStack[] SkinningRewards => Attributes["skinningRewards"].AsObject<BlockDropItemStack[]>(new BlockDropItemStack[0]);
        public string[] ExcludeRewards => Attributes["excludeRewards"].AsArray<string>(new string[0]);
        public float Size => Attributes["size"].AsFloat(1);
        public string ProcessingState => Variant["state"];
        public Shape GetHangingShape(ICoreAPI api)
        {
            var shape = Attributes["hangingShape"].AsObject<CompositeShape>(null, Code.Domain);

            if (shape == null)
            {
                api.Logger.Error(string.Format("Tried to load shape for a hanging {0}, but it could not be found!", Code.Path));
            }

            return api.Assets.Get<Shape>(shape.Base.CopyWithPath("shapes/" + shape.Base.Path + ".json"));
        }
    }
}