using System;
using System.Linq;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Managment;
using SpbAiChamp.Bots.Raund1.Logistics;
using SpbAiChamp.Bots.Raund1.Partners.Consumers;

namespace SpbAiChamp.Bots.Raund1.Debug
{
    public static class DebugStrategy
    {
#if MYDEBUG
        public static int MaxDistr = 0, TickNumberDistr;
        public static long TimeAfterProcessOrder;
        public static long TimeAfterGetPartners;
        public static long TimeAfterNormalize;
        public static long TimeAfterGetAction;
        public static long TimeAfterTM;
        public static long TimeAfter;
        public static long TimePotencial;
        public static long TimeRedist;
        public static long TimeCycle;

        public static void Println(MoveAction[] moveActions, BuildingAction[] buildingActions)
        {
            MyStrategy.watch.Stop();

            var game = Manager.CurrentManager.Game;
            Console.WriteLine();
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
            Console.WriteLine(
                string.Format(">>>>>> CURRENT TICK {0} / {1} >>> Flyig group: {2}: ShortestWay Count Call: {3} <<<<<<<<<<<<<<<<<<<<<<<<<<",
                    game.CurrentTick, game.MaxTickCount, game.FlyingWorkerGroups.Count(_ => _.PlayerIndex == game.MyIndex), ShortestWay.CountCall
            ));
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
            Console.WriteLine();

            Console.WriteLine(">>>  Actions: {0}/{1}", moveActions.Length, buildingActions.Length);
            foreach (var action in moveActions)
                Console.WriteLine("  ==>> {0} to {1}: {2} {3}",
                    action.StartPlanet, action.TargetPlanet, action.WorkerNumber, action.TakeResource.HasValue ? action.TakeResource : "workers");

            Console.WriteLine(">>>  Building on planets:");
            foreach (var bd in Manager.CurrentManager.BuildingDetails)
                if (bd.Value.Planets.Count > 0)
                {
                    Console.Write("  {0}:", bd.Key);
                    bd.Value.Planets.ToList().ForEach(_ => Console.Write(" {0}", _));
                    Console.WriteLine();
                }
            Console.WriteLine(">>>  Capital: {0}", Manager.CurrentManager.CapitalPlanet.Planet.Id);
            Console.Write(">>>  My planet {0} are:", Manager.CurrentManager.MyPlanets.Count);
            Manager.CurrentManager.MyPlanets.Values.ToList().ForEach(_ => Console.Write(" {0}", _.Planet.Id));
            Console.WriteLine();
            Console.WriteLine(">>>  Transport Tax: {0}", Manager.CurrentManager.TransportTax);

            Console.WriteLine(">>>  Resources:");
            foreach (var rd in Manager.CurrentManager.ResourceDetails)
            {
                Console.WriteLine("  {0}: {1}({2}/{3}) numbers, manufacturer {4}",
                    rd.Key, rd.Value.Number, rd.Value.NumberIn, rd.Value.NumberOut,
                    rd.Value.BuildingType);
            }

            PrintTransporTask(Manager.CurrentManager.TransportTaskWorker);
            foreach (var tc in Manager.CurrentManager.TransportTasks.Values)
                PrintTransporTask(tc);

            Console.WriteLine(">>>  Max redist: {0} on {1} tick", MaxDistr, TickNumberDistr);
            Console.WriteLine(">>>  Runtime {0}ms: PO {1}ms, GP {2}ms, Norm {3}ms, TM {4}ms, GA {5}ms", MyStrategy.watch.ElapsedMilliseconds,
                TimeAfterProcessOrder, TimeAfterGetPartners, TimeAfterNormalize, TimeAfterTM, TimeAfterGetAction);
            Console.WriteLine(">>>  Runtime TM: PA {0}ms, UV {1}ms, Redist {2}ms, Cy {3}ms",
                TimeAfter, TimePotencial, TimeRedist, TimeCycle);

            TimeAfter = 0;
            TimePotencial = 0;
            TimeRedist = 0;
            TimeCycle = 0;

            ShortestWay.CountCall = 0;
            Partners.Partner.MaxId = 0;
        }

        private static void PrintTransporTask(TransportTask tc)
        {
            Console.WriteLine(">>>  Transport Task: {0} suppliers/{1} consumers", tc.Suppliers.Count, tc.Consumers.Count);

            for (int i = 0; i < tc.Suppliers.Count; i++)
            {
                var supplier = tc.Suppliers[i];
                Console.WriteLine(
                        string.Format("==>> Id {0} from {1} planet {2}: have {3} {4}", supplier.Id,
                            supplier.PlanetId, supplier.GetType().Name, supplier.Quantity, supplier.Resource.HasValue ? supplier.Resource : "workers"
                ));

                for (int j = 0; j < tc.Consumers.Count; j++)
                //if (tc.ShippingPlans[i, j].Number > 0)
                {
                    var consumer = tc.Consumers[j];
                    Console.WriteLine(
                        string.Format("    <<== Id {0}{1} to {2} planet {3}: need {4} {5} delivery {6} by price {7}",
                            consumer.Id, consumer is SupplierConsumer ? ((consumer as SupplierConsumer).Supplier == null ? string.Empty : "(" + (consumer as SupplierConsumer).Supplier.Id + ")") : string.Empty,
                            consumer.PlanetId, consumer.GetType().Name, consumer.Quantity, consumer.Resource.HasValue ? consumer.Resource : "workers",
                            tc.ShippingPlans[i, j].Number, tc.ShippingPlans[i, j].Cost
                    ));
                    if (consumer is BuildingConsumer && !(consumer is EnemyConsumer) && consumer.Resource.HasValue && consumer.Resource.Value == Resource.Stone)
                        Console.WriteLine(
                        string.Format("    ==== Id {0} on {1} planet {2}: need {3}", consumer.Id,
                            consumer.PlanetId, consumer.GetType().Name, (consumer as BuildingConsumer).BuildingType));
                }
            }

            if (tc.Suppliers.Count + tc.Consumers.Count > 0)
                Console.WriteLine(">>>>>>>>>>>>  Number of base {0}: {1} after initial, {2} at the end, redist = {3} {4}",
                    tc.Suppliers.Count + tc.Consumers.Count - 1,
                    tc.BaseCount, tc.BaseCountAfter, tc.CountRedist, tc.ImpossibleHappens ? ">>>IMPOSIBBLE POTENCIAL<<<" : string.Empty
                );

            MaxDistr = Math.Max(MaxDistr, tc.CountRedist);
            if (tc.CountRedist == MaxDistr) TickNumberDistr = Manager.CurrentManager.Game.CurrentTick;
        }
#endif
    }
}
