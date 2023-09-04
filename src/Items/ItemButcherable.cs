using Vintagestory.API.Common;

namespace Butchering
{
    public class ItemButcherable : Item
    {
        public ButcheringReward[] ButcheringRewards => Attributes["butcheringRewards"].AsObject<ButcheringReward[]>();
        public ButcheringReward[] SkinningRewards => Attributes["skinningRewards"].AsObject<ButcheringReward[]>();
        public float Size => Attributes["size"].AsFloat(1);
        public string ProcessingState => Variant["state"];
        public Shape GetHangingShape(ICoreAPI api)
        {
            var shape = Attributes["hangingShape"].AsObject<CompositeShape>(null, Code.Domain);

            if(shape == null){
                api.Logger.Error(string.Format("Tried to load shape for a hanging {0}, but it could not be found!", Code.Path));
            }

            return api.Assets.Get<Shape>(shape.Base.CopyWithPath("shapes/" + shape.Base.Path + ".json"));
        }
    }

    public class ButcheringReward
    {
        public string Code { get; set; }
        public int AbsoluteMinAmount { get; set; }
        public double MinAmount { get; set; }
        public double MaxAmount { get; set; }
    }
}