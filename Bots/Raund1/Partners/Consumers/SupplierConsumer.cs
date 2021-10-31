using System.Collections.Generic;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Partners.Suppliers;
using SpbAiChamp.Bots.Raund1.Logistics;

namespace SpbAiChamp.Bots.Raund1.Partners.Consumers
{
    public class SupplierConsumer : Consumer
    {
        public ShippingPlan ShippingPlan { get; protected set; }

        public SupplierConsumer(int planetId, int number, Resource? resource = null, int delay = 0, ShippingPlan shippingPlan = null) :
            base(planetId, number, resource, delay)
        {
            ShippingPlan = shippingPlan;
        }

        public override void GetAction(Supplier supplier, int number, List<MoveAction> moveActions, List<BuildingAction> buildingActions)
        {
            base.GetAction(supplier, number, moveActions, buildingActions);

            ShippingPlan?.GetAction(moveActions, buildingActions);
        }

        public override int CalculateCost(Supplier supplier) => ToInt(base.CalculateCost(supplier) + (ShippingPlan?.CalculateCost() ?? 0));
    }
}
