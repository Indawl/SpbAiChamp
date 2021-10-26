using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class Supplier : Partner
    {
        public bool IsInitialAction { get; protected set; } = false;

        public int Number { get; set; }
        public int? Potential { get; set; }
        public int countBase { get; set; }        

        public Supplier(int planetId, int number, Resource? resource = null, int delay = 0) :
            base(planetId, number, resource, delay)
        {
            Number = number;
        }
        public virtual int CalculateCost(Consumer consumer) => 0;
        public virtual bool CheckConsumer(Consumer consumer) => Resource == consumer.Resource;
    }
}
