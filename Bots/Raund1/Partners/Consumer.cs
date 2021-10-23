using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class Consumer : Partner
    {
        public int? Potential { get; set; }
        public int countBase { get; set; }

        public ConsumerType Type { get; }

        public Consumer(ConsumerType type, int planetId, int number, Resource? resource = null, int delay = 0) : 
            base(planetId, number, resource, delay)
        {
            Type = type;
        }

        public override string ToString() => Type.ToString() + ": " + base.ToString() + "; UV: " + Potential;
    }
}
