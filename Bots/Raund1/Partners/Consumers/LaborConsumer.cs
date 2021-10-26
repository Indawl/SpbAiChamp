using SpbAiChamp.Bots.Raund1.Partners.Suppliers;

namespace SpbAiChamp.Bots.Raund1.Partners.Consumers
{
    public class LaborConsumer : Consumer
    {
        public LaborConsumer(int planetId, int number, int delay = 0, Supplier supplier = null) :
            base(planetId, number, null, delay, supplier)
        {
        }
    }
}
