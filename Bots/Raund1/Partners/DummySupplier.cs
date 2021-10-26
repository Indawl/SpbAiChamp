using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class DummySupplier : Supplier
    {
        public DummySupplier(int number, Resource? resource = null) :
            base(0, number, resource)
        {
        }

        public override int CalculateCost(Consumer consumer)
        {
            if (Resource.HasValue && (!consumer.Resource.HasValue || consumer.Resource.Value != Resource.Value))
                return int.MaxValue / 2;
            else return 0;
        }
    }
}
