using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class LaborConsumer : Consumer
    {
        public LaborConsumer(ConsumerType type, int planetId, int number, Resource? resource = null, int delay = 0) : 
            base(type, planetId, number, resource, delay)
        {
        }
    }
}
