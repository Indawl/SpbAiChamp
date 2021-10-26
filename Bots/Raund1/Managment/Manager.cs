using System;
using System.Collections.Generic;
using System.Linq;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Contracts;
using SpbAiChamp.Bots.Raund1.Graphs;
using SpbAiChamp.Bots.Raund1.Partners;
using SpbAiChamp.Bots.Raund1.Logistics;

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
        public int DurationQuarter { get; private set; } = 10;
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

        public TransportTask TransportTask { get; set; }
        #endregion

        public void SetGame(Game game)
        {
            // Game's attribute
            Game = game;

            // Game's properties
            BuildingDetails = new Dictionary<BuildingType, BuildingDetail>();
            foreach (var buildingType in Game.BuildingProperties)
                BuildingDetails.Add(buildingType.Key, new BuildingDetail(buildingType.Value));

            ResourceDetails = new Dictionary<Resource, ResourceDetail>();
            foreach (Resource resource in Enum.GetValues(typeof(Resource)))
                ResourceDetails.Add(resource, new ResourceDetail(resource));

            PlanetDetails = new Dictionary<int, PlanetDetail>(Game.Planets.Length);
            foreach (Planet planet in Game.Planets)
            {
                var planetDetail = new PlanetDetail(planet);
                PlanetDetails.Add(planet.Id, planetDetail);

                if (planet.Building.HasValue && planetDetail.WorkerCount > 0)
                    BuildingDetails[planet.Building.Value.BuildingType].Planets.Add(planet.Id);

                if (!Orders.ContainsKey(planet.Id))
                    Orders[planet.Id] = new Order(planet.Id, Game.CurrentTick, Game.CurrentTick);
            }
        }

        public void ProcessOrder()
        {
            foreach (var order in Orders.Where(_=>_.Value.TickEnd <= Game.CurrentTick))
                Orders[order.Key] = new Order(order.Key, Game.CurrentTick, Game.CurrentTick);

            // Building brunch
            foreach (BuildingType buildingType in Enum.GetValues(typeof(BuildingType)))
                if (BuildingDetails.ContainsKey(buildingType))
                {
                    var planetId = GetPlanetForBuilding(buildingType);
                    if (planetId.HasValue)
                    {
                        Orders[planetId.Value].BuildingType = buildingType;
                        Orders[planetId.Value].Resources = new Dictionary<Resource, int>(BuildingDetails[buildingType].BuildingProperties.BuildResources);
                        Orders[planetId.Value].Number = Math.Max(0, BuildingDetails[buildingType].BuildingProperties.MaxWorkers - BuildingDetails[buildingType].BuildingProperties.BuildResources.Values.Sum());
                    }
                }

            // Create orders for factory
            foreach (var planetDetail in PlanetDetails.Values)
            {
                if (Orders[planetDetail.Planet.Id].TickEnd <= Game.CurrentTick)
                {
                    var order = Orders[planetDetail.Planet.Id];
                    var buildingType = order.BuildingType.HasValue ? order.BuildingType : planetDetail.Planet.Building?.BuildingType ?? null;

                    // Building needs
                    if (buildingType.HasValue)
                    {
                        var buildingDetail = BuildingDetails[buildingType.Value];

                        // For work in factory
                        order.Number = buildingDetail.BuildingProperties.MaxWorkers;

                        // For resources
                        foreach (var resource in buildingDetail.BuildingProperties.WorkResources)
                        {
                            var resourceDetail = ResourceDetails[resource.Key];
                            var buildingDetails = BuildingDetails[resourceDetail.BuildingType];

                            // Get stock
                            planetDetail.Planet.Resources.TryGetValue(resource.Key, out var stock);

                            // No planets, try min, maybe we have resources
                            if (buildingDetails.Planets.Count > 0)
                            {
                                int duration = DurationQuarter * buildingDetails.Planets.Min(_ => planetDetail.ShortestWay.GetDistance(_));
                                // get max, after min duration tickend recreate order
                                order.Resources.Add(resource.Key, stock + duration * buildingDetails.Planets.Count * resource.Value *
                                                                  buildingDetails.BuildingProperties.MaxWorkers / buildingDetails.BuildingProperties.ProduceAmount);
                                order.TickEnd += order.BuildingType.HasValue ? 0 : duration;
                            } // here no planet!!!
                            else if (resourceDetail.Number > 0 || stock > 0)
                                order.Resources.Add(resource.Key, stock + resourceDetail.Number);
                        }
                    }
                }
            }
        }

        private int? GetPlanetForBuilding(BuildingType buildingType)
        {
            int? planetId = null;
            int minDist = int.MaxValue;

            var buildingDetail = BuildingDetails[buildingType];

            foreach (var planetDetail in PlanetDetails.Values.Where(_ => !_.Planet.Building.HasValue))
            {
                // If harvest, then only harvest
                if (buildingDetail.BuildingProperties.Harvest)
                {
                    if (!planetDetail.Planet.HarvestableResource.HasValue) continue;
                    if (planetDetail.Planet.HarvestableResource.Value != buildingDetail.BuildingProperties.ProduceResource.Value) continue;
                }
                else if (planetDetail.Planet.HarvestableResource.HasValue) continue;

                int dist = 0;

                // From
                var resources = buildingDetail.BuildingProperties.WorkResources.Count == 0
                              ? buildingDetail.BuildingProperties.BuildResources
                              : buildingDetail.BuildingProperties.WorkResources;
                foreach (var resource in resources.Keys)
                    if (BuildingDetails[ResourceDetails[resource].BuildingType].Planets.Count > 0)
                        dist += BuildingDetails[ResourceDetails[resource].BuildingType].Planets
                            .Min(_ => PlanetDetails[_].ShortestWay.GetDistance(planetDetail.Planet.Id));

                // To
                if (buildingDetail.BuildingProperties.ProduceResource.HasValue)
                    if (BuildingDetails[ResourceDetails[buildingDetail.BuildingProperties.ProduceResource.Value].BuildingType].Planets.Count > 0)
                        dist += BuildingDetails[ResourceDetails[buildingDetail.BuildingProperties.ProduceResource.Value].BuildingType].Planets
                            .Min(_ => PlanetDetails[_].ShortestWay.GetDistance(planetDetail.Planet.Id));

                if (dist < minDist)
                {
                    planetId = planetDetail.Planet.Id;
                    minDist = dist;
                }
            }

            if (minDist == 0) return null;
            return planetId;
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

            // Add needs from order
            var order = Orders[planetDetail.Planet.Id];
            if (order.Number > 0)
                consumers.Add(new LaborConsumer(order.PlanetId, order.Number, order.Delay));

            foreach (var resource in Orders[planetDetail.Planet.Id].Resources)
                if (order.BuildingType.HasValue)
                    consumers.Add(new BuildingConsumer(order.PlanetId, order.BuildingType.Value, resource.Key, resource.Value));
                else
                    consumers.Add(new ResourceConsumer(order.PlanetId, resource.Value, resource.Key, order.Delay));
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
