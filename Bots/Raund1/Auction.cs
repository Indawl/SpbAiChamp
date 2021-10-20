namespace SpbAiChamp.Bots.Raund1
{
    public class Auction
    {
        public int SupplierId { get; }
        public int ConsumerId { get; }
        public Supplier Supplier { get; }
        public Consumer Consumer { get; }

        public int Cost { get; private set; }
        public int Number { get; set; }
        public bool IsBase { get; set; } = false;

        public int Delta { get; set; }

        public Auction(int supplierId, int consumerId, Supplier supplier, Consumer consumer)
        {
            SupplierId = supplierId;
            ConsumerId = consumerId;

            Supplier = supplier;
            Consumer = consumer;

            Cost = CalculateCost(Supplier, Consumer);
        }

        public int CalculateCost(Supplier supplier, Consumer consumer)
        {
            return 1;
        }
    }
}
