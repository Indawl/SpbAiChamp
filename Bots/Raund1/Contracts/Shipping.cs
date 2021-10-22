namespace SpbAiChamp.Bots.Raund1.Contracts
{
    public class Shipping
    {
        public Partner Supplier { get; }
        public Partner Consumer { get; }

        public int Cost { get; private set; }
        public int Number { get; set; }        

        public Shipping(Partner supplier, Partner consumer)
        {
            Supplier = supplier;
            Consumer = consumer;

            Cost = CalculateCost(Supplier, Consumer);
        }

        public int CalculateCost(Partner supplier, Partner consumer)
        {
            return 1;
        }
    }
}
