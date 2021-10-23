using System.Collections.Generic;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Contracts;
using SpbAiChamp.Bots.Raund1.Partners;

namespace SpbAiChamp.Bots.Raund1.Logistics
{
    public class ShippingPlan : Shipping
    {
        public int MaxCost { get; private set; } = 1000000; // TO DO: fix value

        public int SupplierId { get; }
        public int ConsumerId { get; }

        public bool IsBase { get; set; } = false;
        public int Delta { get; set; } = 0;
        public int Sign { get; set; } = 0;
        public int countBase { get; set; } = 0;

        public ShippingPlan(int supplierId, int consumerId, Supplier supplier, Consumer consumer) : base(supplier, consumer)
        {
            SupplierId = supplierId;
            ConsumerId = consumerId;
        }

        public void GetAction(List<MoveAction> moveActions, List<BuildingAction> buildingActions)
        {
            if (!IsBase || Cost > MaxCost || Number == 0) return;

            if (Supplier.PlanetId != Consumer.PlanetId)
                if (Consumer.Type == ConsumerType.Supplier)
                    moveActions.Add(new MoveAction(Consumer.PlanetId, Supplier.PlanetId, Number, null));
                else
                    moveActions.Add(new MoveAction(Supplier.PlanetId, Consumer.PlanetId, Number, Supplier.Resource));

            Consumer.GetAction(moveActions, buildingActions);
        }

        protected override int CalculateCost()
        {
            return base.CalculateCost();
        }        
    }
}
