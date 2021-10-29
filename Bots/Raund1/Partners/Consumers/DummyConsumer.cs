using System.Collections.Generic;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Partners.Suppliers;

namespace SpbAiChamp.Bots.Raund1.Partners.Consumers
{
    public class DummyConsumer : Consumer
    {
        public DummyConsumer(int number, Resource? resource = null, Supplier supplier = null) : 
            base(0, number, resource, 0, supplier)
        {
            IsFake = true;
        }

        public override void GetAction(Supplier supplier, int number, List<MoveAction> moveActions, List<BuildingAction> buildingActions) { }

        public override int CalculateCost(Supplier supplier) => 1;
    }
}
