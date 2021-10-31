using System.Collections.Generic;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Managment;
using SpbAiChamp.Bots.Raund1.Partners.Suppliers;

namespace SpbAiChamp.Bots.Raund1.Partners.Consumers
{
    public class Consumer : Partner
    {
        public Consumer(int planetId, int number, Resource? resource = null, int delay = 0) :
            base(planetId, number, resource, delay)
        {
        }

        public virtual void GetAction(Supplier supplier, int number, List<MoveAction> moveActions, List<BuildingAction> buildingActions)
        {
            if (supplier.PlanetId == PlanetId || supplier.Delay > 0) return;

            moveActions.Add(new MoveAction(supplier.PlanetId,
                Manager.CurrentManager.PlanetDetails[PlanetId].ShortestWay.GetNextPlanetInv(supplier.PlanetId),
                number, supplier.Resource));
        }

        public virtual int CalculateCost(Supplier supplier)
        {
            if (supplier.IsFake) return ToInt(supplier.CalculateCost(this));

            var building = Manager.CurrentManager.PlanetDetails[PlanetId].Planet.Building;
            double cost = BuildingDetail.GetCost(building.HasValue ? building.Value.BuildingType
                                                                   : Manager.CurrentManager.Orders[PlanetId].BuildingType);
            return ToInt(cost * supplier.CalculateCost(this));
        }

        protected int ToInt(double value) => value > int.MaxValue ? int.MaxValue : (int)value;
    }
}
