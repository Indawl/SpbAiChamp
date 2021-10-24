using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class ResourceConsumer : Consumer
    {
        public ResourceConsumer(int planetId, int number, Resource resource, int delay = 0, bool isDummy = false) :
            base(planetId, number, resource, delay, isDummy)
        {
        }
    }
}
