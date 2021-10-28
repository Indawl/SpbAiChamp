using System.Collections.Generic;
using System.Linq;
using SpbAiChamp.Model;
using Action = SpbAiChamp.Model.Action;
using SpbAiChamp.Bots.Raund1.Managment;
using SpbAiChamp.Bots.Raund1.Partners.Suppliers;
using SpbAiChamp.Bots.Raund1.Partners.Consumers;
using SpbAiChamp.Bots.Raund1.Logistics;

namespace SpbAiChamp.Bots.Raund1
{
    public class Bot : IBot
    {
        public void SetGame(Game game) => Manager.CurrentManager.GetNewManager().SetGame(game);

        public Action GetAction()
        {
            List<MoveAction> moveActions = new List<MoveAction>();
            List<BuildingAction> buildingActions = new List<BuildingAction>();

#if MYDEBUG
            Debug.DebugStrategy.TimeAfterProcessOrder = MyStrategy.watch.ElapsedMilliseconds;
#endif
            // Process Orders
            Manager.CurrentManager.ProcessOrder();
#if MYDEBUG
            Debug.DebugStrategy.TimeAfterProcessOrder = MyStrategy.watch.ElapsedMilliseconds - Debug.DebugStrategy.TimeAfterProcessOrder;
            Debug.DebugStrategy.TimeAfterGetPartners = MyStrategy.watch.ElapsedMilliseconds;
#endif

            // Get Suppliers and Consumers
            Manager.CurrentManager.GetPartners(out List<Supplier> suppliers, out List<Consumer> consumers);
#if MYDEBUG
            Debug.DebugStrategy.TimeAfterGetPartners = MyStrategy.watch.ElapsedMilliseconds - Debug.DebugStrategy.TimeAfterGetPartners;
            Debug.DebugStrategy.TimeAfterNormalize = MyStrategy.watch.ElapsedMilliseconds;
#endif

            // Suppliers price must be = Consumers price
            Manager.CurrentManager.NormalizePartners(suppliers, consumers);
#if MYDEBUG
            Debug.DebugStrategy.TimeAfterNormalize = MyStrategy.watch.ElapsedMilliseconds - Debug.DebugStrategy.TimeAfterNormalize;
            Debug.DebugStrategy.TimeAfterTM = MyStrategy.watch.ElapsedMilliseconds;
#endif

            // Get transport map
            foreach (var resourceDetail in Manager.CurrentManager.ResourceDetails)
                Manager.CurrentManager.TransportTasks[resourceDetail.Key] = new TransportTask(
                    suppliers.Where(_ => _.Resource == resourceDetail.Key).ToList(),
                    consumers.Where(_ => _.Resource == resourceDetail.Key).ToList());

            // And For workers
            Manager.CurrentManager.TransportTaskWorker = new TransportTask(
                suppliers.Where(_ => !_.Resource.HasValue).ToList(),
                consumers.Where(_ => !_.Resource.HasValue).ToList());

#if MYDEBUG
            Debug.DebugStrategy.TimeAfterTM = MyStrategy.watch.ElapsedMilliseconds - Debug.DebugStrategy.TimeAfterTM;
            Debug.DebugStrategy.TimeAfterGetAction = MyStrategy.watch.ElapsedMilliseconds;
#endif
            // Get actions            
            Manager.CurrentManager.TransportTaskWorker.GetActions(moveActions, buildingActions);

            // Grouping actions
            var groupMoveActions = moveActions
                .GroupBy(_ => new { _.StartPlanet, _.TargetPlanet, _.TakeResource })
                .Select(_ => new MoveAction(_.Key.StartPlanet, _.Key.TargetPlanet, _.Sum(_ => _.WorkerNumber), _.Key.TakeResource)).ToArray();

            var groupBuildingActions = buildingActions
                .GroupBy(_ => new { _.Planet })
                .Select(_ => new BuildingAction(_.Key.Planet, _.First().BuildingType)).ToArray();
#if MYDEBUG
            Debug.DebugStrategy.TimeAfterGetAction = MyStrategy.watch.ElapsedMilliseconds - Debug.DebugStrategy.TimeAfterGetAction;
            Debug.DebugStrategy.Println(groupMoveActions, groupBuildingActions);
#endif

            // Return actions
            return new Action(groupMoveActions, groupBuildingActions, null);
        }
    }
}
