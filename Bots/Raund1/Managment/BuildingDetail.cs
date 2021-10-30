using System.Collections.Generic;
using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Managment
{
    public class BuildingDetail
    {
        public BuildingProperties BuildingProperties { get; }

        public List<int> Planets { get; } = new List<int>();

        public BuildingDetail(BuildingProperties buildingProperties)
        {
            BuildingProperties = buildingProperties;
        }

        public double GetCost(int planetId, BuildingType buildingType)
        {
            var buildingDetail = Manager.CurrentManager.BuildingDetails[buildingType];

            if (buildingDetail.BuildingProperties.ProduceResource.HasValue)
                return Manager.CurrentManager.ResourceDetails[buildingDetail.BuildingProperties.ProduceResource.Value].GetCost(planetId);
            else return Manager.CurrentManager.BuildingDetails[BuildingType.Replicator].BuildingProperties.ProduceScore * ResourceDetail.SCORE_SCALE;
        }
    }
}
