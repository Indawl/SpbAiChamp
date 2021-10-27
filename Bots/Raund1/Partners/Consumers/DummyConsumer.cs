using System.Collections.Generic;
using SpbAiChamp.Model;
using SpbAiChamp.Bots.Raund1.Partners.Suppliers;

namespace SpbAiChamp.Bots.Raund1.Partners.Consumers
{
    public class DummyConsumer : Consumer
    {
        public DummyConsumer(int number, Resource? resource = null) : 
            base(0, number, resource)
        {
        }

        public override int CalculateCost(Supplier supplier) => 0;

        public override void GetAction(Supplier supplier, int number, List<MoveAction> moveActions, List<BuildingAction> buildingActions) { }
    }
}
