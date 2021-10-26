using SpbAiChamp.Model;
using System.Collections.Generic;

namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class EnemyConsumer : Consumer
    {
        public EnemyConsumer(int planetId, int number, int delay = 0) : 
            base(planetId, number, null, delay)
        {
        }

        public override int CalculateCost(Supplier supplier)
        {
            return base.CalculateCost(supplier);
        }

        public override void GetAction(Supplier supplier, int number, List<MoveAction> moveActions, List<BuildingAction> buildingActions)
        {
            base.GetAction(supplier, number, moveActions, buildingActions);
        }
    }
}
