using System.Collections.Generic;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Managment;
using SpbAiChamp.Bots.Raund1.Partners.Suppliers;

namespace SpbAiChamp.Bots.Raund1.Partners.Consumers
{
    public class BuildingConsumer : Consumer
    {
        public BuildingType BuildingType { get; protected set; }

        public BuildingConsumer(int planetId, int number, BuildingType buildingType, Resource resource) :
            base(planetId, number, resource)
        {
            BuildingType = buildingType;
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

        public override int CalculateCost(Supplier supplier)
        {
            var buildingDetail = Manager.CurrentManager.BuildingDetails[BuildingType];
            foreach (var planetId in buildingDetail.Planets)
                if (Manager.CurrentManager.PlanetDetails[planetId].WorkerCount < 0.8 * buildingDetail.BuildingProperties.MaxWorkers)
                    return int.MaxValue;

            return ToInt(BuildingDetail.GetCost(PlanetId, BuildingType) + base.CalculateCost(supplier));
        }
    }
}
