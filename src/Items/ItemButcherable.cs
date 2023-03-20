using Vintagestory.API.Common;

namespace Butchering
{
    public class ItemButcherable : Item
    {
        public ButcheringReward[] ButcheringRewards => Attributes["butcheringRewards"].AsObject<ButcheringReward[]>();
        public float Size => Attributes["size"].AsFloat(1);
        public Shape GetHangingShape()
        {
            var shape = Attributes["hangingShape"].AsObject<CompositeShape>(null, Code.Domain);

            return api.Assets.Get<Shape>(shape.Base.CopyWithPath("shapes/" + shape.Base.Path + ".json"));
        }
    }

    public class ButcheringReward
    {
        public string Code { get; set; }
        public int MinAmount { get; set; }
        public int MaxAmount { get; set; }
    }
}