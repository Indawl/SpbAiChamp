using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Managment;
using SpbAiChamp.Bots.Raund1.Partners.Consumers;

namespace SpbAiChamp.Bots.Raund1.Partners.Suppliers
{
    public class Supplier : Partner
    {
        public Supplier(int planetId, int number, Resource? resource = null, int delay = 0) :
            base(planetId, number, resource, delay)
        {
        }

        public virtual int CalculateCost(Consumer consumer)
            => Resource.HasValue ? 0 : (int)Manager.CurrentManager.PlanetDetails[consumer.PlanetId].getTransportCost(PlanetId, Delay);
    }
}
