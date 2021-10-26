using System.Collections.Generic;
using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Contracts
{
    public class Order
    {
        public int PlanetId { get; }
        public int TickStart { get; }
        public int TickEnd { get; set; }

        public Dictionary<Resource, int> Resources { get; set; } = new Dictionary<Resource, int>();
        public int Number { get; set; }

        public BuildingType? BuildingType { get; set; }

        public int Delay { get; set; }

        public Order(int planetId, int tickStart = 0, int tickEnd = 1000, int delay = 0)
        {
            PlanetId = planetId;
            TickStart = tickStart;
            TickEnd = tickEnd;

            Delay = delay;
        }
    }
}
