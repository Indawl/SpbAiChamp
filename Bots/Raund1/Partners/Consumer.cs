using System.Collections.Generic;
using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class Consumer : Partner
    {
        public Supplier Supplier { get; }

        public int? Potential { get; set; }
        public int countBase { get; set; }

        public Consumer(int planetId, int number, Resource? resource = null, int delay = 0, Supplier supplier = null) : 
            base(planetId, number, resource, delay)
        {
            Supplier = supplier;
        }

        public virtual void GetAction(Supplier supplier, List<MoveAction> moveActions, List<BuildingAction> buildingActions) { }

        public virtual int CalculateCost(Supplier supplier) => 1;
    }
}
