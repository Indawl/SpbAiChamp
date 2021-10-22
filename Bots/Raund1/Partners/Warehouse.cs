using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class Warehouse : Partner
    {
        public Warehouse(int planetId, int number, Resource? resource = null, int delay = 0, bool isDummy = false) :
            base(planetId, number, resource, delay, isDummy)
        {
        }
    }
}
