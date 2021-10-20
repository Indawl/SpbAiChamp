using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1
{
    public class Partner
    {
        public Planet Planet { get; private set; }
        public int Number { get; set; }
        public Resource? Resource { get; private set; }
        public int Delay { get; private set; }
        public bool IsDummy { get; private set; }

        public int? Potential { get; set; }

        public Partner(Planet planet, int number, Resource? resource = null, int delay = 0, bool isDummy = false)
        {
            Planet = planet;
            Number = number;
            Resource = resource;
            Delay = delay;
            IsDummy = isDummy;
        }

        public override string ToString() => "Id: " + Planet.Id +"; N: " + Number + "; R:" + Resource + "; T: " + Delay + "; D: " + IsDummy;
    }
}
