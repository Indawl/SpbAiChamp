namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class EnemyConsumer : Consumer
    {
        public EnemyConsumer(int planetId, int number, int delay = 0) : 
            base(ConsumerType.Consumer, planetId, number, null, delay)
        {
        }
    }
}
