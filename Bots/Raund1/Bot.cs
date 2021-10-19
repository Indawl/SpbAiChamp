using System;
using System.Collections.Generic;
using System.Linq;
using SpbAiChamp.Model;
using Action = SpbAiChamp.Model.Action;

namespace SpbAiChamp.Bots.Raund1
{
    public class Bot : IBot
    {
        #region Game attributes
        public static Game Game { get; private set; }
        #endregion

        public void SetGame(Game game) => Game = game;
        public Action GetAction()
        {
            List<MoveAction> moveActions = new List<MoveAction>();
            List<BuildingAction> buildingActions = new List<BuildingAction>();

            // Get Suppliers
            List<Supplier> suppliers = GetSuppliers();

            // Get Consumers
            List<Consumer> consumers = GetConsumers();

            // Get transport card
            TransportCard transportCard = new TransportCard(suppliers, consumers);

            // Get actions            
            transportCard.GetActions(moveActions, buildingActions);

            // Return actions
            return new Action(moveActions.ToArray(), buildingActions.ToArray());
        }

        private List<Supplier> GetSuppliers()
        {
            List<Supplier> suppliers = new List<Supplier>();

            // Step 1: Analyze all planets
            foreach (Planet planet in Game.Planets)
                AddSuppliers(suppliers, planet);

            // Step 2: Analyze all flying work group
            foreach (FlyingWorkerGroup flyingWorkerGroup in Game.FlyingWorkerGroups)
                AddSuppliers(suppliers, flyingWorkerGroup);

            return suppliers;
        }

        private void AddSuppliers(List<Supplier> suppliers, FlyingWorkerGroup flyingWorkerGroup)
        {
            Price price = new Price(flyingWorkerGroup.Number, flyingWorkerGroup.Resource, flyingWorkerGroup.Number);
            suppliers.Add(new Supplier(price, flyingWorkerGroup.TargetPlanet, flyingWorkerGroup.NextPlanetArrivalTick - Game.CurrentTick + 0/*TO DO: distance to target planet*/));
        }

        private void AddSuppliers(List<Supplier> suppliers, Planet planet)
        {
            int workerCount = planet.WorkerGroups.Sum(group => group.PlayerIndex == Game.MyIndex ? group.Number : -group.Number);
            if (workerCount <= 0) return;

            // Step 1a: Reserve building
            if (planet.Building.HasValue)
                workerCount -= Game.BuildingProperties[planet.Building.Value.BuildingType].MaxWorkers;
            if (workerCount <= 0) return;

            // Step 1b: Reserve resources
            int count = Math.Min(workerCount, planet.Resources.Values.Sum());

            if (count > 0)
            {
                Price price = new Price(count, planet.Resources);
                suppliers.Add(new Supplier(price, planet.Id));
                                
                workerCount -= count;
                if (workerCount <= 0) return;
            }

            // Step 1c: Reserve free workers
            suppliers.Add(new Supplier(new Price(workerCount), planet.Id));
        }

        private List<Consumer> GetConsumers()
        {
            List<Consumer> consumers = new List<Consumer>();

            // Analyze all planets
            foreach (Planet planet in Game.Planets)
                AddConsumer(consumers, planet);

            return consumers;
        }

        private void AddConsumer(List<Consumer> consumers, Planet planet)
        {
            int workerCount = planet.WorkerGroups.Sum(group => group.PlayerIndex == Game.MyIndex ? group.Number : -group.Number);

            // Add building
            if (planet.Building.HasValue)
            {
                workerCount -= Game.BuildingProperties[planet.Building.Value.BuildingType].MaxWorkers;  // for workers needs                

                foreach (var resource in Game.BuildingProperties[planet.Building.Value.BuildingType].WorkResources)
                    consumers.Add(new Consumer(planet.Id, resource.Value, resource.Key, planet.Building.Value.BuildingType));
            }

            // Add new buildng here
            foreach (var building in Game.BuildingProperties)
                if (!planet.Building.HasValue || planet.Building.Value.BuildingType != building.Key)
                {
                    // If harvest, then only harvest
                    if (building.Value.Harvest && (!planet.HarvestableResource.HasValue || planet.HarvestableResource.Value != building.Value.ProduceResource.Value))
                        continue;

                    foreach (var resource in building.Value.WorkResources)
                        consumers.Add(new Consumer(planet.Id, resource.Value, resource.Key, planet.Building.Value.BuildingType));

                    // if need more workers, call more
                    workerCount -= Math.Max(0, building.Value.MaxWorkers - building.Value.WorkResources.Values.Sum());
                }

            // For reinforcement
            foreach (FlyingWorkerGroup flyingWorkerGroup in Game.FlyingWorkerGroups
                .Where(group => group.PlayerIndex != Game.MyIndex   // enemy
                             && group.TargetPlanet == planet.Id))   // to this planet
                workerCount -= flyingWorkerGroup.Number;

            // Add needs for workers
            workerCount -= planet.Resources.Values.Sum();

            if (workerCount < 0)
                consumers.Add(new Consumer(planet.Id, -workerCount));
        }
    }
}
