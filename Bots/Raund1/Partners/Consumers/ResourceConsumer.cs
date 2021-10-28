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
            double cost = 0.0;

            var resourceDetail = Manager.CurrentManager.ResourceDetails[Resource.Value];
            var buildingDetail = Manager.CurrentManager.BuildingDetails[resourceDetail.BuildingType];

            cost += resourceDetail.KoefInOut * Quantity * buildingDetail.BuildingProperties.ProduceScore / buildingDetail.BuildingProperties.ProduceAmount;

            return (int)cost;
        }
    }
}
