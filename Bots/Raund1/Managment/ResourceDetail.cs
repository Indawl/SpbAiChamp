using System.Linq;
using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Managment
{
    public class ResourceDetail
    {
        public const int SCORE_SCALE = 10;

        public Resource Resource { get; }
        public int Number { get; set; } = 0;

        public int NumberIn { get; set; } = 0;
        public int NumberOut { get; set; } = 0;
        public int NumberInit { get; set; } = 0;

        public BuildingType BuildingType { get; }
        public int Score { get; }

        public ResourceDetail(Resource resource)
        {
            Resource = resource;

            BuildingType = Manager.CurrentManager.Game.BuildingProperties
                .First(_ => _.Value.ProduceResource.HasValue && _.Value.ProduceResource.Value == resource).Key;

            Score = Manager.CurrentManager.BuildingDetails[BuildingType].BuildingProperties.ProduceScore * SCORE_SCALE;
        }

        public int GetCost(int planetId, bool proportionately = false)
        {
            double k = 1;

             if (proportionately)
            {
                var order = Manager.CurrentManager.Orders[planetId];
                var full = order.Resources.Values.Sum();

                k += (double)order.Resources[Resource] / full;
            }

            if (NumberInit == 0) // Stone
            {
                if (NumberOut != 0) k = (double)NumberIn / NumberOut;
            }
            else
            {
                if (NumberInit != 0) k /= NumberInit;
                if (NumberOut != 0) k *= (double)NumberIn / NumberOut;
                else if (NumberIn != 0) return int.MaxValue;
            }

            var buildingDetail = Manager.CurrentManager.BuildingDetails[BuildingType];

            return (int)(k * Score / buildingDetail.BuildingProperties.ProduceAmount);
        }
    }
}
