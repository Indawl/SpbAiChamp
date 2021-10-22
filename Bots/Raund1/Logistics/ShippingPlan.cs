using SpbAiChamp.Bots.Raund1.Contracts;

namespace SpbAiChamp.Bots.Raund1.Logistics
{
    public class ShippingPlan : Shipping
    {      
        public bool IsBase { get; set; } = false;
        public int Delta { get; set; }

        public ShippingPlan(Supplier supplier, Consumer consumer) : base(supplier, consumer) { }
    }
}
