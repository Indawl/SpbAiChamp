using SpbAiChamp.Model;
using System.Collections.Generic;

namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class DummyConsumer : Consumer
    {
        public DummyConsumer(int number, Resource? resource = null) : 
            base(0, number, resource)
        {
        }

        public override void GetAction(Supplier supplier, int number, List<MoveAction> moveActions, List<BuildingAction> buildingActions) { }
    }
}
