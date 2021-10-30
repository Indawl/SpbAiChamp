using System.Collections.Generic;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Partners.Suppliers;
using SpbAiChamp.Bots.Raund1.Managment;

namespace SpbAiChamp.Bots.Raund1.Partners.Consumers
{
    public class SupplierConsumer : Consumer
    {
        public Supplier Supplier { get; protected set; }

        public SupplierConsumer(int planetId, int number, Resource? resource = null, int delay = 0, Supplier supplier = null) :
            base(planetId, number, resource, delay)
        {
            Supplier = supplier;
        }

        public override void GetAction(Supplier supplier, int number, List<MoveAction> moveActions, List<BuildingAction> buildingActions)
        {
            base.GetAction(supplier, number, moveActions, buildingActions);

            if (Supplier == null) return;

            var transportTask = Manager.CurrentManager.TransportTask(Supplier.Resource);

            int supplierId = transportTask.Suppliers.IndexOf(Supplier);

            for (int j = 0; j < transportTask.Consumers.Count; j++)
                transportTask.ShippingPlans[supplierId, j].GetAction(moveActions, buildingActions);
        }

        public override int CalculateCost(Supplier supplier)
        {
            double cost = 0;

            if (Supplier != null)
            {
                var transportTask = Manager.CurrentManager.TransportTask(Supplier.Resource);

                int supplierId = transportTask.Suppliers.IndexOf(Supplier);
                int count = 0;

                for (int j = 0; j < transportTask.Consumers.Count; j++)
                    if (!transportTask.ShippingPlans[supplierId, j].Consumer.IsFake)
                    {
                        cost += transportTask.ShippingPlans[supplierId, j].Cost;
                        count++;
                    }
                if (count == 0) return int.MaxValue;
                else cost /= count;
            }

            return ToInt(cost + base.CalculateCost(supplier));
        }
    }
}
