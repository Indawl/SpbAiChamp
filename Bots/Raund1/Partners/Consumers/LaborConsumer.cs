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

            if (Supplier == null)
            {
                var planetDetail = Manager.CurrentManager.PlanetDetails[PlanetId];
                if (planetDetail.Planet.Building.HasValue)
                {
                    var buildingDetail = Manager.CurrentManager.BuildingDetails[planetDetail.Planet.Building.Value.BuildingType];
                    if (buildingDetail.BuildingProperties.ProduceResource.HasValue)
                    {
                        var resourceDetail = Manager.CurrentManager.ResourceDetails[buildingDetail.BuildingProperties.ProduceResource.Value];
                        if (resourceDetail.NumberOut == 0) return 0;

                        var buildingType = Manager.CurrentManager.BuildingDetails[resourceDetail.BuildingType];

                        cost += resourceDetail.KoefInOut * Quantity * buildingType.BuildingProperties.ProduceScore * buildingType.BuildingProperties.ProduceAmount;
                    }
                }
            }

            return (int)cost;
        }
    }
}
