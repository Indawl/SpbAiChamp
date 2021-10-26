using System;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Graphs;
using SpbAiChamp.Bots.Raund1.Managment;

namespace SpbAiChamp.Bots.Raund1.Logistics
{
    public class ShortestWay : Dijkstra
    {
        public ShortestWay(Planet planet) : base(Manager.CurrentManager.Graph, planet.Id) { }

        public int GetDistance(int planetId) => costAsFar[new Node(planetId)];
        public int GetNextPlanet(int planetId) => GetNextNode(new Node(planetId)).id;
        public int GetNextPlanetInv(int planetId) => cameFrom[new Node(planetId)].id;

        protected override int GetCost(Edge edge) => base.GetCost(edge) + Math.Max(0, Manager.CurrentManager.PlanetDetails[edge.toNode.id].Influence);
    }
}