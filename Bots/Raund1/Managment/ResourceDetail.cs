using System.Linq;
using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Managment
{
    public class ResourceDetail
    {
        public Resource Resource { get; }
        public int Number { get; } = 0;

        public int NumberIn { get; set; } = 0;
        public int NumberOut { get; set; } = 0;
        public int NumberInit { get; set; } = 0;

        public BuildingType BuildingType { get; }

        public ResourceDetail(Resource resource)
        {
            Resource = resource;

            BuildingType = Manager.CurrentManager.Game.BuildingProperties
                .First(_ => _.Value.ProduceResource.HasValue && _.Value.ProduceResource.Value == resource).Key;

            Number = Manager.CurrentManager.Game.Planets.Sum(_ => _.Resources.ContainsKey(Resource) ? _.Resources[Resource] : 0);
        }

        public int GetCost(int planetId, bool proportionately = true)
        {
            var buildingDetail = Manager.CurrentManager.BuildingDetails[BuildingType];

            if (NumberOut == 0) return 0;
            if (NumberInit == 0) return buildingDetail.BuildingProperties.ProduceScore;

            var order = Manager.CurrentManager.Orders[planetId];
            var full = order.Resources.Values.Sum();
            double k = 2;
            if (proportionately && order.Resources.ContainsKey(Resource)) k += order.Resources[Resource] / full;

            return (int)(NumberIn * buildingDetail.BuildingProperties.ProduceScore * k /
                        (NumberOut * buildingDetail.BuildingProperties.ProduceAmount * NumberInit));
        }
    }
}
