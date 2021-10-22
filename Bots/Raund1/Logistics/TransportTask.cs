using System;
using System.Collections.Generic;
using System.Linq;
using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Logistics
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

            // Repack to actions
            foreach (Auction auction in Auctions)
                if (auction.IsBase && !auction.Supplier.IsDummy && !auction.Consumer.IsDummy)
                    if (auction.Consumer.BuildingType.HasValue)
                        buildingActions.Add(new BuildingAction(auction.Consumer.Planet.Id, auction.Consumer.BuildingType.Value));
                    else if (auction.Supplier.Planet.Id != auction.Consumer.Planet.Id)
                        moveActions.Add(new MoveAction(auction.Supplier.Planet.Id, auction.Consumer.Planet.Id, auction.Number,
                            (!auction.Consumer.Resource.HasValue || !auction.Supplier.Resource.HasValue ||
                             auction.Consumer.Resource.Value != auction.Supplier.Resource.Value) ? null : auction.Consumer.Resource));
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
                if (!SwapLoop(auction)) break;
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
                    Consumers.Add(new Consumer(Bot.Game.Planets[0], resource.Value, resource.Key, null, 0, true));
                else if (resource.Value < 0) // Consumers more
                    Suppliers.Add(new Supplier(Bot.Game.Planets[0], -resource.Value, resource.Key, 0, true));

            // And workers
            if (number > 0) // Suppliers more
                Consumers.Add(new Consumer(Bot.Game.Planets[0], number, null, null, 0, true));
            else if (number < 0) // Consumers more
                Suppliers.Add(new Supplier(Bot.Game.Planets[0], -number, null, 0, true));

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

        private Auction GetSwapLoop(Auction auction)
        {
            for (int i = 0; i < Suppliers.Count - 1; i++)
                for (int j = 0; j < Consumers.Count - 1; j++)
                    if (Auctions[i, auction.ConsumerId].IsBase && Auctions[auction.SupplierId, j].IsBase && Auctions[auction.SupplierId, auction.ConsumerId].IsBase)
                        return Auctions[auction.SupplierId, auction.ConsumerId];

            return null;
        }

        private bool SwapLoop(Auction auction)
        {
            Auction swapAuction = GetSwapLoop(auction);
            if (swapAuction == null) return false;

            int number = Math.Min(Auctions[auction.SupplierId, swapAuction.ConsumerId].Number, Auctions[swapAuction.SupplierId, auction.ConsumerId].Number);
            Auctions[auction.SupplierId, swapAuction.ConsumerId].Number -= number;
            Auctions[swapAuction.SupplierId, auction.ConsumerId].Number -= number;
            Auctions[swapAuction.SupplierId, swapAuction.ConsumerId].Number += number;
            Auctions[auction.SupplierId, auction.ConsumerId].Number += number;

            Auctions[auction.SupplierId, auction.ConsumerId].IsBase = true;

            if (Auctions[auction.SupplierId, swapAuction.ConsumerId].Number == 0)
                Auctions[auction.SupplierId, swapAuction.ConsumerId].IsBase = false;
            else
                Auctions[swapAuction.SupplierId, auction.ConsumerId].IsBase = false;

            return true;
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
