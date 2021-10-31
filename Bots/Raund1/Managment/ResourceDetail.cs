using System.Linq;
using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Managment
{
    public class ResourceDetail
    {
        public const int SCORE_SCALE = 100;

        public Resource Resource { get; }
        public int Number { get; set; } = 0;

        public double NumberIn { get; set; } = 0;
        public double NumberOut { get; set; } = 0;
        public double NumberInit { get; set; } = 0;

        public BuildingType BuildingType { get; }
        public int Score { get; }

        public ResourceDetail(Resource resource)
        {
            Resource = resource;

            BuildingType = Manager.CurrentManager.Game.BuildingProperties
                .First(_ => _.Value.ProduceResource.HasValue && _.Value.ProduceResource.Value == resource).Key;

            Score = Manager.CurrentManager.BuildingDetails[BuildingType].BuildingProperties.ProduceScore * SCORE_SCALE;
        }

        public double GetCost(int planetId, bool invert = false, bool proportionately = false)
        {
            double k = 1;

            if (proportionately)
            {
                var order = Manager.CurrentManager.Orders[planetId];
                var full = order.Resources.Values.Sum();

                k += (double)order.Resources[Resource] / full;
            }

            if (NumberOut != 0 && NumberIn != 0)
            {
                if (NumberInit != 0) k /= NumberInit;
                if (invert) k *= NumberIn / NumberOut;
                else k *= NumberOut / NumberIn;
            }
            //else if (invert && NumberIn != 0 || !invert && NumberOut != 0) return int.MaxValue;
            //else if (!invert && NumberIn != 0 || invert && NumberOut != 0) return 0;

            var buildingDetail = Manager.CurrentManager.BuildingDetails[BuildingType];
            k *= (double)Score / buildingDetail.BuildingProperties.ProduceAmount;

            return k > int.MaxValue ? int.MaxValue : k;
        }
    }
}
