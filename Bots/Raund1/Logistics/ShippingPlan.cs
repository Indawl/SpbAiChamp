using System.Collections.Generic;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Contracts;
using SpbAiChamp.Bots.Raund1.Partners.Suppliers;
using SpbAiChamp.Bots.Raund1.Partners.Consumers;

namespace SpbAiChamp.Bots.Raund1.Logistics
{
    public class ShippingPlan : Shipping
    {
        public int MaxCost { get; private set; } = int.MaxValue; // TO DO: fix value

        public int SupplierId { get; }
        public int ConsumerId { get; }

        public bool Possible { get; }

        public bool IsBase { get; set; } = false;
        public int Delta { get; set; } = 0;
        public int countBase { get; set; } = 0;

        public int Sign { get; set; }

        public ShippingPlan(int supplierId, int consumerId, Supplier supplier, Consumer consumer) : base(supplier, consumer)
        {
            SupplierId = supplierId;
            ConsumerId = consumerId;

            Possible = Supplier.CheckConsumer(Consumer);
        }

        public void GetAction(List<MoveAction> moveActions, List<BuildingAction> buildingActions)
        {
            if (!IsBase || Cost > MaxCost || Number == 0) return;            

            if (Supplier.IsInitialAction)
                Consumer.GetAction(Supplier, Number, moveActions, buildingActions);
        }

        protected override int CalculateCost() => Possible ? (Supplier.CalculateCost(Consumer) + Consumer.CalculateCost(Supplier)) : int.MaxValue;
    }
}
