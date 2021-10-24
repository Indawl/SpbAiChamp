using System.Collections.Generic;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Managment;

namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class BuildingConsumer : Consumer
    {
        public BuildingType BuildingType { get; private set; }

        public BuildingConsumer(int planetId, BuildingType buildingType) :
            base(planetId, 0, null, 0)
        {
            BuildingType = buildingType;
        }

        public override void GetAction(List<MoveAction> moveActions, List<BuildingAction> buildingActions)
        {
            if (!Manager.CurrentManager.PlanetDetails[PlanetId].Planet.Building.HasValue)
                buildingActions.Add(new BuildingAction(PlanetId, BuildingType));
            else if (Manager.CurrentManager.PlanetDetails[PlanetId].Planet.Building.Value.BuildingType != BuildingType)
                buildingActions.Add(new BuildingAction(PlanetId, null));
        }

        public override string ToString() => BuildingType.ToString() + ": " + base.ToString();
    }
}
