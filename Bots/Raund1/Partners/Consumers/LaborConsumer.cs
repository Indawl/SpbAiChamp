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
            if (Supplier == null)
            {
                var planetDetail = Manager.CurrentManager.PlanetDetails[PlanetId];
                var order = Manager.CurrentManager.Orders[PlanetId];
                var buildingType = order.BuildingType.HasValue ? order.BuildingType : planetDetail.Planet.Building?.BuildingType ?? null;

                if (buildingType.HasValue)
                {
                    var buildingDetail = Manager.CurrentManager.BuildingDetails[buildingType.Value];

                    if (buildingDetail.BuildingProperties.ProduceResource.HasValue)
                        return Manager.CurrentManager.ResourceDetails[buildingDetail.BuildingProperties.ProduceResource.Value].GetCost(PlanetId, false);
                    else return buildingDetail.BuildingProperties.ProduceScore;
                }
            }

            return 0;
        }
    }
}
