using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Partners.Consumers;

namespace SpbAiChamp.Bots.Raund1.Partners.Suppliers
{
    public class DummySupplier : Supplier
    {
        public DummySupplier(int number, Resource? resource = null) :
            base(0, number, resource)
        {
            IsFake = true;
        }

        public override int CalculateCost(Consumer consumer) => 1;
    }
}
