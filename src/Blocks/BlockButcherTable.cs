namespace Butchering
{
    public class BlockButcherTable : BlockButcherWorkstation
    {
        public float TableWith => Attributes["butcherTableWidth"].AsFloat(1f);

        protected override string processesState => "skinned";
        protected override string fitsState => "skinned";

        protected override string langCodePlace => "butchering:put-creature-on-table";

        protected override string langCodeTake => "butchering:take-creature-from-table";

        protected override string langCodeProcess => "butchering:butcher-creature";
    }
}