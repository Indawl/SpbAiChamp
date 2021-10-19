using System;
using System.Collections.Generic;
using System.Linq;
using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1
{
    public class TransportCard
    {
        public List<Supplier> Suppliers { get; set; }
        public List<Consumer> Consumers { get; set; }

        public Auction[][] Auctions { get; set; }

        public TransportCard(List<Supplier> suppliers, List<Consumer> consumers)
        {
            Suppliers = suppliers;
            Consumers = consumers;
        }

        public void GetActions(List<MoveAction> moveActions, List<BuildingAction> buildingActions)
        {
            // Create road map...hmm
            CreateRoadMap();

            // Unpack actions
            //for (int j = 0; j < Consumers.Count; j++)
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
            //                if (Auctions[i][j].Price.Number > 0)
            //                {
            //                    foreach (KeyValuePair<Resource, int> resource in Auctions[i][j].Price.Resources.Where(r => r.Value > 0))
            //                        moveActions.Add(new MoveAction(Suppliers[i].PlanetId, Consumers[j].PlanetId,
            //                                                       resource.Value, resource.Key));
            //                }
            //            }

            //TO DO: union action for flying group
        }

        private void CreateRoadMap()
        {
            // Suppliers price must be = Consumers price
            NormalizePartners();

            // Create Auctions
            CreateAuctions();

            // Get transportation plan
            CalculatePlan();

            // Find potencial
            while (!CalculatePotencial())
                SwapLoop();
        }

        private void NormalizePartners()
        {
            throw new NotImplementedException();
        }

        private void CreateAuctions()
        {
            //Auctions = new Auction[Suppliers.Count][];

            //for (int i = 0; i < Suppliers.Count; i++)
            //{
            //    Auctions[i] = new Auction[Consumers.Count];

            //    for (int j = 0; j < Consumers.Count; j++)
            //        Auctions[i][j] = new Auction(i, j, Suppliers[i], Consumers[j]);
            //}
        }

        private void CalculatePlan()
        {
            //List<Auction> auctions = new List<Auction>(Suppliers.Count * Consumers.Count);

            //for (int i = 0; i < Suppliers.Count; i++)
            //    for (int j = 0; j < Consumers.Count; j++)
            //        auctions.Add(Auctions[i][j]);

            //auctions.Sort((a, b) => a.Cost.CompareTo(b.Cost));

            //foreach (Auction auction in auctions)
            //    if (auction.Price == null)
            //        SetAuctionPrice(auction);

            //// Find basis
            //int count = Suppliers.Count + Consumers.Count - 1 - auctions.Count(_ => _.IsInit);

            //// Check m + n - 1
            //for (int i = 0; i < Suppliers.Count && count > 0; i++)
            //    for (int j = 0; j < Consumers.Count && count > 0; j++)
            //        if (!Auctions[i][j].IsInit)
            //        {
            //            Auctions[i][j].IsInit = true;
            //            count--;
            //        }
        }

        private void SetAuctionPrice(Auction auction)
        {
            //auction.Price = new Price(auction.Consumer.Price);
            //auction.Price.Union(auction.Supplier.Price);

            //auction.Supplier.Price -= auction.Price;
            //auction.Consumer.Price -= auction.Price;
        }

        private void SwapLoop()
        {
            throw new NotImplementedException();
        }

        private bool CalculatePotencial()
        {
            int[] sP = new int[Suppliers.Count];
            int[] cP = new int[Consumers.Count];
            int[,] delta = new int[Suppliers.Count, Consumers.Count];
            return true;
            
        }        
    }
}
