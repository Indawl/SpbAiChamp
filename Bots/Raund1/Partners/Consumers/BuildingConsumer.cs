﻿using System.Collections.Generic;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Managment;
using SpbAiChamp.Bots.Raund1.Partners.Suppliers;

namespace SpbAiChamp.Bots.Raund1.Partners.Consumers
{
    public class BuildingConsumer : Consumer
    {
        public BuildingType BuildingType { get; private set; }

        public BuildingConsumer(int planetId, BuildingType buildingType, Resource resource, int number) :
            base(planetId, number, resource, 0)
        {
            BuildingType = buildingType;
        }

        public override void GetAction(Supplier supplier, int number, List<MoveAction> moveActions, List<BuildingAction> buildingActions)
        {
            base.GetAction(supplier, number, moveActions, buildingActions);

            if (!Manager.CurrentManager.PlanetDetails[PlanetId].Planet.Building.HasValue)
                buildingActions.Add(new BuildingAction(PlanetId, BuildingType));
            else if (Manager.CurrentManager.PlanetDetails[PlanetId].Planet.Building.Value.BuildingType != BuildingType)
                buildingActions.Add(new BuildingAction(PlanetId, null));
        }

        public override string ToString() => BuildingType.ToString() + base.ToString();
    }
}