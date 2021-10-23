using System;
using System.Collections.Generic;
using System.Linq;
using SpbAiChamp.Model;
using Action = SpbAiChamp.Model.Action;
using SpbAiChamp.Bots.Raund1.Managment;
using SpbAiChamp.Bots.Raund1.Partners;
using SpbAiChamp.Bots.Raund1.Logistics;

namespace SpbAiChamp.Bots.Raund1
{
    public class Bot : IBot
    {
        public void SetGame(Game game) => Manager.GetNewManager().SetGame(game);

        public Action GetAction()
        {
            List<MoveAction> moveActions = new List<MoveAction>();
            List<BuildingAction> buildingActions = new List<BuildingAction>();

            // Get Suppliers
            List<Supplier> suppliers = GetSuppliers();

            // Get Consumers
            List<Consumer> consumers = GetConsumers();

            // Get transport map
            TransportTask transportTask = new TransportTask(suppliers, consumers);

            // Get actions            
            transportTask.GetActions(moveActions, buildingActions);

            // Return actions
            return new Action(moveActions.ToArray(), buildingActions.ToArray());
        }

        private List<Supplier> GetSuppliers()
        {
            List<Supplier> suppliers = new List<Supplier>();

            // Analyze all planets
            foreach (Planet planet in Game.Planets)
                AddSuppliers(suppliers, planet);

            // Analyze all flying work group
            foreach (FlyingWorkerGroup flyingWorkerGroup in Game.FlyingWorkerGroups.Where(group => group.PlayerIndex == Game.MyIndex))
                AddSuppliers(suppliers, flyingWorkerGroup);

            return suppliers;
        }

        private void AddSuppliers(List<Supplier> suppliers, FlyingWorkerGroup flyingWorkerGroup)
        {
            suppliers.Add(new Supplier(flyingWorkerGroup.TargetPlanet, flyingWorkerGroup.Number, flyingWorkerGroup.Resource,
                                       flyingWorkerGroup.NextPlanetArrivalTick - Game.CurrentTick));

            // Order complete
            if (flyingWorkerGroup.Resource.HasValue)
                if (Orders.ContainsKey(flyingWorkerGroup.TargetPlanet))
                    if (Orders[flyingWorkerGroup.TargetPlanet].ContainsKey(flyingWorkerGroup.Resource.Value))
                        Orders[flyingWorkerGroup.TargetPlanet][flyingWorkerGroup.Resource.Value] = 
                            Math.Max(0, Orders[flyingWorkerGroup.TargetPlanet][flyingWorkerGroup.Resource.Value] - flyingWorkerGroup.Number);
        }

        private void AddSuppliers(List<Supplier> suppliers, Planet planet)
        {
            // Reserve resources
            foreach (var resource in planet.Resources)
                suppliers.Add(new Supplier(planet, resource.Value, resource.Key));

            // Reserve workers
            int workerCount = planet.WorkerGroups.Sum(group => group.PlayerIndex == Game.MyIndex ? group.Number : -group.Number);
            if (workerCount > 0)
                suppliers.Add(new Supplier(planet, workerCount));
        }

        private List<Consumer> GetConsumers()
        {
            List<Consumer> consumers = new List<Consumer>();

            // Analyze all planets
            foreach (Planet planet in Game.Planets)
                AddConsumer(consumers, planet);

            return consumers;
        }

        private Consumer AddOrder(Planet planet, Resource resource, int number)
        {
            if (!Orders.ContainsKey(planet.Id))
                Orders[planet.Id] = new Dictionary<Resource, int>();

            if (Orders[planet.Id].ContainsKey(resource))
                Orders[planet.Id][resource] += number;
            else
                Orders[planet.Id][resource] = number;

            return new Consumer(planet, Orders[planet.Id][resource], resource, planet.Building.Value.BuildingType);
        }

        private void AddConsumer(List<Consumer> consumers, Planet planet)
        {
            int workerCount = planet.WorkerGroups.Sum(group => group.PlayerIndex == Game.MyIndex ? 0 : group.Number);

            // Add building
            if (planet.Building.HasValue)
            {
                workerCount += Game.BuildingProperties[planet.Building.Value.BuildingType].MaxWorkers;  // for workers needs                

                foreach (var resource in Game.BuildingProperties[planet.Building.Value.BuildingType].WorkResources)
                    consumers.Add(AddOrder(planet, resource.Key, resource.Value));
            }

            // Add new buildng here
            foreach (var building in Game.BuildingProperties)
                if (!planet.Building.HasValue || planet.Building.Value.BuildingType != building.Key)
                {
                    // If harvest, then only harvest
                    if (building.Value.Harvest && (!planet.HarvestableResource.HasValue || planet.HarvestableResource.Value != building.Value.ProduceResource.Value))
                        continue;

                    foreach (var resource in building.Value.BuildResources)
                        consumers.Add(new Consumer(planet, resource.Value, resource.Key, building.Key));

                    // if need more workers, call more
                    workerCount += Math.Max(0, building.Value.MaxWorkers - building.Value.WorkResources.Values.Sum());
                }

            // For reinforcement
            foreach (FlyingWorkerGroup flyingWorkerGroup in Game.FlyingWorkerGroups
                .Where(group => group.PlayerIndex != Game.MyIndex   // enemy
                             && group.TargetPlanet == planet.Id))   // to this planet
                workerCount += flyingWorkerGroup.Number;

            // Add worker for resources
            workerCount += planet.Resources.Values.Sum();

            // Add needs for workers
            if (workerCount > 0)
                consumers.Add(new Consumer(planet, workerCount));
        }
    }
}
