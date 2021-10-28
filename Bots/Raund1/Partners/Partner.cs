using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class Partner
    {
#if MYDEBUG
        public static int MaxId = 0;
        public int Id = ++MaxId;
#endif
        public int PlanetId { get; private set; }
        public int Quantity { get; private set; }
        public Resource? Resource { get; private set; }
        public int Delay { get; private set; }
                
        public Partner(int planetId, int number, Resource? resource = null, int delay = 0)
        {
            PlanetId = planetId;
            Quantity = number;
            Resource = resource;
            Delay = delay;
        }

        public override string ToString() => "<" + GetType().Name +  "> Id: " + PlanetId +"; N: " + Quantity + "; R: " + Resource + "; T: " + Delay;
    }
}
