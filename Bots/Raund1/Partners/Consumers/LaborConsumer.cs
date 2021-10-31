using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Logistics;
using SpbAiChamp.Bots.Raund1.Managment;

namespace SpbAiChamp.Bots.Raund1.Partners.Consumers
{
    public class LaborConsumer : SupplierConsumer
    {
        public LaborConsumer(ShippingPlan shippingPlan) : 
            base(shippingPlan.Supplier.PlanetId, shippingPlan.Number, null, shippingPlan.Supplier.Delay, shippingPlan)
        {            
        }

        public LaborConsumer(int planetId, int number, BuildingType? buildingType, int delay = 0) :
            base(planetId, number, null, delay, null)
        {
            if (buildingType.HasValue)
                Resource = Manager.CurrentManager.BuildingDetails[buildingType.Value].BuildingProperties.ProduceResource;
        }
    }
}
