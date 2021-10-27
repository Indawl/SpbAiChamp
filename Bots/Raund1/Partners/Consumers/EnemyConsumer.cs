using SpbAiChamp.Bots.Raund1.Partners.Suppliers;

namespace SpbAiChamp.Bots.Raund1.Partners.Consumers
{
    public class EnemyConsumer : Consumer
    {
        public EnemyConsumer(int planetId, int number, int delay = 0) : 
            base(planetId, number, null, delay)
        {
        }

        public override int CalculateCost(Supplier supplier)
        {
            return int.MaxValue;
        }
    }
}
