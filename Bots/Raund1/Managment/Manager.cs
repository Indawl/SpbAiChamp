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
        public static Manager CurrentManager { get; } = new Manager();
        #endregion

        #region Game's attributes
        public Game Game { get; private set; }
        #endregion

        #region Graph's
        private Graph graph = null;
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
                    PlanetDetails[edge.toNode.id].Influence += sign - 2 * edge.cost + 1;
            }
        }
        #endregion

        #region Game's properties
        public Dictionary<int, Order> Orders { get; } = new Dictionary<int, Order>();
        public Dictionary<int, PlanetDetail> PlanetDetails { get; private set; }
        public Dictionary<Resource, ResourceDetail> ResourceDetails { get; private set; }
        public Dictionary<BuildingType, BuildingDetail> BuildingDetails { get; private set; }
        public Dictionary<int, PlanetDetail> MyPlanets { get; set; }
        public PlanetDetail CapitalPlanet { get; set; }
        public double TransportTax { get; set; } = 1.0;

        public Dictionary<Resource, TransportTask> TransportTasks { get; set; }
        public TransportTask TransportTaskWorker { get; set; }
        public TransportTask TransportTask(Resource? resource) => resource.HasValue ? TransportTasks[resource.Value] : TransportTaskWorker;
        #endregion

        public Manager GetNewManager() => CurrentManager;

        public void SetGame(Game game)
        {
            // Game's attribute
            Game = game;
            TransportTasks = new Dictionary<Resource, TransportTask>();

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

                if (!Orders.ContainsKey(planet.Id))
                    Orders[planet.Id] = new Order(planet.Id);
            }

            // Division territory
            DivisionTerritory();

            foreach (var planetDetail in PlanetDetails.Values)
                if (planetDetail.Planet.Building.HasValue && planetDetail.Influence >= 0)
                    BuildingDetails[planetDetail.Planet.Building.Value.BuildingType].Planets.Add(planetDetail.Planet.Id);

            // Get my planets
            MyPlanets = new Dictionary<int, PlanetDetail>();
            foreach (var planetDetail in PlanetDetails.Values.Where(_ => _.Influence >= 0))
            {
                MyPlanets.Add(planetDetail.Planet.Id, planetDetail);

                // Get supply and demand
                if (planetDetail.Planet.Building.HasValue)
                {
                    var buildingDetail = BuildingDetails[planetDetail.Planet.Building.Value.BuildingType];

                    planetDetail.TaskCount = Math.Min(buildingDetail.BuildingProperties.MaxWorkers, planetDetail.WorkerCount)
                                           / buildingDetail.BuildingProperties.WorkAmount;

                    foreach (var resource in buildingDetail.BuildingProperties.WorkResources)
                        if (planetDetail.TaskCount > 0) planetDetail.TaskCount = Math.Min(planetDetail.TaskCount, resource.Value / planetDetail.TaskCount);

                    if (planetDetail.TaskCount > 0)
                        if (buildingDetail.BuildingProperties.WorkResources.Count > 0)
                            foreach (var resource in buildingDetail.BuildingProperties.WorkResources)
                            {
                                ResourceDetails[resource.Key].NumberIn += resource.Value * planetDetail.TaskCount;
                                if (buildingDetail.BuildingProperties.ProduceResource.HasValue)
                                    ResourceDetails[buildingDetail.BuildingProperties.ProduceResource.Value].NumberOut += buildingDetail.BuildingProperties.ProduceAmount * planetDetail.TaskCount;
                            }
                        else
                            ResourceDetails[buildingDetail.BuildingProperties.ProduceResource.Value].NumberOut += buildingDetail.BuildingProperties.ProduceAmount * planetDetail.TaskCount;

                    planetDetail.Efficiency = (double)planetDetail.TaskCount * buildingDetail.BuildingProperties.WorkAmount / buildingDetail.BuildingProperties.MaxWorkers;
                }
            }

            foreach (var resourceDetail in ResourceDetails.Values)
                resourceDetail.Number = PlanetDetails.Values.Sum(_ => (_.Influence >= 0 &&
                    _.Planet.Resources.ContainsKey(resourceDetail.Resource)) ? _.Planet.Resources[resourceDetail.Resource] : 0);

            foreach (var flyingWorkerGroups in Game.FlyingWorkerGroups.Where(_ => _.PlayerIndex == Game.MyIndex && _.Resource.HasValue))
                ResourceDetails[flyingWorkerGroups.Resource.Value].Number += flyingWorkerGroups.Number;

            // Get initial supply and demand
            GetInitialNumber(BuildingType.Replicator);

            // Get Capital planet
            GetCapitalPlanet();

            // Transport Tax
            var flyingCount = Game.FlyingWorkerGroups.Count(_ => _.PlayerIndex == Game.MyIndex);
            if (flyingCount == Game.MaxFlyingWorkerGroups) TransportTax = Game.MaxFlyingWorkerGroups;
            else TransportTax = (double)Game.MaxFlyingWorkerGroups / (Game.MaxFlyingWorkerGroups - flyingCount);
        }

        private void GetInitialNumber(BuildingType buildingType, int amount = 1)
        {
            var buildingProperties = BuildingDetails[buildingType].BuildingProperties;

            if (buildingProperties.ProduceResource.HasValue)
                ResourceDetails[buildingProperties.ProduceResource.Value].NumberInit += amount;

            foreach (var workResource in buildingProperties.WorkResources)
                for (int i = 0; i < amount; i++)
                    GetInitialNumber(ResourceDetails[workResource.Key].BuildingType, workResource.Value);            
        }

        private void GetCapitalPlanet()
        {
            int x = (int)MyPlanets.Values.Average(_ => _.Planet.X);
            int y = (int)MyPlanets.Values.Average(_ => _.Planet.Y);
            int minDist = int.MaxValue;

            CapitalPlanet = PlanetDetails.Values.First();
            foreach (var planetDetail in MyPlanets.Values)
            {
                int dist = PlanetDetail.Distance(CapitalPlanet.Planet, planetDetail.Planet);
                if (dist < minDist)
                {
                    minDist = dist;
                    CapitalPlanet = planetDetail;
                }
            }
        }

        public void ProcessOrder()
        {
            UpdateStateOrders();

            // Refresh
            RefreshOrders();

            // Add In for building resource
            foreach (var order in Orders.Values.Where(_ => _.BuildingType.HasValue))
                ResourceDetails[Resource.Stone].NumberIn += BuildingDetails[order.BuildingType.Value].BuildingProperties.BuildResources[Resource.Stone];

            // Create orders for factory
            foreach (var planetDetail in PlanetDetails.Values.Where(_ => _.Influence >= 0))
            {
                if (Orders[planetDetail.Planet.Id].TickStart == Game.CurrentTick)
                    Orders[planetDetail.Planet.Id].CreateResourceOrder(planetDetail);

                // Subtract that we have for building
                if (planetDetail.Planet.Resources.TryGetValue(Resource.Stone, out var value))
                    ResourceDetails[Resource.Stone].NumberIn = Math.Max(0, ResourceDetails[Resource.Stone].NumberIn - value);
            }
        }

        private void UpdateStateOrders()
        {
            foreach (var planetDetail in PlanetDetails.Values.Where(_ => Orders[_.Planet.Id].TickEnd <= Game.CurrentTick))
                Orders[planetDetail.Planet.Id] = new Order(planetDetail.Planet.Id);
        }

        private void RefreshOrders()
        {
            foreach (var order in Orders.Values.Where(_ => _.BuildingType.HasValue))
                Orders[order.PlanetId] = new Order(order.PlanetId);

            foreach (BuildingType buildingType in Enum.GetValues(typeof(BuildingType)))
                if (BuildingDetails.ContainsKey(buildingType))
                {
                    var planetId = GetPlanetForBuilding(buildingType);
                    if (planetId.HasValue) Orders[planetId.Value].CreateBuildingOrder(buildingType);
                }
        }

        private int? GetPlanetForBuilding(BuildingType buildingType)
        {
            int? planetId = null;
            int minDist = int.MaxValue;

            var buildingDetail = BuildingDetails[buildingType];

            foreach (var planetDetail in PlanetDetails.Values
                .Where(_ => !_.Planet.Building.HasValue && MyPlanets.ContainsKey(_.Planet.Id) && !Orders[_.Planet.Id].BuildingType.HasValue))
            {
                if (planetDetail.Planet.Building.HasValue && planetDetail.Planet.Building.Value.BuildingType == buildingType &&
                    planetDetail.Planet.Building.Value.Health < BuildingDetails[planetDetail.Planet.Building.Value.BuildingType].BuildingProperties.MaxHealth)
                    return planetDetail.Planet.Id;

                if (buildingDetail.Planets.Exists(_ => PlanetDetails[_].Efficiency < 0.5)) continue;

                // If harvest, then only harvest
                if (buildingDetail.BuildingProperties.Harvest)
                {
                    if (!planetDetail.Planet.HarvestableResource.HasValue) continue;
                    if (planetDetail.Planet.HarvestableResource.Value != buildingDetail.BuildingProperties.ProduceResource.Value) continue;
                }
                else if (planetDetail.Planet.HarvestableResource.HasValue) continue;

                // From
                List<int> planetFrom = new List<int>();
                var resources = buildingDetail.BuildingProperties.WorkResources.Count == 0
                              ? buildingDetail.BuildingProperties.BuildResources
                              : buildingDetail.BuildingProperties.WorkResources;

                foreach (var resource in resources.Keys)
                    if (BuildingDetails[ResourceDetails[resource].BuildingType].Planets.Count > 0)
                        planetFrom.AddRange(BuildingDetails[ResourceDetails[resource].BuildingType].Planets);

                // To
                List<int> planetTo = new List<int>();

                if (buildingDetail.BuildingProperties.ProduceResource.HasValue)
                    if (BuildingDetails[ResourceDetails[buildingDetail.BuildingProperties.ProduceResource.Value].BuildingType].Planets.Count > 0)
                        planetTo.AddRange(BuildingDetails[ResourceDetails[buildingDetail.BuildingProperties.ProduceResource.Value].BuildingType].Planets);

                // Find min
                if (planetFrom.Count + planetTo.Count == 0)
                    planetFrom.Add(CapitalPlanet.Planet.Id);

                int dist = 0;
                if (planetFrom.Count > 0) dist += planetFrom.Min(_ => PlanetDetails[_].ShortestWay.GetDistance(planetDetail.Planet.Id));
                if (planetTo.Count > 0) dist += planetTo.Min(_ => PlanetDetails[_].ShortestWay.GetDistance(planetDetail.Planet.Id));

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
            var order = Orders[planetDetail.Planet.Id];

            // Add workers
            if (planetDetail.WorkerCount > 0)
                suppliers.Add(new LaborSupplier(planetDetail.Planet.Id, planetDetail.WorkerCount));
            else if (planetDetail.WorkerCount < 0)
                consumers.Add(new EnemyConsumer(planetDetail.Planet.Id, -planetDetail.WorkerCount));

            // Add resources
            if (planetDetail.Influence >= 0)
                foreach (var resource in planetDetail.Planet.Resources)
                {
                    int value = resource.Value;

                    if (order.Resources.ContainsKey(resource.Key) && resource.Key != Resource.Stone)
                        value -= order.Resources[resource.Key];

                    if (value > 0) suppliers.Add(new WarehouseSupplier(planetDetail.Planet.Id, value, resource.Key));
                }

            // Add needs from order
            if (order.Number > 0)
                consumers.Add(new LaborConsumer(order.PlanetId, order.Number, order.Delay));

            foreach (var resource in Orders[planetDetail.Planet.Id].Resources)
            {
                int value = resource.Value;

                if (planetDetail.Planet.Resources.ContainsKey(resource.Key) && resource.Key != Resource.Stone)
                    value -= planetDetail.Planet.Resources[resource.Key];

                if (order.BuildingType.HasValue && resource.Key == Resource.Stone)
                    consumers.Add(new BuildingConsumer(order.PlanetId, value, resource.Key, order.BuildingType.Value));
                else if (value > 0)
                    consumers.Add(new ResourceConsumer(order.PlanetId, value, resource.Key));
            }
        }

        private void GetPartners(List<Supplier> suppliers, List<Consumer> consumers, FlyingWorkerGroup flyingWorkerGroups)
        {
            // Add resource with delay
            if (flyingWorkerGroups.Resource.HasValue && flyingWorkerGroups.PlayerIndex == Game.MyIndex)
                suppliers.Add(new WarehouseSupplier(flyingWorkerGroups.NextPlanet, flyingWorkerGroups.Number, flyingWorkerGroups.Resource.Value,
                                                    flyingWorkerGroups.NextPlanetArrivalTick - Game.CurrentTick));

            // Add workers with delay
            if (flyingWorkerGroups.PlayerIndex == Game.MyIndex)
                suppliers.Add(new LaborSupplier(flyingWorkerGroups.NextPlanet, flyingWorkerGroups.Number, flyingWorkerGroups.NextPlanetArrivalTick - Game.CurrentTick));
            else
                consumers.Add(new EnemyConsumer(flyingWorkerGroups.NextPlanet, flyingWorkerGroups.Number, flyingWorkerGroups.NextPlanetArrivalTick - Game.CurrentTick));
        }

        public void NormalizePartners(List<Supplier> suppliers, List<Consumer> consumers)
        {
            Dictionary<Resource, int> resources = new Dictionary<Resource, int>();

            // Get all suppliers quotation
            foreach (Supplier supplier in suppliers)
                if (supplier.Resource.HasValue)
                {
                    if (resources.ContainsKey(supplier.Resource.Value))
                        resources[supplier.Resource.Value] += supplier.Number;
                    else
                        resources.Add(supplier.Resource.Value, supplier.Number);
                }

            // Get all consumers needs
            foreach (Consumer consumer in consumers)
                if (consumer.Resource.HasValue)
                {
                    if (resources.ContainsKey(consumer.Resource.Value))
                        resources[consumer.Resource.Value] -= consumer.Number;
                    else
                        resources.Add(consumer.Resource.Value, -consumer.Number);
                }

            // Add dummy partners
            foreach (var resource in resources)
                if (resource.Value > 0)
                    consumers.Add(new DummyConsumer(resource.Value, resource.Key));
                else if (resource.Value < 0)
                {
                    var supplier = new DummySupplier(-resource.Value, resource.Key);
                    suppliers.Add(supplier);
                }
        }
    }
}
