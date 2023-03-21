namespace Butchering
{
    public class BlockButcherHook : BlockButcherWorkstation
    {
        protected override string processesState => "dead";

        protected override string langCodePlace => "butchering:put-creature-on-hook";

        protected override string langCodeTake => "butchering:take-creature-from-hook";

        protected override string langCodeProcess => "butchering:skin-creature";
    }
}