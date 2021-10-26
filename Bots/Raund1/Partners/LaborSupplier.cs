using System.Collections.Generic;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Managment;

namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class LaborSupplier : Supplier
    {
        public LaborSupplier(int planetId, int number, int delay = 0) :
            base(planetId, number, null, delay)
        {
            IsInitialAction = true;
        }

        public override int CalculateCost(Consumer consumer)
        {
            return Manager.CurrentManager.PlanetDetails[consumer.PlanetId].ShortestWay.GetDistance(PlanetId);
        }

        public override void GetAction(Consumer consumer, int number, List<MoveAction> moveActions, List<BuildingAction> buildingActions)
        {
            if (consumer.PlanetId != PlanetId)
                moveActions.Add(new MoveAction(PlanetId, Manager.CurrentManager.PlanetDetails[consumer.PlanetId].ShortestWay.GetNextPlanetInv(PlanetId), NumberO, Resource));
            else if (consumer.Supplier != null)
            {
                int supplierId = Manager.CurrentManager.TransportTask.Suppliers.IndexOf(consumer.Supplier);

                for (int j = 0; j < Manager.CurrentManager.TransportTask.Consumers.Count; j++)
                    Manager.CurrentManager.TransportTask.ShippingPlans[supplierId, j].GetAction(moveActions, buildingActions);
            }
        }
    }
}
