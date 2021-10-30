using System;
using System.Collections.Generic;
using System.Linq;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Managment;

namespace SpbAiChamp.Bots.Raund1.Contracts
{
    public class Order
    {
        public int DurationQuarter { get; set; } = 10;

        public int PlanetId { get; }
        public int TickStart { get; }
        public int TickEnd { get; set; }

        public Dictionary<Resource, int> Resources { get; set; } = new Dictionary<Resource, int>();
        public int Number { get; set; }

        public BuildingType? BuildingType { get; set; }

        public int Delay { get; set; } = 0;

        public Order(int planetId)
        {
            PlanetId = planetId;
            TickStart = Manager.CurrentManager.Game.CurrentTick;
            TickEnd = Manager.CurrentManager.Game.CurrentTick;
        }

        public void CreateBuildingOrder(BuildingType buildingType)
        {
            BuildingType = buildingType;
            var buildingDetail = Manager.CurrentManager.BuildingDetails[buildingType].BuildingProperties;

            Resources = new Dictionary<Resource, int>(buildingDetail.BuildResources);
            Number = Math.Max(0, buildingDetail.MaxWorkers - buildingDetail.BuildResources.Values.Sum());
            TickEnd = int.MaxValue;
        }

        public void CreateResourceOrder(PlanetDetail planetDetail)
        {
            var buildingType = BuildingType.HasValue ? BuildingType : planetDetail.Planet.Building?.BuildingType ?? null;
            if (!buildingType.HasValue) return;

            var buildingDetail = Manager.CurrentManager.BuildingDetails[buildingType.Value];

            // For work in factory (in exist building)
            if (!BuildingType.HasValue)
                Number = buildingDetail.BuildingProperties.MaxWorkers;

            // For resources
            foreach (var resource in buildingDetail.BuildingProperties.WorkResources)
            {
                // Get stock
                planetDetail.Planet.Resources.TryGetValue(resource.Key, out var stock);

                // How many needs
                int number = stock + DurationQuarter * resource.Value;

                if (Resources.ContainsKey(resource.Key))
                    Resources[resource.Key] += number;
                else Resources.Add(resource.Key, number);
            }

            if (!BuildingType.HasValue)
                TickEnd += DurationQuarter;
        }
    }
}
