using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class BuildingConsumer : Consumer
    {
        public BuildingConsumer(int planetId, int number, Resource? resource = null, int delay = 0) :
            base(ConsumerType.Consumer, planetId, number, resource, delay)
        {
        }
    }
}
