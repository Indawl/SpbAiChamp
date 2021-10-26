using SpbAiChamp.Bots.Raund1.Partners.Suppliers;
using SpbAiChamp.Bots.Raund1.Partners.Consumers;

namespace SpbAiChamp.Bots.Raund1.Contracts
{
    public class Shipping
    {
        public Supplier Supplier { get; }
        public Consumer Consumer { get; }

        public int Cost { get; private set; } = 0;
        public int Number { get; set; } = 0;

        public Shipping(Supplier supplier, Consumer consumer)
        {
            Supplier = supplier;
            Consumer = consumer;

            Cost = CalculateCost();
        }

        protected virtual int CalculateCost() => 0;
    }
}
