using System.Collections.Generic;
using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Contracts
{
    public class OrderItems
    {
        public Dictionary<Resource, int> Resources { get; set; }
        public BuildingType? BuildingType { get; set; }

        public bool CanDummy { get; set; }

        public OrderItems(Dictionary<Resource, int> resources, bool canDummy = true, BuildingType? buildingType = null)
        {
            Resources = resources;
            CanDummy = canDummy;
            BuildingType = buildingType;
        }
    }
}
