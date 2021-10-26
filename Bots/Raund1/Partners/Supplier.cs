using System.Collections.Generic;
using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class Supplier : Partner
    {
        public bool IsInitialAction { get; protected set; } = false;

        public int? Potential { get; set; }
        public int countBase { get; set; }

        public int NumberO;

        public Supplier(int planetId, int number, Resource? resource = null, int delay = 0) :
            base(planetId, number, resource, delay)
        {
            NumberO = number;
        }

        public virtual void GetAction(Consumer consumer, List<MoveAction> moveActions, List<BuildingAction> buildingActions) { }

        public virtual int CalculateCost(Consumer consumer) => 2;
    }
}
