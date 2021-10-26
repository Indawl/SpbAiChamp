using System.Collections.Generic;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Managment;

namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class WarehouseSupplier : Supplier
    {
        public WarehouseSupplier(int planetId, int number, Resource? resource = null, int delay = 0) :
            base(planetId, number, resource, delay)
        {
        }

        public override int CalculateCost(Consumer consumer)
        {
            return Manager.CurrentManager.PlanetDetails[consumer.PlanetId].ShortestWay.GetDistance(PlanetId);
        }

        public override void GetAction(Consumer consumer, int number, List<MoveAction> moveActions, List<BuildingAction> buildingActions)
        {
            if (consumer.PlanetId != PlanetId)
                moveActions.Add(new MoveAction(PlanetId, Manager.CurrentManager.PlanetDetails[consumer.PlanetId].ShortestWay.GetNextPlanetInv(PlanetId), Number, Resource));
        }
    }
}
