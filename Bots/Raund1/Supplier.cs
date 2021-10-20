using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1
{
    public class Supplier : Partner
    {
        public Supplier(Planet planet, int number, Resource? resource = null, int delay = 0, bool isDummy = false) :
            base(planet, number, resource, delay, isDummy)
        {
        }

        public Supplier(int planetId, int number, Resource? resource = null, int delay = 0, bool isDummy = false) :
            base(new Planet() { Id = planetId }, number, resource, delay, isDummy)
        {
        }
    }
}
