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

        public static double GetCost(int planetId = 0, BuildingType? buildingType = null, bool invert = false, bool proportionately = false)
        {
            var buildingDetail = Manager.CurrentManager.BuildingDetails[buildingType.HasValue ? buildingType.Value : BuildingType.Replicator];

            if (buildingDetail.BuildingProperties.ProduceResource.HasValue)
                return Manager.CurrentManager.ResourceDetails[buildingDetail.BuildingProperties.ProduceResource.Value].GetCost(planetId, invert, proportionately);
            else return buildingDetail.BuildingProperties.ProduceScore * ResourceDetail.SCORE_SCALE;
        }
    }
}
