using System;
using System.Collections.Generic;
using System.Linq;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Graphs;
using SpbAiChamp.Bots.Raund1.Contracts;
using SpbAiChamp.Bots.Raund1.Partners.Suppliers;
using SpbAiChamp.Bots.Raund1.Partners.Consumers;
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
        public static bool IsRefreshBrunchBuildings { get; set; } = true;
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
        private void DivisionTerritory()
        {
            // Initialize
            foreach (var node in Graph.edges.Keys)
                PlanetDetails[node.id].Influence = 0;

            // Spread of influence
            foreach (var node in Graph.edges)
            {
                int sign = Math.Sign(PlanetDetails[node.Key.id].WorkerCount) * 2 * Game.MaxTravelDistance;
                if (sign == 0) continue;

                PlanetDetails[node.Key.id].Influence += sign;
                foreach (var edge in node.Value)
                    PlanetDetails[edge.toNode.id].Influence += sign - edge.cost;
            }
        }
        #endregion

        #region Game's properties
        public Dictionary<int, Order> Orders { get; } = new Dictionary<int, Order>();
        public Dictionary<int, PlanetDetail> PlanetDetails { get; private set; }
        public Dictionary<Resource, ResourceDetail> ResourceDetails { get; private set; }
        public Dictionary<BuildingType, BuildingDetail> BuildingDetails { get; private set; }
        public Dictionary<int, PlanetDetail> MyPlanets { get; set; }
        public PlanetDetail CapitalPlanetId { get; set; }

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

                if (planet.Building.HasValue)
                    BuildingDetails[planet.Building.Value.BuildingType].Planets.Add(planet.Id);

                if (!Orders.ContainsKey(planet.Id))
                    Orders[planet.Id] = new Order(planet.Id, Game.CurrentTick, Game.CurrentTick);
            }

            // Division territory
            DivisionTerritory();

            // Get my planets
            MyPlanets = new Dictionary<int, PlanetDetail>();
            foreach (var planetDetail in PlanetDetails.Values.Where(_ => _.Influence >= 0))
                MyPlanets.Add(planetDetail.Planet.Id, planetDetail);

            // Get Capital planet
            GetCapitalPlanet();
        }

        private void GetCapitalPlanet()
        {
            int x = MyPlanets.Values.Sum(_ => _.Planet.X) / PlanetDetails.Count;
            int y = MyPlanets.Values.Sum(_ => _.Planet.Y) / PlanetDetails.Count;
            int minDist = int.MaxValue;

            CapitalPlanetId = PlanetDetails.Values.First();
            foreach (var planetDetail in MyPlanets.Values)
            {
                int dist = PlanetDetail.Distance(CapitalPlanetId.Planet, planetDetail.Planet);
                if (dist < minDist)
                {
                    minDist = dist;
                    CapitalPlanetId = planetDetail;
                }
            }
        }

        public void ProcessOrder()
        {
            foreach (var planetDetail in PlanetDetails.Values.Where(_ => Orders[_.Planet.Id].TickEnd <= Game.CurrentTick))
                Orders[planetDetail.Planet.Id] = new Order(planetDetail.Planet.Id, Game.CurrentTick, Game.CurrentTick);

            // Building brunch
            if (IsRefreshBrunchBuildings) //TO DO: determ value refresh
                RefreshBrunchBuildings();

            // Create orders for factory
            foreach (var planetDetail in PlanetDetails.Values)
            {
                var order = Orders[planetDetail.Planet.Id];
                var buildingType = (order.BuildingType.HasValue && IsRefreshBrunchBuildings) ? order.BuildingType : planetDetail.Planet.Building?.BuildingType ?? null;

                // Building needs
                if (buildingType.HasValue)
                {
                    var buildingDetail = BuildingDetails[buildingType.Value];

                    // For work in factory (in exist building)
                    if (!order.BuildingType.HasValue)
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
                            int number = stock + DurationQuarter * resource.Value;

                            if (order.Resources.ContainsKey(resource.Key))
                                order.Resources[resource.Key] += number;
                            else order.Resources.Add(resource.Key, number);

                            order.TickEnd += DurationQuarter;
                        } // here no planet!!!
                        else if (resourceDetail.Number > 0 || stock > 0)
                            order.Resources.Add(resource.Key, stock + resourceDetail.Number);
                    }
                }
            }
        }

        private void RefreshBrunchBuildings()
        {
            foreach (var order in Orders.Values.Where(_ => _.BuildingType.HasValue))
                Orders[order.PlanetId] = new Order(order.PlanetId, Game.CurrentTick, Game.CurrentTick);

            foreach (BuildingType buildingType in Enum.GetValues(typeof(BuildingType)))
                if (BuildingDetails.ContainsKey(buildingType))
                {
                    var planetId = GetPlanetForBuilding(buildingType);
                    if (planetId.HasValue)
                    {
                        Orders[planetId.Value].BuildingType = buildingType;
                        Orders[planetId.Value].Resources = new Dictionary<Resource, int>(BuildingDetails[buildingType].BuildingProperties.BuildResources);
                        Orders[planetId.Value].Number = Math.Max(0, BuildingDetails[buildingType].BuildingProperties.MaxWorkers - BuildingDetails[buildingType].BuildingProperties.BuildResources.Values.Sum());
                        Orders[planetId.Value].TickEnd = Game.MaxTickCount;
                    }
                }

            IsRefreshBrunchBuildings = false;
        }

        private int? GetPlanetForBuilding(BuildingType buildingType)
        {
            int? planetId = null;
            int minDist = int.MaxValue;

            var buildingDetail = BuildingDetails[buildingType];

            foreach (var planetDetail in PlanetDetails.Values.Where(_ => MyPlanets.ContainsKey(_.Planet.Id) && !_.Planet.Building.HasValue))
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
