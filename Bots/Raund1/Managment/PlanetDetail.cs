using System;
using System.Linq;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Logistics;

namespace SpbAiChamp.Bots.Raund1.Managment
{
    public class PlanetDetail
    {
        private ShortestWay shortestWay = null;

        public Planet Planet { get; }
        public int WorkerCount { get; } = 0;
        public int Influence { get; set; } = 0;

        public ShortestWay ShortestWay => shortestWay == null ? shortestWay = new ShortestWay(Planet) : shortestWay;
        public static int Distance(Planet from, Planet to) => Math.Abs(to.X - from.X) + Math.Abs(to.Y - from.Y);

        public PlanetDetail(Planet planet)
        {
            Planet = planet;

            WorkerCount = planet.WorkerGroups.Sum(group => group.PlayerIndex == Manager.CurrentManager.Game.MyIndex ? group.Number : -group.Number);
        }

        public int getTransportCost(int planetId, int delay) => getTransportCost(ShortestWay.GetRealDistance(planetId) + delay);
        public int getTransportCost(int dist) => (int)(Manager.CurrentManager.TransportTax * dist);
        //public int getTransportCost(int dist) => (int)(Manager.CurrentManager.TransportTax * dist * (Influence >= 0 ? 0 : 1));
    }
}
