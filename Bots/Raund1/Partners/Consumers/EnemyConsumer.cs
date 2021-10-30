using System;
using SpbAiChamp.Bots.Raund1.Managment;
using SpbAiChamp.Bots.Raund1.Partners.Suppliers;

namespace SpbAiChamp.Bots.Raund1.Partners.Consumers
{
    public class EnemyConsumer : Consumer
    {
        public EnemyConsumer(int planetId, int number, int delay = 0) : base(planetId, number, null, delay)
        {
        }

        public override int CalculateCost(Supplier supplier)
        {
            double cost = 0;

            if (Delay > 0 && !Manager.CurrentManager.Game.Planets[PlanetId].Building.HasValue)
            {
                var planetDetail = Manager.CurrentManager.PlanetDetails[PlanetId];
                int dist = planetDetail.ShortestWay.GetRealDistance(supplier.PlanetId);
                if (dist > Delay) return int.MaxValue;
                cost += planetDetail.getTransportCost(Delay - dist);
            }

            if (Manager.CurrentManager.PlanetDetails[PlanetId].Planet.Resources.Count > 0)
                foreach (var resource in Manager.CurrentManager.PlanetDetails[PlanetId].Planet.Resources)
                    cost += Math.Max(cost, resource.Value * Manager.CurrentManager.ResourceDetails[resource.Key].GetCost(PlanetId))
                          / Manager.CurrentManager.PlanetDetails[PlanetId].Planet.Resources.Count;

            if (Manager.CurrentManager.PlanetDetails[PlanetId].Influence < 0)
                cost += BuildingDetail.GetCost();

            return ToInt(cost + base.CalculateCost(supplier));
        }
    }
}
