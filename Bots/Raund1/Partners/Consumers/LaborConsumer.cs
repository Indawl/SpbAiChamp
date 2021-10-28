using SpbAiChamp.Bots.Raund1.Managment;
using SpbAiChamp.Bots.Raund1.Partners.Suppliers;

namespace SpbAiChamp.Bots.Raund1.Partners.Consumers
{
    public class LaborConsumer : Consumer
    {
        public LaborConsumer(int planetId, int number, int delay = 0, Supplier supplier = null) :
            base(planetId, number, null, delay, supplier)
        {
        }

        public override int CalculateCost(Supplier supplier)
        {
            double cost = 0.0;

            if (Supplier == null) // for resources is free
            {
                var planetDetail = Manager.CurrentManager.PlanetDetails[PlanetId];
                var buildingType = planetDetail.Planet.Building?.BuildingType ?? Manager.CurrentManager.Orders[PlanetId].BuildingType;

                if (buildingType.HasValue)
                {
                    var buildingDetail = Manager.CurrentManager.BuildingDetails[buildingType.Value];
                    if (buildingDetail.BuildingProperties.ProduceResource.HasValue)
                    {
                        var resourceDetail = Manager.CurrentManager.ResourceDetails[buildingDetail.BuildingProperties.ProduceResource.Value];
                        cost += resourceDetail.KoefInOut * buildingDetail.BuildingProperties.ProduceScore / buildingDetail.BuildingProperties.ProduceAmount;
                    }
                }
            }

            return (int)cost;
        }
    }
}
