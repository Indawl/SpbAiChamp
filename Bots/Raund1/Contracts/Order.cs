using System.Collections.Generic;
using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Contracts
{
    public class Order
    {
        public int PlanetId { get; }
        public int TickStart { get; }
        public int TickEnd { get; set; }

        public LinkedList<OrderItems> OrderItems { get; } = new LinkedList<OrderItems>();

        public Order(int planetId, int tickStart = 0, int tickEnd = 1000)
        {
            PlanetId = planetId;
            TickStart = tickStart;
            TickEnd = tickEnd;
        }

        public void AddItem(Dictionary<Resource, int> resources, BuildingType? buildingType)
        {
            OrderItems.AddLast(new OrderItems(resources, buildingType));
        }
    }
}
