using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Managment;
using SpbAiChamp.Bots.Raund1.Partners.Suppliers;

namespace SpbAiChamp.Bots.Raund1.Partners.Consumers
{
    public class ResourceConsumer : Consumer
    {
        public ResourceConsumer(int planetId, int number, Resource resource, int delay = 0) :
            base(planetId, number, resource, delay)
        {
        }

        public override int CalculateCost(Supplier supplier)
        {
            if (supplier.IsFake) return ToInt(supplier.CalculateCost(this));

            var building = Manager.CurrentManager.PlanetDetails[PlanetId].Planet.Building;
            double cost = BuildingDetail.GetCost(building.HasValue ? building.Value.BuildingType
                                                                   : Manager.CurrentManager.Orders[PlanetId].BuildingType,
                                                 true);
            return ToInt(cost * supplier.CalculateCost(this));
        }
    }
}
