using System;
using System.Collections.Generic;
using System.Linq;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Contracts;
using SpbAiChamp.Bots.Raund1.Graphs;
using SpbAiChamp.Bots.Raund1.Partners;

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

        public void ProcessOrder()
        {
            throw new NotImplementedException();
        }

        public void GetPartners(out List<Supplier> suppliers, out List<Consumer> consumers)
        {
            suppliers = new List<Supplier>();
            consumers = new List<Consumer>();

            // Analyze all planet
            foreach (var planetDetail in PlanetDetails.Values)
                GetPartners(suppliers, consumers, planetDetail);

            // Analyze all flying group
            foreach (var flyingWorkerGroups in Game.FlyingWorkerGroups)
                GetPartners(suppliers, consumers, flyingWorkerGroups);
        }

        private void GetPartners(List<Supplier> suppliers, List<Consumer> consumers, PlanetDetail planetDetail)
        {
            // Add workers
            if (planetDetail.WorkerCount > 0)
                suppliers.Add(new LaborSupplier(planetDetail.Planet.Id, planetDetail.WorkerCount));
            else if (planetDetail.WorkerCount < 0)
                consumers.Add(new EnemyConsumer(planetDetail.Planet.Id, -planetDetail.WorkerCount));

            // Add resources
            foreach (var resource in planetDetail.Planet.Resources)
                GetPartners(suppliers, consumers, planetDetail.Planet.Id, resource.Key, resource.Value);

            // Add needs
            var order = Orders[planetDetail.Planet.Id];

            if (order.Number > 0)
                consumers.Add(new LaborConsumer(order.PlanetId, order.Number, order.Delay));

            foreach (var resource in Orders[planetDetail.Planet.Id].Resources)
                consumers.Add(new ResourceConsumer(order.PlanetId, resource.Value, resource.Key, order.Delay));

            if (order.BuildingType.HasValue)
                consumers.Add(new BuildingConsumer(order.PlanetId, order.BuildingType.Value));
        }

        private void GetPartners(List<Supplier> suppliers, List<Consumer> consumers, int planetId, Resource resource, int number, int delay = 0)
        {
            var warehouseSupplier = new WarehouseSupplier(planetId, number, resource, delay);
            suppliers.Add(warehouseSupplier);
            consumers.Add(new LaborConsumer(planetId, number, delay, warehouseSupplier));
        }

        private void GetPartners(List<Supplier> suppliers, List<Consumer> consumers, FlyingWorkerGroup flyingWorkerGroups)
        {
            // Add resource with delay
            if (flyingWorkerGroups.Resource.HasValue)
                GetPartners(suppliers, consumers, flyingWorkerGroups.NextPlanet, flyingWorkerGroups.Resource.Value, flyingWorkerGroups.Number,
                                                  flyingWorkerGroups.NextPlanetArrivalTick - Game.CurrentTick);

            // Add workers with delay
            if (flyingWorkerGroups.PlayerIndex == Game.MyIndex)
                suppliers.Add(new LaborSupplier(flyingWorkerGroups.NextPlanet, flyingWorkerGroups.Number, flyingWorkerGroups.NextPlanetArrivalTick - Game.CurrentTick));
            else
                consumers.Add(new EnemyConsumer(flyingWorkerGroups.NextPlanet, flyingWorkerGroups.Number, flyingWorkerGroups.NextPlanetArrivalTick - Game.CurrentTick));
        }
    }
}
