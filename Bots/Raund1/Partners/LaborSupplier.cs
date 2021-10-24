namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class LaborSupplier : Supplier
    {
        public LaborSupplier(int planetId, int number, int delay = 0, bool isDummy = false) :
            base(planetId, number, null, delay, isDummy)
        {
        }
    }
}
