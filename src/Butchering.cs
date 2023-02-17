using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Butchering
{
    public class Butchering : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterEntityBehaviorClass("butcherable", typeof(EntityBehaviorButcherable));
        }
    }
}
