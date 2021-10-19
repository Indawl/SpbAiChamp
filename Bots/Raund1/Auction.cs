namespace SpbAiChamp.Bots.Raund1
{
    public class Auction
    {
        public Supplier Supplier { get; }
        public Consumer Consumer { get; }

        public int Cost { get; private set; }
        public int Number { get; set; }
        public bool IsBase { get; set; } = false;

        public Auction(Supplier supplier, Consumer consumer)
        {
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
