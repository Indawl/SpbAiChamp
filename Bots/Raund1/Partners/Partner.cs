using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class Partner
    {
        public int PlanetId { get; private set; }
        public int Number { get; set; }
        public Resource? Resource { get; private set; }
        public int Delay { get; private set; }
        
        public Partner(int planetId, int number, Resource? resource = null, int delay = 0)
        {
            PlanetId = planetId;
            Number = number;
            Resource = resource;
            Delay = delay;
        }

        public override string ToString() => "Id: " + PlanetId +"; N: " + Number + "; R:" + Resource + "; T: " + Delay;
    }
}
