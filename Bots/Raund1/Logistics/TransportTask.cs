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

#if MYDEBUG
            Debug.DebugStrategy.BaseCount = ShippingPlans.Cast<ShippingPlan>().Count(_ => _.IsBase);
#endif

            // Find potencial
            //for (int i = 0; i < 100000 && CalculatePotencial(out List<ShippingPlan> optimalPlans); i++)
            for (int i = 0; CalculatePotencial(out List<ShippingPlan> optimalPlans); i++
#if MYDEBUG
                , Debug.DebugStrategy.CountRedist++
#endif
                )
                if (!Redistribution(optimalPlans)) break;

#if MYDEBUG
            Debug.DebugStrategy.BaseCountAfter = ShippingPlans.Cast<ShippingPlan>().Count(_ => _.IsBase);
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
                    if (Suppliers[i].Number > 0 && Consumers[j].Number > 0 && ShippingPlans[i, j].Possible)
                    {
                        ShippingPlans[i, j].IsBase = true;
                        ShippingPlans[i, j].Number = Math.Min(Suppliers[i].Number, Consumers[j].Number);

                        ShippingPlans[i, j].Supplier.Number -= ShippingPlans[i, j].Number;
                        ShippingPlans[i, j].Consumer.Number -= ShippingPlans[i, j].Number;

                        if (ShippingPlans[i, j].Supplier.Number == 0 && ShippingPlans[i, j].Consumer.Number == 0)
                            if (!SetNextBaseForSupplier(i, j))
                                SetNextBaseForConsumer(i, j);
                    }
        }

        private bool SetNextBaseForSupplier(int supplierId, int consumerId)
        {
            for (int j = consumerId + 1; j < Consumers.Count; j++)
                if (ShippingPlans[supplierId, j].Possible)
                {
                    ShippingPlans[supplierId, j].IsBase = true;
                    return true;
                }
            return false;
        }

        private bool SetNextBaseForConsumer(int supplierId, int consumerId)
        {
            for (int i = supplierId + 1; i < Suppliers.Count; i++)
                if (ShippingPlans[i, consumerId].Possible)
                {
                    ShippingPlans[i, consumerId].IsBase = true;
                    return true;
                }
            return false;
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

                if (searching && !processing) // in right algoritm is inpossible?! but cycle is norm for me
                    foreach (var shippingPlan in shippingPlans)
                        if (!shippingPlan.Supplier.Potential.HasValue)
                        {
                            shippingPlan.Supplier.Potential = 0;
                            break;
                        }
            }

            // Calculate delta
            foreach (var shippingPlan in ShippingPlans)
                shippingPlan.Delta = shippingPlan.Supplier.Potential.Value + shippingPlan.Consumer.Potential.Value - shippingPlan.Cost;

            // Find max delta
            optimalPlans = ShippingPlans.Cast<ShippingPlan>().Where(_ => _.Possible && !_.IsBase && _.Delta > 0).ToList();
            optimalPlans.Sort((a, b) => b.Delta.CompareTo(a.Delta));

            return optimalPlans.Count > 0;
        }

        private bool Redistribution(List<ShippingPlan> optimalPlans)
        {
            var shippingPlans = ShippingPlans.Cast<ShippingPlan>().ToList();

            shippingPlans.ForEach(_ => _.Processed = !_.Possible || !_.IsBase);

            foreach (var optimalPlan in optimalPlans)
            {
                shippingPlans.ForEach(_ =>
                {
                    _.Visited = _.Processed;
                    _.FromShipping = null;
                });

                var cycle = FindCycle(optimalPlan);
                if (cycle.Count < 4) continue;

                var cycleMinus = cycle.Where((value, index) => index % 2 == 1).ToList();
                int min = cycleMinus.Min(_ => _.Number);

                optimalPlan.IsBase = true;
                cycleMinus.First(_ => _.Number == min).IsBase = false;

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

            bool direction = false;
            ShippingPlan shipping = null;

            while (processShippings.Count > 0)
            {
                shipping = processShippings.Dequeue();
                if (shipping.Visited) continue;

                if (cycle.Count == 0)
                    cycle.Add(shippingPlan);
                else if (shipping == shippingPlan)
                    break;
                else shipping.Visited = true;

                #region Neigborns
                if (direction)
                {
                    for (int j = 0; j < Consumers.Count; j++)
                        if (!ShippingPlans[shippingPlan.SupplierId, j].Visited)
                        {
                            processShippings.Enqueue(ShippingPlans[shippingPlan.SupplierId, j]);
                            ShippingPlans[shippingPlan.SupplierId, j].FromShipping = shipping;
                        }
                }
                else
                {
                    for (int i = 0; i < Suppliers.Count; i++)
                        if (!ShippingPlans[i, shippingPlan.ConsumerId].Visited)
                        {
                            processShippings.Enqueue(ShippingPlans[i, shippingPlan.ConsumerId]);
                            ShippingPlans[i, shippingPlan.ConsumerId].FromShipping = shipping;
                        }
                }

                direction = !direction;
                #endregion
            }

            shipping = shippingPlan.FromShipping;
            while (shipping != null && shipping != shippingPlan)
            {
                cycle.Add(shipping);
                shipping = shipping.FromShipping;
            }
            cycle.Add(shippingPlan);

            return cycle;
        }
    }
}
