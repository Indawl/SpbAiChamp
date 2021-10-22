using System.Collections.Generic;
using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Contracts
{
    public class OrderItems
    {
        public Dictionary<Resource, int> Resources { get; set; }
        public BuildingType? BuildingType { get; set; }

        public OrderItems(Dictionary<Resource, int> resources, BuildingType? buildingType)
        {
            Resources = resources;
            BuildingType = buildingType;
        }
    }
}
