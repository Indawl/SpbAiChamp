using System.Collections.Generic;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Managment;
using SpbAiChamp.Bots.Raund1.Partners.Suppliers;

namespace SpbAiChamp.Bots.Raund1.Partners.Consumers
{
    public class BuildingConsumer : ResourceConsumer
    {
        public BuildingType? BuildingType { get; protected set; }

        public BuildingConsumer(int planetId, int number, BuildingType? buildingType = null, Resource? resource = null) :
            base(planetId, number, resource, 0)
        {
            BuildingType = buildingType;
        }

        public override void GetAction(Supplier supplier, int number, List<MoveAction> moveActions, List<BuildingAction> buildingActions)
        {
            base.GetAction(supplier, number, moveActions, buildingActions);

            if (BuildingType.HasValue)
            {
                if (!Manager.CurrentManager.PlanetDetails[PlanetId].Planet.Building.HasValue)
                    buildingActions.Add(new BuildingAction(PlanetId, BuildingType.Value));
                else if (Manager.CurrentManager.PlanetDetails[PlanetId].Planet.Building.Value.BuildingType != BuildingType.Value)
                    buildingActions.Add(new BuildingAction(PlanetId, null));
            }
        }

        public override string ToString() => BuildingType.ToString() + base.ToString();
    }
}
