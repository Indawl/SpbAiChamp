using System.Collections.Generic;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Partners.Suppliers;
using SpbAiChamp.Bots.Raund1.Logistics;
using SpbAiChamp.Bots.Raund1.Managment;

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
            if (supplier.PlanetId != PlanetId && supplier.Delay == 0)
            {
                moveActions.Add(new MoveAction(supplier.PlanetId,
                    Manager.CurrentManager.PlanetDetails[PlanetId].ShortestWay.GetNextPlanetInv(supplier.PlanetId),
                    number, supplier.Resource));
            }
            else ShippingPlan?.GetAction(moveActions, buildingActions);
        }

        public override int CalculateCost(Supplier supplier)
        {
            if (supplier.IsFake) return ToInt(supplier.CalculateCost(this));

            if (ShippingPlan == null) return base.CalculateCost(supplier);            
            return ToInt(supplier.CalculateCost(this) + ShippingPlan.Cost);

            //double cost = supplier.CalculateCost(this)
            //            + ShippingPlan.Supplier.CalculateCost(ShippingPlan.Consumer);
            //cost *= ShippingPlan.Consumer.CalculateCost(ShippingPlan.Supplier)
            //      * (Manager.CurrentManager.BuildingDetails[BuildingType.Replicator].BuildingProperties.ProduceScore + 1 -
            //          Manager.CurrentManager.BuildingDetails[ShippingPlan.Supplier.Resource.HasValue
            //      ? Manager.CurrentManager.ResourceDetails[ShippingPlan.Supplier.Resource.Value].BuildingType
            //      : BuildingType.Replicator].BuildingProperties.ProduceScore);

            //return ToInt(cost);
        }
    }
}
