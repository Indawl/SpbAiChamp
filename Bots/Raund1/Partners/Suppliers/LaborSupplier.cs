namespace SpbAiChamp.Bots.Raund1.Partners.Suppliers
{
    public class LaborSupplier : Supplier
    {
        public LaborSupplier(int planetId, int number, int delay = 0) :
            base(planetId, number, null, delay)
        {
            IsInitialAction = true;
        }
    }
}
