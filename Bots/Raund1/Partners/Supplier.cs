using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1
{
    public class Supplier : Partner
    {
        public Supplier(int planetId, int number, Resource? resource = null, int delay = 0, bool isDummy = false) :
            base(planetId, number, resource, delay, isDummy)
        {
        }
    }
}
