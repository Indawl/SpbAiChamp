using System.Collections.Generic;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Partners.Suppliers;
using SpbAiChamp.Bots.Raund1.Logistics;

namespace SpbAiChamp.Bots.Raund1.Partners.Consumers
{
    public class DummyConsumer : SupplierConsumer
    {
        public DummyConsumer(int number, Resource? resource = null, ShippingPlan shippingPlan = null) : 
            base(0, number, resource, 0, shippingPlan)
        {
            IsFake = true;
        }

        public override void GetAction(Supplier supplier, int number, List<MoveAction> moveActions, List<BuildingAction> buildingActions) { }

        public override int CalculateCost(Supplier supplier) => Resource.HasValue ? 1 : int.MaxValue;
    }
}
