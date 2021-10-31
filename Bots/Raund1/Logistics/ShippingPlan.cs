using System.Collections.Generic;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Contracts;
using SpbAiChamp.Bots.Raund1.Partners.Suppliers;
using SpbAiChamp.Bots.Raund1.Partners.Consumers;

namespace SpbAiChamp.Bots.Raund1.Logistics
{
    public class ShippingPlan : Shipping
    {
        public int MaxCost { get; private set; } = int.MaxValue / 2;

        public int SupplierId { get; }
        public int ConsumerId { get; }

        #region Potencial helper
        public bool IsBase { get; set; } = false;
        public int Delta { get; set; } = 0;
        #endregion

        #region Cycle helper
        public bool Processed { get; set; }
        public bool Visited { get; set; }
        public bool? Direction { get; set; }
        public ShippingPlan FromShipping { get; set; }        
        #endregion

        public ShippingPlan(int supplierId, int consumerId, Supplier supplier, Consumer consumer) : base(supplier, consumer)
        {
            SupplierId = supplierId;
            ConsumerId = consumerId;

            Cost = CalculateCost();
        }

        public void GetAction(List<MoveAction> moveActions, List<BuildingAction> buildingActions)
        {
            if (Supplier.IsFake || Consumer.IsFake) return;
            if (Number == 0 || Cost >= MaxCost) return;

            Consumer.GetAction(Supplier, Number, moveActions, buildingActions);
        }

        private int CalculateCost() => Supplier.IsFake ? Supplier.CalculateCost(Consumer) : Consumer.CalculateCost(Supplier);

        public override string ToString() => string.Format("{0} -> {1}: {2} {3}", SupplierId, ConsumerId, Cost, Consumer);
    }
}
