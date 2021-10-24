namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class LaborConsumer : Consumer
    {
        public Supplier Supplier { get; }

        public LaborConsumer(int planetId, int number, int delay = 0, Supplier supplier = null, bool isDummy = false) :
            base(planetId, number, null, delay, isDummy)
        {
            Supplier = supplier;
        }
    }
}
