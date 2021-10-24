using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class Supplier : Partner
    {
        public int? Potential { get; set; }
        public int countBase { get; set; }

        public Supplier(int planetId, int number, Resource? resource = null, int delay = 0, bool isDummy) :
            base(planetId, number, resource, delay, isDummy)
        {
        }
    }
}
