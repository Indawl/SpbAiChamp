using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Partners.Consumers
{
    public class ResourceConsumer : Consumer
    {
        public ResourceConsumer(int planetId, int number, Resource resource, int delay = 0) :
            base(planetId, number, resource, delay)
        {
        }
    }
}
