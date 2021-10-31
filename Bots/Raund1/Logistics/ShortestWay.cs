using System;
using System.Collections.Generic;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Graphs;
using SpbAiChamp.Bots.Raund1.Managment;

namespace SpbAiChamp.Bots.Raund1.Logistics
{
    public class ShortestWay : Dijkstra
    {
#if MYDEBUG
        public static int CountCall = 0;
#endif
        public Dictionary<int, int> Distances { get; set; } = new Dictionary<int, int>();

        public ShortestWay(Planet planet) : base(Manager.CurrentManager.Graph, planet.Id)
        {
#if MYDEBUG
            CountCall++;
#endif
        }

        protected override int GetCost(Edge edge) => base.GetCost(edge) + Math.Max(0, -Math.Sign(Manager.CurrentManager.PlanetDetails[edge.toNode.id].Influence));

        public int GetDistance(int planetId) => costAsFar[new Node(planetId)];
        public int GetNextPlanet(int planetId) => GetNextNode(new Node(planetId)).id;
        public int GetNextPlanetInv(int planetId) => cameFrom[new Node(planetId)].id;
        public int GetRealDistance(int planetId)
        {
            if (!Distances.ContainsKey(planetId))
            {
                Distances[planetId] = 0;

                var node = new Node(planetId);

                while (cameFrom[node] != null)
                {
                    Distances[planetId] += costAsFar[node];
                    node = cameFrom[node];
                }
            }

            return Distances[planetId];
        }
    }
}