using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class DummyConsumer : Consumer
    {
        public DummyConsumer(int number, Resource? resource = null) : 
            base(0, number, resource)
        {
        }

        public override int CalculateCost(Supplier supplier)
        {
            if (supplier.Resource.HasValue && (!Resource.HasValue || Resource.Value != supplier.Resource.Value))
                return int.MaxValue / 2;
            else return 0;
        }
    }
}
