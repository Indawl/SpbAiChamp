using System;
using System.Collections.Generic;
using System.Linq;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Partners.Suppliers;
using SpbAiChamp.Bots.Raund1.Partners.Consumers;

namespace SpbAiChamp.Bots.Raund1.Logistics
{
    public class TransportTask
    {
        public List<Supplier> Suppliers { get; set; }
        public List<Consumer> Consumers { get; set; }

        public ShippingPlan[,] ShippingPlans { get; set; }

#if MYDEBUG
        public int BaseCount = 0;
        public int BaseCountAfter = 0;
        public int CountRedist = 0;
        public bool ImpossibleHappens = false;
#endif

        public TransportTask(List<Supplier> suppliers, List<Consumer> consumers)
        {
            Suppliers = suppliers;
            Consumers = consumers;

            // Create transport map
            CreateTransportMap();
        }

        public void GetActions(List<MoveAction> moveActions, List<BuildingAction> buildingActions)
        {
            var shippingPlans = ShippingPlans.Cast<ShippingPlan>().ToList();
            shippingPlans.Sort((a, b) => a.Cost.CompareTo(b.Cost));
            shippingPlans.ForEach(_ => _.GetAction(moveActions, buildingActions));
        }

        private void CreateTransportMap()
        {
#if MYDEBUG
            Debug.DebugStrategy.TimeAfter = MyStrategy.watch.ElapsedMilliseconds;
#endif
            // Create shipping plan
            CreateShippingPlan();

            // Get transportation plan
            CreateInitialPlan();
#if MYDEBUG
            Debug.DebugStrategy.TimeAfter = MyStrategy.watch.ElapsedMilliseconds - Debug.DebugStrategy.TimeAfter;
#endif

            if (Suppliers.Count + Consumers.Count == 0) return;

#if MYDEBUG
            BaseCount = ShippingPlans.Cast<ShippingPlan>().Count(_ => _.IsBase);
#endif

            // Find potencial
            for (int i = 0; i < 100 && CalculatePotencial(out List<ShippingPlan> optimalPlans); i++
                //   for (int i = 0; CalculatePotencial(out List<ShippingPlan> optimalPlans); i++
#if MYDEBUG
                , CountRedist++
#endif
                )
                if (!Redistribution(optimalPlans)) break;

#if MYDEBUG
            BaseCountAfter = ShippingPlans.Cast<ShippingPlan>().Count(_ => _.IsBase);
#endif
        }

        private void CreateShippingPlan()
        {
            ShippingPlans = new ShippingPlan[Suppliers.Count, Consumers.Count];

            for (int i = 0; i < Suppliers.Count; i++)
                for (int j = 0; j < Consumers.Count; j++)
                    ShippingPlans[i, j] = new ShippingPlan(i, j, Suppliers[i], Consumers[j]);
        }

        private void CreateInitialPlan()
        {
            // Initial plan 
            for (int i = 0; i < Suppliers.Count; i++)
                for (int j = 0; j < Consumers.Count; j++)
                    if (Suppliers[i].Number > 0 && Consumers[j].Number > 0)
                    {
                        ShippingPlans[i, j].IsBase = true;
                        ShippingPlans[i, j].Number = Math.Min(Suppliers[i].Number, Consumers[j].Number);

                        ShippingPlans[i, j].Supplier.Number -= ShippingPlans[i, j].Number;
                        ShippingPlans[i, j].Consumer.Number -= ShippingPlans[i, j].Number;

                        if (ShippingPlans[i, j].Supplier.Number == 0 && ShippingPlans[i, j].Consumer.Number == 0)
                            if (j + 1 < Consumers.Count)
                                ShippingPlans[i, j + 1].IsBase = true;
                            else if (i + 1 < Suppliers.Count)
                                ShippingPlans[i + 1, j].IsBase = true;
                    }
        }

        private bool CalculatePotencial(out List<ShippingPlan> optimalPlans)
        {
#if MYDEBUG
            long tp = MyStrategy.watch.ElapsedMilliseconds;
#endif

            // Init potential
            Suppliers.ForEach(_ => _.Potential = null);
            Consumers.ForEach(_ => _.Potential = null);

            // Get Base shipping
            var shippingPlans = ShippingPlans.Cast<ShippingPlan>().Where(_ => _.IsBase).ToList();
            Suppliers[0].Potential = 0;

            // Calculate potencial
            bool searching = true, processing;
            while (searching)
            {
                searching = processing = false;
                foreach (var shippingPlan in shippingPlans)
                    if (shippingPlan.Supplier.Potential.HasValue && !shippingPlan.Consumer.Potential.HasValue)
                    {
                        shippingPlan.Consumer.Potential = shippingPlan.Cost - shippingPlan.Supplier.Potential.Value;
                        processing = true;
                    }
                    else if (shippingPlan.Consumer.Potential.HasValue && !shippingPlan.Supplier.Potential.HasValue)
                    {
                        shippingPlan.Supplier.Potential = shippingPlan.Cost - shippingPlan.Consumer.Potential.Value;
                        processing = true;
                    }
                    else if (!shippingPlan.Supplier.Potential.HasValue && !shippingPlan.Consumer.Potential.HasValue)
                        searching = true;

                if (searching && !processing) // in right algoritm is inpossible?! 
                {
#if MYDEBUG
                    ImpossibleHappens = true;
#endif
                    foreach (var shippingPlan in shippingPlans)
                        if (!shippingPlan.Supplier.Potential.HasValue)
                        {
                            shippingPlan.Supplier.Potential = 0;
                            break;
                        }
                }
            }

            // Calculate delta
            foreach (var shippingPlan in ShippingPlans)
                shippingPlan.Delta = shippingPlan.Supplier.Potential.Value + shippingPlan.Consumer.Potential.Value - shippingPlan.Cost;

            // Find max delta
            optimalPlans = ShippingPlans.Cast<ShippingPlan>().Where(_ => !_.IsBase && _.Delta > 0).ToList();
            optimalPlans.Sort((a, b) => b.Delta.CompareTo(a.Delta));

#if MYDEBUG
            Debug.DebugStrategy.TimePotencial += MyStrategy.watch.ElapsedMilliseconds - tp;
#endif

            return optimalPlans.Count > 0;
        }

        private bool Redistribution(List<ShippingPlan> optimalPlans)
        {
#if MYDEBUG
            long tp = MyStrategy.watch.ElapsedMilliseconds;
#endif
            List<ShippingPlan> newPlans = new List<ShippingPlan>();

            foreach (var optimalPlan in optimalPlans)
            {
#if MYDEBUG
                long cy = MyStrategy.watch.ElapsedMilliseconds;
#endif
                var cycle = FindCycle(optimalPlan);
#if MYDEBUG
                Debug.DebugStrategy.TimeCycle += MyStrategy.watch.ElapsedMilliseconds - cy;
#endif
                if (cycle.Count < 4) continue;

                var cycleMinus = cycle.Where((value, index) => index % 2 == 1).ToList();
                int min = cycleMinus.Min(_ => _.Number);

                cycleMinus.First(_ => _.Number == min).IsBase = false;
                newPlans.Add(optimalPlan);

                for (int i = 0, sign = 1; i < cycle.Count; i++, sign *= -1)
                    cycle[i].Number += sign * min;

                if (newPlans.Count > 5) break;
            }
#if MYDEBUG
            Debug.DebugStrategy.TimeRedist += MyStrategy.watch.ElapsedMilliseconds - tp;
#endif
            newPlans.ForEach(_ => _.IsBase = true);
            return newPlans.Count > 0;
        }

        private List<ShippingPlan> FindCycle(ShippingPlan optimalPlan)
        {
            List<ShippingPlan> cycle = new List<ShippingPlan>();

            List<ShippingPlan> shippingPlans = new List<ShippingPlan>(Suppliers.Count * Consumers.Count);
            for (int i = 0; i < Suppliers.Count; i++)
                if (ShippingPlans[i, optimalPlan.ConsumerId].IsBase)
                    shippingPlans.Add(ShippingPlans[i, optimalPlan.ConsumerId]);
            for (int j = 0; j < Consumers.Count; j++)
                if (ShippingPlans[optimalPlan.SupplierId, j].IsBase)
                    shippingPlans.Add(ShippingPlans[optimalPlan.SupplierId, j]);

            shippingPlans.Sort((a, b) => b.Cost.CompareTo(a.Cost));

            foreach (var shippingPlanSupplier in shippingPlans)
                if (shippingPlanSupplier.SupplierId == optimalPlan.SupplierId)
                {
                    for (int i = 0; i < Suppliers.Count; i++)
                        if (shippingPlanSupplier.IsBase &&
                            ShippingPlans[i, shippingPlanSupplier.ConsumerId].IsBase &&
                            ShippingPlans[i, optimalPlan.ConsumerId].IsBase)
                        {
                            cycle.Add(optimalPlan);
                            cycle.Add(shippingPlanSupplier);
                            cycle.Add(ShippingPlans[i, shippingPlanSupplier.ConsumerId]);
                            cycle.Add(ShippingPlans[i, optimalPlan.ConsumerId]);
                            return cycle;
                        }
                }
                else
                {
                    for (int j = 0; j < Consumers.Count; j++)
                        if (shippingPlanSupplier.IsBase &&
                            ShippingPlans[shippingPlanSupplier.SupplierId, j].IsBase &&
                            ShippingPlans[optimalPlan.SupplierId, j].IsBase)
                        {
                            cycle.Add(optimalPlan);
                            cycle.Add(shippingPlanSupplier);
                            cycle.Add(ShippingPlans[shippingPlanSupplier.SupplierId, j]);
                            cycle.Add(ShippingPlans[optimalPlan.SupplierId, j]);
                            return cycle;
                        }
                }

            return cycle;
        }
    }
}
