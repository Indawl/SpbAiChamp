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

            // Find potencial
            for (int i = 0; i < 100 && CalculatePotencial(out ShippingPlan shippingPlan) > 0; i++)
                if (!Redistribution(shippingPlan)) break;
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
                if (Suppliers[i].Number > 0)
                    for (int j = 0; j < Consumers.Count; j++)
                        if (Consumers[j].Number > 0 && ShippingPlans[i, j].Possible)
                        {
                            ShippingPlans[i, j].IsBase = true;
                            ShippingPlans[i, j].Number = Math.Min(Suppliers[i].Number, Consumers[j].Number);

                            ShippingPlans[i, j].Supplier.Number -= ShippingPlans[i, j].Number;
                            ShippingPlans[i, j].Consumer.Number -= ShippingPlans[i, j].Number;

                            if (ShippingPlans[i, j].Supplier.Number == 0 && ShippingPlans[i, j].Consumer.Number == 0)
                                if (j < Consumers.Count - 1)
                                    ShippingPlans[i, j + 1].IsBase = true;
                                else if (i < Suppliers.Count - 1)
                                    ShippingPlans[i + 1, j].IsBase = true;
                        }
        }

        private int CalculatePotencial(out ShippingPlan optimalPlan)
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
            var optimalPlans = ShippingPlans.Cast<ShippingPlan>().Where(_ => _.Possible && !_.IsBase).ToList();
            optimalPlans.Sort((a, b) => b.Delta.CompareTo(a.Delta));
            optimalPlan = optimalPlans.First();

            return optimalPlan.Delta;
        }

        private bool Redistribution(ShippingPlan optimalPlan)
        {
            optimalPlan.IsBase = true;

            // Count how many base shipping in one row and column
            Consumers.ForEach(_ => _.countBase = 0);
            Suppliers.ForEach(_ => _.countBase = 0);
                        
            for (int i = 0; i < Suppliers.Count; i++)
                for (int j = 0; j < Consumers.Count; j++)
                    if (ShippingPlans[i, j].IsBase)
                    {
                        ShippingPlans[i, j].Consumer.countBase++;
                        ShippingPlans[i, j].Supplier.countBase++;
                    }

            if (optimalPlan.Consumer.countBase < 2 || optimalPlan.Supplier.countBase < 2)
                return false;   //optimal IsBase = true, but dont matter, becouse it finish algorithm....its possible?!

            // Find vertex for loop
            ShippingPlan[] loop = new ShippingPlan[4];
            loop[0] = optimalPlan;
            loop[0].Sign = 1;

            for (int i = 0; i < Suppliers.Count; i++)
                if (ShippingPlans[i, optimalPlan.ConsumerId].IsBase && i != optimalPlan.SupplierId && ShippingPlans[i, optimalPlan.ConsumerId].Consumer.countBase > 1)
                {
                    loop[1] = ShippingPlans[i, optimalPlan.ConsumerId];
                    loop[1].Sign = -loop[0].Sign;
                    break;
                }

            for (int j = 0; j < Consumers.Count; j++)
                if (ShippingPlans[optimalPlan.SupplierId, j].IsBase && j != optimalPlan.ConsumerId && ShippingPlans[optimalPlan.SupplierId, j].Supplier.countBase > 1)
                {
                    loop[2] = ShippingPlans[optimalPlan.SupplierId, j];
                    loop[2].Sign = -loop[0].Sign;
                    break;
                }

            loop[3] = ShippingPlans[loop[1].SupplierId, loop[2].ConsumerId];
            loop[3].Sign = loop[0].Sign;

            // Find old shipping that will be unbased
            ShippingPlan oldShipping = loop[1];

            for (int i = 2; i < 4; i++)
                if (Math.Abs(loop[i].Number) < Math.Abs(oldShipping.Number))
                    oldShipping = loop[i];

            oldShipping.IsBase = false;

            // Redistribute
            for (int i = 0; i < 4; i++)
                loop[i].Number += loop[i].Sign * oldShipping.Number;

            return true;
        }
    }
}
