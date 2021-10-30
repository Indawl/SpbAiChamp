using SpbAiChamp.Bots.Raund1.Managment;
using SpbAiChamp.Bots.Raund1.Partners.Suppliers;

namespace SpbAiChamp.Bots.Raund1.Partners.Consumers
{
    public class LaborConsumer : SupplierConsumer
    {
        public LaborConsumer(int planetId, int number, int delay = 0, Supplier supplier = null) :
            base(planetId, number, null, delay, supplier)
        {
        }

        public override int CalculateCost(Supplier supplier)
        {
            double cost = base.CalculateCost(supplier);

            var building = Manager.CurrentManager.PlanetDetails[PlanetId].Planet.Building;
            if (building.HasValue)
                cost += BuildingDetail.GetCost(PlanetId, building.Value.BuildingType);

            return ToInt(cost);
        }
    }
}
