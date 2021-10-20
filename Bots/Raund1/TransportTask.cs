using System;
using System.Collections.Generic;
using System.Linq;
using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1
{
    public class TransportTask
    {
        public List<Supplier> Suppliers { get; set; }
        public List<Consumer> Consumers { get; set; }

        public Auction[,] Auctions { get; set; }

        public TransportTask(List<Supplier> suppliers, List<Consumer> consumers)
        {
            Suppliers = suppliers;
            Consumers = consumers;
        }

        public void GetActions(List<MoveAction> moveActions, List<BuildingAction> buildingActions)
        {
            // Create transport map
            CreateTransportMap();

            // Unpack actions: to, from, resource, number[wihtout dummy, all]
            //Dictionary<int, Dictionary<int, Dictionary<Resource, int[]>>> groups = new Dictionary<int, Dictionary<int, Dictionary<Resource, int[]>>>();

            //for (int j = 0; j < Consumers.Count; j++)
            //{
            //    if (!groups.TryGetValue(Consumers[j].PlanetId, out var group_to))
            //        group_to = new Dictionary<int, Dictionary<Resource, int[]>>();

            //    for (int i = 0; i < Suppliers.Count; i++)
            //        // Check is building action
            //        if (!Consumers[j].IsDummy && !Suppliers[i].IsDummy && Consumers[j].BuildingType.HasValue)
            //        {
            //            buildingActions.Add(new BuildingAction(Consumers[j].PlanetId, Consumers[j].BuildingType.Value));
            //            break;
            //        }
            //        else
            //        {
            //            if (!group_to.TryGetValue(Suppliers[i].PlanetId, out var group_from))
            //                group_from = new Dictionary<Resource, int[]>();

            //            if (!group_from.TryGetValue(Suppliers[i].Resource, out var group_res))
            //                group_from = new Dictionary<Resource, int[]>();
            //        }
            //}


            //    for (int i = 0; i < Suppliers.Count; i++)
            //        // Dummy partner no need to use
            //        if (!Auctions[i][j].Consumer.IsDummy && !Auctions[i][j].Supplier.IsDummy)
            //            // Check is building action
            //            if (Consumers[j].BuildingType.HasValue)
            //            {
            //                buildingActions.Add(new BuildingAction(Consumers[j].PlanetId, Consumers[j].BuildingType.Value));
            //                break;
            //            }
            //            else // move action
            //            {
            //                if (Auctions[i][j].Number > 0)
            //                {
            //                    foreach (KeyValuePair<Resource, int> resource in Auctions[i][j].Price.Resources.Where(r => r.Value > 0))
            //                        moveActions.Add(new MoveAction(Suppliers[i].PlanetId, Consumers[j].PlanetId,
            //                                                       resource.Value, resource.Key));
            //                }
            //            }

            //TO DO: union action for flying group
        }

        private void CreateTransportMap()
        {
            // Suppliers price must be = Consumers price
            NormalizePartners();

            // Create Auctions
            CreateAuctions();

            // Get transportation plan
            CreateInitialPlan();

            // Find potencial
            while (true)
            {
                Auction auction = CalculatePotencial();
                if (auction == null || auction.Delta <= 0) break;
                SwapLoop(auction);
            }
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
                if (resource.Value > 0) // Suppliers more
                    Consumers.Add(new Consumer(0, resource.Value, resource.Key, null, 0, true));
                else if (resource.Value < 0) // Consumers more
                    Suppliers.Add(new Supplier(0, -resource.Value, resource.Key, 0, true));

            // And workers
            if (number > 0) // Suppliers more
                Consumers.Add(new Consumer(0, number, null, null, 0, true));
            else if (number < 0) // Consumers more
                Suppliers.Add(new Supplier(0, -number, null, 0, true));

        }

        private void CreateAuctions()
        {
            Auctions = new Auction[Suppliers.Count, Consumers.Count];

            for (int i = 0; i < Suppliers.Count; i++)
                for (int j = 0; j < Consumers.Count; j++)
                    Auctions[i, j] = new Auction(i, j, Suppliers[i], Consumers[j]);
        }

        private void CreateInitialPlan()
        {
            List<Auction> cycleAuction = new List<Auction>();
            int count = Suppliers.Count + Consumers.Count - 1;

            // Initial plan
            for (int i = 0; i < Suppliers.Count; i++)
                for (int j = 0; j < Consumers.Count; j++)
                    if (Auctions[i, j].Supplier.Number > 0 && Auctions[i, j].Consumer.Number > 0)
                    {
                        Auctions[i, j].Number = Math.Min(Auctions[i, j].Supplier.Number, Auctions[i, j].Consumer.Number);
                        Auctions[i, j].IsBase = true;
                        count--;

                        Auctions[i, j].Supplier.Number -= Auctions[i, j].Number;
                        Auctions[i, j].Consumer.Number -= Auctions[i, j].Number;

                        if (Auctions[i, j].Supplier.Number == 0 && Auctions[i, j].Consumer.Number == 0)
                            cycleAuction.Add(Auctions[i, j]);
                    }

            // Fix cycle
            for (int i = 0; i < cycleAuction.Count && count > 0; i++, count--)
                if (cycleAuction[i].SupplierId < Suppliers.Count - 1)
                    Auctions[cycleAuction[i].SupplierId + 1, cycleAuction[i].ConsumerId].IsBase = true;
                else if (cycleAuction[i].ConsumerId < Consumers.Count - 1)
                    Auctions[cycleAuction[i].SupplierId, cycleAuction[i].ConsumerId + 1].IsBase = true;
                else
                {
                    bool added = false;
                    for (int ki = 0; ki < Suppliers.Count - 1; ki++)
                        if (!Auctions[ki, cycleAuction[i].ConsumerId].IsBase)
                        {
                            Auctions[ki, cycleAuction[i].ConsumerId].IsBase = true;
                            added = true;
                            break;
                        }

                    if (!added)
                        for (int kj = 0; kj < Consumers.Count - 1; kj++)
                            if (!Auctions[cycleAuction[i].SupplierId, kj].IsBase)
                            {
                                Auctions[cycleAuction[i].SupplierId, kj].IsBase = true;
                                break;
                            }
                }
        }

        private void SwapLoop(Auction auction)
        {
            //for (int i = 0; i < Suppliers.Count - 1; i++)
            //    for (int j = 0; j < Consumers.Count - 1; j++)
            //        if (!Auctions[i][j].IsBase)
            //            for (int ki = i + 1; ki < Suppliers.Count; ki++)
            //                for (int kj = j + 1; kj < Consumers.Count; kj++)
            //                    if (Auctions[i][kj].IsBase && Auctions[ki][j].IsBase)
            //                    {

            //                    }
        }

        private Auction CalculatePotencial()
        {
            // Init potential
            Suppliers.ForEach(_ => _.Potential = null);
            Consumers.ForEach(_ => _.Potential = null);

            // Get partner potencial
            foreach (var auction in Auctions.Cast<Auction>().Where(_ => _.IsBase).OrderByDescending(_ => _.Cost))
                SetPotencial(auction);

            // Get delta and remember max delta
            Auction maxAuction = null;

            foreach (var auction in Auctions.Cast<Auction>().Where(_ => !_.IsBase))
            {
                auction.Delta = auction.Supplier.Potential.Value + auction.Consumer.Potential.Value - auction.Cost;
                if (maxAuction == null || auction.Delta > maxAuction.Delta) maxAuction = auction;
            }

            return maxAuction;
        }

        private void SetPotencial(Auction auction)
        {
            if (!auction.Supplier.Potential.HasValue && !auction.Consumer.Potential.HasValue)
            {
                auction.Supplier.Potential = 0;
                auction.Consumer.Potential = auction.Cost;
            }

            for (int i = 0; i < Suppliers.Count; i++)
                if (Auctions[i, auction.ConsumerId].IsBase && !Auctions[i, auction.ConsumerId].Supplier.Potential.HasValue)
                    Auctions[i, auction.ConsumerId].Supplier.Potential = Auctions[i, auction.ConsumerId].Cost - auction.Consumer.Potential.Value;

            for (int j = 0; j < Consumers.Count; j++)
                if (Auctions[auction.SupplierId, j].IsBase && !Auctions[auction.SupplierId, j].Consumer.Potential.HasValue)
                    Auctions[auction.SupplierId, j].Consumer.Potential = Auctions[auction.SupplierId, j].Cost - auction.Supplier.Potential.Value;
        }
    }
}
