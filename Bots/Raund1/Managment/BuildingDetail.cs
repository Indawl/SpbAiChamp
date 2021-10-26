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
    }
}
