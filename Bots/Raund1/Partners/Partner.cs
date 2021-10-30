using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class Partner
    {
#if MYDEBUG
        public static int MaxId = 0;
        public int Id = ++MaxId;
#endif
        public int PlanetId { get; protected set; }
        public int Quantity { get; protected set; }
        public Resource? Resource { get; protected set; }
        public int Delay { get; protected set; }

        public int Number { get; set; }

        public int? Potential { get; set; }
        public int countBase { get; set; }

        public bool IsFake { get; protected set; } = false;
                
        public Partner(int planetId, int number, Resource? resource = null, int delay = 0)
        {
            PlanetId = planetId;
            Quantity = Number = number;
            Resource = resource;
            Delay = delay;
        }

        public override string ToString() => "<" + GetType().Name +  "> Id: " + PlanetId +"; N: " + Quantity + "; R: " + Resource + "; T: " + Delay;
    }
}
