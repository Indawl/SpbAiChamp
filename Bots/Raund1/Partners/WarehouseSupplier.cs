using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class WarehouseSupplier : Supplier
    {
        public bool IsDummy { get; private set; }

        public WarehouseSupplier(int planetId, int number, Resource? resource = null, int delay = 0, bool isDummy = false) :
            base(planetId, number, resource, delay)
        {
            IsDummy = isDummy;
        }

        public override string ToString() => base.ToString() + "; D: " + IsDummy;
    }
}
