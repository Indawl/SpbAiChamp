using System;
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
            double k = 1.0;

            if (proportionately)
            {
                if (Manager.CurrentManager.Orders[planetId].Resources.TryGetValue(Resource, out var full))
                    if (Manager.CurrentManager.PlanetDetails[planetId].Planet.Resources.TryGetValue(Resource, out var have))
                    {
                        have = Math.Min(have, full);
                        if (full != have) k *= (double)full / (full - have);
                        else return int.MaxValue;
                    }
            }

            if (NumberIn != 0 || NumberOut != 0)
            {
                if (NumberInit == 0)
                {
                    if (NumberIn == 0) return int.MaxValue;
                    if (NumberOut == 0) return 0;
                    k *= NumberIn / NumberOut;
                } 
                else if (invert) k *= NumberIn / NumberInit;
                else k *= NumberOut / NumberInit;
            }

            var buildingDetail = Manager.CurrentManager.BuildingDetails[BuildingType];
            return k * Score / buildingDetail.BuildingProperties.ProduceAmount;
        }
    }
}
