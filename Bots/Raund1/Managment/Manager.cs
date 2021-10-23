using System;
using System.Collections.Generic;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Contracts;
using SpbAiChamp.Bots.Raund1.Graphs;

namespace SpbAiChamp.Bots.Raund1.Managment
{
    public class Manager
    {
        #region Static Attributes
        private static Manager[] manager = new Manager[2] { new Manager(), new Manager() };
        public static Manager CurrentManager => manager[0];
        public static Manager LastManager => manager[1];
        public static Manager GetNewManager()
        {
            manager[1] = manager[0];
            return manager[0] = new Manager();
        }
        private static Graph graph = null;
        #endregion

        #region Game's attributes
        public Game Game { get; private set; }
        #endregion

        #region Graph's
        public Graph Graph => graph == null ? graph = CreateGraph() : graph;
        private Graph CreateGraph()
        {
            Graph graph = new Graph();

            // Connect all planet with MaxTravelDistance
            foreach (Planet fromPlanet in Game.Planets)
            {
                List<Edge> edges = new List<Edge>();

                foreach (Planet toPlanet in Game.Planets)
                {
                    int dist = PlanetDetail.Distance(fromPlanet, toPlanet);

                    if (dist <= Game.MaxTravelDistance)
                        edges.Add(new Edge(new Node(toPlanet.Id), dist));
                }

                graph.edges.Add(new Node(fromPlanet.Id), edges);
            }

            return graph;
        }
        #endregion

        #region Game's properties
        public Dictionary<int, Order> Orders { get; } = new Dictionary<int, Order>();
        public Dictionary<int, PlanetDetail> PlanetDetails { get; private set; }
        public Dictionary<Resource, ResourceDetail> ResourceDetails { get; private set; }
        public Dictionary<BuildingType, BuildingDetail> BuildingDetails { get; private set; }

        public int MaxCost { get; private set; } = 10000; // TO DO: fix value
        #endregion        

        public void SetGame(Game game)
        {
            // Game's attribute
            Game = game;

            // Game's properties
            PlanetDetails = new Dictionary<int, PlanetDetail>(Game.Planets.Length);
            foreach (Planet planet in Game.Planets)
                PlanetDetails.Add(planet.Id, new PlanetDetail(planet));

            ResourceDetails = new Dictionary<Resource, ResourceDetail>();
            foreach (Resource resource in Enum.GetValues(typeof(Resource)))
                ResourceDetails.Add(resource, new ResourceDetail(resource));

            BuildingDetails = new Dictionary<BuildingType, BuildingDetail>();
            foreach (var buildingType in Game.BuildingProperties)
                BuildingDetails.Add(buildingType.Key, new BuildingDetail(buildingType.Value));
        }
    }
}
