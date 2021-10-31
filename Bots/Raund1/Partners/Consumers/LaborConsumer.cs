using SpbAiChamp.Bots.Raund1.Logistics;

namespace SpbAiChamp.Bots.Raund1.Partners.Consumers
{
    public class LaborConsumer : SupplierConsumer
    {
        public LaborConsumer(ShippingPlan shippingPlan) : 
            base(shippingPlan.Supplier.PlanetId, shippingPlan.Number, null, shippingPlan.Supplier.Delay, shippingPlan)
        {
        }

        public LaborConsumer(int planetId, int number, int delay = 0, ShippingPlan shippingPlan = null) :
            base(planetId, number, null, delay, shippingPlan)
        {
        }
    }
}
