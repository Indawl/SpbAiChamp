using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1
{
    public class Partner
    {
        public int PlanetId { get; private set; }
        public int Number { get; set; }
        public Resource? Resource { get; private set; }
        public int Delay { get; private set; }
        public bool IsDummy { get; private set; }

        public int? Potential { get; set; }

        public Partner(int planetId, int number, Resource? resource = null, int delay = 0, bool isDummy = false)
        {
            PlanetId = planetId;
            Number = number;
            Resource = resource;
            Delay = delay;
            IsDummy = isDummy;
        }

        public override string ToString() => "Id: " + PlanetId +"; N: " + Number + "; R:" + Resource + "; T: " + Delay + "; D: " + IsDummy;
    }
}
