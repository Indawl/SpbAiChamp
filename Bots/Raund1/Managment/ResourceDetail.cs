using System;
using System.Linq;
using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Managment
{
    public class ResourceDetail
    {
        public const int SCORE_SCALE = 10000;

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

        public double GetCost(bool invert = false)
        {
            var buildingDetail = Manager.CurrentManager.BuildingDetails[BuildingType];

            var numberIn = invert ? NumberOut : NumberIn;
            var numberOut = invert ? NumberIn : NumberOut;

            double k = invert ? 2.0 : 1.0;
            if (NumberInit == 0)
            {
                if (numberIn == 0) return int.MaxValue;
                if (numberOut == 0) return 0;
                k *= numberIn / numberOut;
            }
            else if (numberIn == 0)
                k /= NumberInit;
            else if (numberOut == 0)
                k *= 0;
            else
                k *= numberIn / (numberOut * NumberInit);

            return k * SCORE_SCALE / buildingDetail.BuildingProperties.ProduceAmount;
        }
    }
}
