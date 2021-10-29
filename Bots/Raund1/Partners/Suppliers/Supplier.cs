using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Managment;
using SpbAiChamp.Bots.Raund1.Partners.Consumers;

namespace SpbAiChamp.Bots.Raund1.Partners.Suppliers
{
    public class Supplier : Partner
    {
        public int Number { get; set; }
        public int? Potential { get; set; }
        public int countBase { get; set; }

        public Supplier(int planetId, int number, Resource? resource = null, int delay = 0) :
            base(planetId, number, resource, delay)
        {
            Number = number;
        }

        public virtual int CalculateCost(Consumer consumer) => Manager.CurrentManager.PlanetDetails[consumer.PlanetId].getTransportCost(PlanetId);
    }
}
