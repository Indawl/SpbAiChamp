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

            // Step 1: Analyze all planets
            foreach (Planet planet in Game.Planets)
                AddConsumer(consumers, planet);

            return consumers;
        }

        private void AddConsumer(List<Consumer> consumers, Planet planet)
        {
            int workerCount = planet.WorkerGroups.Sum(group => group.PlayerIndex == Game.MyIndex ? group.Number : -group.Number);

            // Step 1a: Add building
            if (planet.Building.HasValue)
                workerCount -= AddConsumer(consumers, planet.Id, workerCount, planet.Building.Value.BuildingType);

            // Step 1b: Add all resources
            AddConsumer(consumers, planet.Id, workerCount, planet.Resources);
        }

        private void AddConsumer(List<Consumer> consumers, int id, int workerCount, IDictionary<Resource, int> resources)
        {
            int resourceCount = resources.Values.Sum();

            if (workerCount < resourceCount)
                consumers.Add(new Consumer(new Price(resourceCount - workerCount), id));
        }

        private int AddConsumer(List<Consumer> consumers, int id, int workerCount, BuildingType buildingType)
        {
            BuildingProperties buildingProperties = Game.BuildingProperties[buildingType];

            // For production
            int needWorkers = Math.Max(buildingProperties.MaxWorkers - workerCount, 0);

            Price price = new Price(needWorkers, buildingProperties.WorkResources);
            if (!price.IsInitial)
                consumers.Add(new Consumer(price, id));

            // For reinforcement
            int enemyCount = 0;

            foreach (FlyingWorkerGroup flyingWorkerGroup in Game.FlyingWorkerGroups
                .Where(group => group.PlayerIndex != Game.MyIndex   // enemy
                             && group.TargetPlanet == id ))         // to this planet
                enemyCount += flyingWorkerGroup.Number;

            if (enemyCount > 0)
            {
                int count = buildingProperties.MaxWorkers - workerCount + enemyCount;
                if (count > 0)
                    consumers.Add(new Consumer(new Price(count), id));
            }

            // How many workers busy
            return buildingProperties.MaxWorkers - needWorkers;
        }
    }
}
