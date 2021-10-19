using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1
{
    public class Partner
    {
        public int PlanetId { get; private set; }
        public int Number { get; private set; }
        public Resource? Resource { get; private set; }
        public int Delay { get; private set; }
        public bool IsDummy { get; private set; }

        public Partner(int id, int number, Resource? resource = null, int delay = 0, bool isDummy = false)
        {
            PlanetId = id;
            Number = number;
            Resource = resource;
            Delay = delay;
            IsDummy = isDummy;
        }
    }
}
