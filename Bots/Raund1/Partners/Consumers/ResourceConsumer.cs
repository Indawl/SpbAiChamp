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
            => ToInt(BuildingDetail.GetCost(PlanetId, Manager.CurrentManager.ResourceDetails[Resource.Value].BuildingType, true, true));
    }
}
