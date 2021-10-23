using SpbAiChamp.Bots.Raund1.Contracts;
using SpbAiChamp.Bots.Raund1.Partners;

namespace SpbAiChamp.Bots.Raund1.Logistics
{
    public class ShippingPlan : Shipping
    {      
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

        protected override int CalculateCost()
        {
            return base.CalculateCost();
        }
    }
}
