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

            // Grouping actions
            var groupMoveActions = moveActions
                .GroupBy(_ => new { _.StartPlanet, _.TargetPlanet, _.TakeResource })
                .Select(_ => new MoveAction(_.Key.StartPlanet, _.Key.TargetPlanet, _.Sum(_ => _.WorkerNumber), _.Key.TakeResource)).ToArray();

            var groupBuildingActions = buildingActions
                .GroupBy(_ => new { _.Planet })
                .Select(_ => new BuildingAction(_.Key.Planet, _.First().BuildingType)).ToArray();

            // Return actions
            return new Action(groupMoveActions, groupBuildingActions, null);
        }

        private List<Supplier> GetSuppliers()
        {
            List<Supplier> suppliers = new List<Supplier>();

            // Analyze all planets
            foreach (Planet planet in Manager.CurrentManager.Game.Planets)
                AddSuppliers(suppliers, planet);

            // Analyze all flying work group
            foreach (FlyingWorkerGroup flyingWorkerGroup in Manager.CurrentManager.Game.FlyingWorkerGroups)
                AddSuppliers(suppliers, flyingWorkerGroup);

            return suppliers;
        }

        private void AddSuppliers(List<Supplier> suppliers, FlyingWorkerGroup flyingWorkerGroup)
        {
            if (flyingWorkerGroup.PlayerIndex != Manager.CurrentManager.Game.MyIndex) return;

            if (flyingWorkerGroup.Resource.HasValue)
                suppliers.Add(new WarehouseSupplier(flyingWorkerGroup.NextPlanet, flyingWorkerGroup.Number, flyingWorkerGroup.Resource,
                                                    flyingWorkerGroup.NextPlanetArrivalTick - Manager.CurrentManager.Game.CurrentTick));
        }

        private void AddSuppliers(List<Supplier> suppliers, Planet planet)
        {
            // Reserve resources
            foreach (var resource in planet.Resources)
                suppliers.Add(new WarehouseSupplier(planet.Id, resource.Value, resource.Key));
        }

        private List<Consumer> GetConsumers()
        {
            List<Consumer> consumers = new List<Consumer>();

            // Analyze all planets
            foreach (Planet planet in Manager.CurrentManager.Game.Planets)
                AddConsumer(consumers, planet);

            // Analyze all flying work group
            foreach (FlyingWorkerGroup flyingWorkerGroup in Manager.CurrentManager.Game.FlyingWorkerGroups)
                AddSuppliers(consumers, flyingWorkerGroup);

            return consumers;
        }

        private void AddSuppliers(List<Consumer> consumers, FlyingWorkerGroup flyingWorkerGroup)
        {
            if (flyingWorkerGroup.PlayerIndex == Manager.CurrentManager.Game.MyIndex)
                consumers.Add(new LaborConsumer(ConsumerType.Supplier, flyingWorkerGroup.NextPlanet, flyingWorkerGroup.Number, null,
                                                                       flyingWorkerGroup.NextPlanetArrivalTick - Manager.CurrentManager.Game.CurrentTick));
            else
                consumers.Add(new EnemyConsumer(flyingWorkerGroup.NextPlanet, -flyingWorkerGroup.Number,
                                                flyingWorkerGroup.NextPlanetArrivalTick - Manager.CurrentManager.Game.CurrentTick));
        }

        private void AddConsumer(List<Consumer> consumers, Planet planet)
        {
            int workerCount = planet.WorkerGroups.Sum(group => group.PlayerIndex == Manager.CurrentManager.Game.MyIndex ? -group.Number : group.Number);

            // if is enemy plannet
            if (workerCount < 0)
            {
                consumers.Add(new EnemyConsumer(planet.Id, workerCount));
                return;
            }

            // Add building
            if (planet.Building.HasValue)
            {
                // need for work
                consumers.Add(new LaborConsumer(ConsumerType.Consumer, planet.Id, -Manager.CurrentManager.Game.BuildingProperties[planet.Building.Value.BuildingType].MaxWorkers));

                // need for resource
                foreach (var resource in Manager.CurrentManager.Game.BuildingProperties[planet.Building.Value.BuildingType].WorkResources)
                    consumers.Add(new ResourceConsumer(planet.Id, -resource.Value, resource.Key));
            }

            // Add new buildng here
            foreach (var building in Manager.CurrentManager.Game.BuildingProperties)
                if (!planet.Building.HasValue || planet.Building.Value.BuildingType != building.Key)
                {
                    // If harvest, then only harvest
                    if (building.Value.Harvest && (!planet.HarvestableResource.HasValue || planet.HarvestableResource.Value != building.Value.ProduceResource.Value))
                        continue;

                    foreach (var resource in building.Value.BuildResources)
                        consumers.Add(new BuildingConsumer(planet.Id, building.Key, -resource.Value, resource.Key));

                    // if need more workers, call more
                    int count = building.Value.MaxWorkers - building.Value.WorkResources.Values.Sum();
                    if (count > 0)
                        consumers.Add(new LaborConsumer(ConsumerType.Consumer, planet.Id, -count));
                }
        }
    }
}
