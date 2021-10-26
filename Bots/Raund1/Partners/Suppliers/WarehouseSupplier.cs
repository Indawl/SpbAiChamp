using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Partners.Suppliers
{
    public class WarehouseSupplier : Supplier
    {
        public WarehouseSupplier(int planetId, int number, Resource? resource = null, int delay = 0) :
            base(planetId, number, resource, delay)
        {
        }
    }
}
