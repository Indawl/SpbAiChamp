using System.Collections.Generic;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Managment;
using SpbAiChamp.Bots.Raund1.Partners.Suppliers;

namespace SpbAiChamp.Bots.Raund1.Partners.Consumers
{
    public class BuildingConsumer : LaborConsumer
    {
        public BuildingType BuildingType { get; protected set; }

        public BuildingConsumer(int planetId, int number, Resource resource, BuildingType buildingType) :
            base(planetId, number)
        {
            BuildingType = buildingType;
            Resource = resource;
        }

        public override string ToString() => BuildingType.ToString() + base.ToString();

        public override void GetAction(Supplier supplier, int number, List<MoveAction> moveActions, List<BuildingAction> buildingActions)
        {
            base.GetAction(supplier, number, moveActions, buildingActions);

            if (!Manager.CurrentManager.PlanetDetails[PlanetId].Planet.Building.HasValue)
                buildingActions.Add(new BuildingAction(PlanetId, BuildingType));
            //else if (Manager.CurrentManager.PlanetDetails[PlanetId].Planet.Building.Value.BuildingType != BuildingType)
            //    buildingActions.Add(new BuildingAction(PlanetId, null));
        }
    }
}
