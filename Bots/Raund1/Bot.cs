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
            if (Manager.CurrentManager.Game.CurrentTick < 150) return new Action(new MoveAction[0], new BuildingAction[0], null);

            List<MoveAction> moveActions = new List<MoveAction>();
            List<BuildingAction> buildingActions = new List<BuildingAction>();

            // Process Orders
            Manager.CurrentManager.ProcessOrder();

            // Get Suppliers and Consumers
            Manager.CurrentManager.GetPartners(out List<Supplier> suppliers, out List<Consumer> consumers);

            // Get transport map
            Manager.CurrentManager.TransportTask = new TransportTask(suppliers, consumers);

            // Get actions            
            Manager.CurrentManager.TransportTask.GetActions(moveActions, buildingActions);

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
    }
}
