﻿using System;
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
            // Suppliers price must be = Consumers price
            NormalizePartners();

            // Create shipping plan
            CreateShippingPlan();

            // Get transportation plan
            CreateInitialPlan();

            if (Suppliers.Count + Consumers.Count == 0) return;

#if MYDEBUG
            BaseCount = ShippingPlans.Cast<ShippingPlan>().Count(_ => _.IsBase);
#endif

            // Find potencial
            //for (int i = 0; i < 100000 && CalculatePotencial(out List<ShippingPlan> optimalPlans); i++)
            for (int i = 0; CalculatePotencial(out List<ShippingPlan> optimalPlans); i++
#if MYDEBUG
                , CountRedist++
#endif
                )
                if (!Redistribution(optimalPlans)) break;

#if MYDEBUG
            BaseCountAfter = ShippingPlans.Cast<ShippingPlan>().Count(_ => _.IsBase);
#endif
        }

        private void NormalizePartners()
        {
            Dictionary<Resource, int> resources = new Dictionary<Resource, int>();
            int number = 0;

            // Get all suppliers quotation
            foreach (Supplier supplier in Suppliers)
                if (supplier.Resource.HasValue)
                {
                    if (resources.ContainsKey(supplier.Resource.Value))
                        resources[supplier.Resource.Value] += supplier.Number;
                    else
                        resources.Add(supplier.Resource.Value, supplier.Number);
                }
                else number += supplier.Number;

            // Get all consumers needs
            foreach (Consumer consumer in Consumers)
                if (consumer.Resource.HasValue)
                {
                    if (resources.ContainsKey(consumer.Resource.Value))
                        resources[consumer.Resource.Value] -= consumer.Number;
                    else
                        resources.Add(consumer.Resource.Value, -consumer.Number);
                }
                else number -= consumer.Number;

            // Add dummy partners
            foreach (var resource in resources)
                if (resource.Value > 0)
                    Consumers.Add(new DummyConsumer(resource.Value, resource.Key));
                else if (resource.Value < 0)
                    Suppliers.Add(new DummySupplier(-resource.Value, resource.Key));

            // And dummy workers
            if (number > 0)
                Consumers.Add(new DummyConsumer(number));
            else if (number < 0)
                Suppliers.Add(new DummySupplier(-number));
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

            return optimalPlans.Count > 0;
        }

        private bool Redistribution(List<ShippingPlan> optimalPlans)
        {
            var shippingPlans = ShippingPlans.Cast<ShippingPlan>().ToList();

            shippingPlans.ForEach(_ => _.Processed = !_.IsBase);

            foreach (var optimalPlan in optimalPlans)
            {
                shippingPlans.ForEach(_ =>
                {
                    _.Visited = _.Processed;
                    _.FromShipping = null;
                    _.Direction = null;
                });

                var cycle = FindCycle(optimalPlan);
                if (cycle.Count < 4) continue;

                var cycleMinus = cycle.Where(_ => _.Direction != optimalPlan.Direction).ToList();
                int min = cycleMinus.Min(_ => _.Number);
                var oldPlan = cycleMinus.First(_ => _.Number == min);

                optimalPlan.IsBase = true;
                oldPlan.IsBase = false;
                oldPlan.Processed = true;

                for (int i = 0, sign = 1; i < cycle.Count; i++, sign *= -1)
                    cycle[i].Number += sign * min;
            }

            return optimalPlans.Exists(_ => _.IsBase);
        }

        private List<ShippingPlan> FindCycle(ShippingPlan shippingPlan)
        {
            List<ShippingPlan> cycle = new List<ShippingPlan>();
            shippingPlan.Visited = false;

            Queue<ShippingPlan> processShippings = new Queue<ShippingPlan>();
            processShippings.Enqueue(shippingPlan);

            ShippingPlan shipping = null;

            while (processShippings.Count > 0)
            {
                shipping = processShippings.Dequeue();
                if (shipping.Visited) continue;

                if (!shipping.Direction.HasValue)
                    shipping.Direction = true;
                else if (shipping == shippingPlan && shipping.Direction == shipping.FromShipping.Direction) continue;
                else if (shipping == shippingPlan) break;
                else shipping.Visited = true;

                #region Neigborns
                if (shipping.Direction.Value)
                {
                    for (int j = 0; j < Consumers.Count; j++)
                        if (j != shipping.ConsumerId && !ShippingPlans[shipping.SupplierId, j].Visited)
                        {
                            processShippings.Enqueue(ShippingPlans[shipping.SupplierId, j]);
                            ShippingPlans[shipping.SupplierId, j].FromShipping = shipping;
                            ShippingPlans[shipping.SupplierId, j].Direction = !shipping.Direction.Value;
                        }
                }
                else
                {
                    for (int i = 0; i < Suppliers.Count; i++)
                        if (i != shipping.SupplierId && !ShippingPlans[i, shipping.ConsumerId].Visited)
                        {
                            processShippings.Enqueue(ShippingPlans[i, shipping.ConsumerId]);
                            ShippingPlans[i, shipping.ConsumerId].FromShipping = shipping;
                            ShippingPlans[i, shipping.ConsumerId].Direction = !shipping.Direction.Value;
                        }
                }
                #endregion
            }

            if (shipping != shippingPlan || shipping.FromShipping == null) return cycle;

            do
            {
                cycle.Add(shipping);
                shipping = shipping.FromShipping;
            } while (shipping != shippingPlan);

            return cycle;
        }
    }
}
