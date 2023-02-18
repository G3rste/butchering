using Vintagestory.API.Common;

namespace Butchering
{
    public class ItemButcherable : Item
    {
        public ButcheringReward[] ButcheringRewards => Attributes["butcheringRewards"].AsObject<ButcheringReward[]>();
        public float Size => Attributes["size"].AsFloat(1);
    }

    public class ButcheringReward
    {
        public string Code { get; set; }
        public int MinAmount { get; set; }
        public int MaxAmount { get; set; }
    }
}