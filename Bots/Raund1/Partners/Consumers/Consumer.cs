﻿using System.Collections.Generic;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Managment;
using SpbAiChamp.Bots.Raund1.Partners.Suppliers;

namespace SpbAiChamp.Bots.Raund1.Partners.Consumers
{
    public class Consumer : Partner
    {
        public int Number { get; set; }
        public Supplier Supplier { get; protected set; }

        public int? Potential { get; set; }
        public int countBase { get; set; }

        public Consumer(int planetId, int number, Resource? resource = null, int delay = 0, Supplier supplier = null) :
            base(planetId, number, resource, delay)
        {
            Number = number;
            Supplier = supplier;
        }

        public virtual void GetAction(Supplier supplier, int number, List<MoveAction> moveActions, List<BuildingAction> buildingActions)
        {
            if (supplier.PlanetId != PlanetId)
                moveActions.Add(new MoveAction(supplier.PlanetId, Manager.CurrentManager.PlanetDetails[PlanetId].ShortestWay.GetNextPlanetInv(supplier.PlanetId), number, supplier.Resource));
            else if (Supplier != null)
            {
                int supplierId = Manager.CurrentManager.TransportTask.Suppliers.IndexOf(Supplier);

                for (int j = 0; j < Manager.CurrentManager.TransportTask.Consumers.Count; j++)
                    Manager.CurrentManager.TransportTask.ShippingPlans[supplierId, j].GetAction(moveActions, buildingActions);
            }
        }

        public virtual int CalculateCost(Supplier supplier) => 0;
    }
}
