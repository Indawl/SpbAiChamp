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

        public static double GetCost(BuildingType? buildingType = null, bool invert = false)
        {
            var buildingDetail = Manager.CurrentManager.BuildingDetails[buildingType.HasValue ? buildingType.Value : BuildingType.Replicator];

            if (buildingDetail.BuildingProperties.ProduceResource.HasValue)
                return Manager.CurrentManager.ResourceDetails[buildingDetail.BuildingProperties.ProduceResource.Value].GetCost(invert);
            else return ResourceDetail.SCORE_SCALE;
        }
    }
}
