using Vintagestory.API.Common;
using System.Collections.Generic;

namespace Butchering
{
    public class ItemButcherable : Item
    {
        public ButcheringReward[] ButcheringRewards => Attributes["butcheringRewards"].AsObject<ButcheringReward[]>();
    }

    public class ButcheringReward
    {
        public string Code { get; set; }
        public int MinAmount { get; set; }
        public int MaxAmount { get; set; }
    }
}