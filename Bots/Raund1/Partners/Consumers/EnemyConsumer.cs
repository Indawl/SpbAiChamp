using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Managment;
using SpbAiChamp.Bots.Raund1.Partners.Suppliers;

namespace SpbAiChamp.Bots.Raund1.Partners.Consumers
{
    public class EnemyConsumer : Consumer
    {
        public BuildingType? BuildingType { get; set; } = null;

        public EnemyConsumer(int planetId, int number, int delay = 0) : base(planetId, number, null, delay)
        {
            Delay = delay;

            if (Manager.CurrentManager.Orders[planetId].BuildingType.HasValue)
                BuildingType = Manager.CurrentManager.Orders[planetId].BuildingType.Value;
        }

        public override int CalculateCost(Supplier supplier)
        {
            long cost = 0;

            if (Delay > 0 && !BuildingType.HasValue)
            {
                var planetDetail = Manager.CurrentManager.PlanetDetails[PlanetId];
                int dist = planetDetail.ShortestWay.GetRealDistance(supplier.PlanetId);
                if (dist > Delay) return int.MaxValue;
                cost += planetDetail.getTransportCost(Delay - dist);
            }

            foreach (var resource in Manager.CurrentManager.PlanetDetails[PlanetId].Planet.Resources.Keys)
                cost += Manager.CurrentManager.ResourceDetails[resource].GetCost(PlanetId);

            return cost > int.MaxValue ? int.MaxValue : (int)cost;
        }
    }
}
