using System.Collections.Generic;
using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class Consumer : Partner
    {
        public int? Potential { get; set; }
        public int countBase { get; set; }

        public Consumer(int planetId, int number, Resource? resource = null, int delay = 0, bool isDummy = false) : 
            base(planetId, number, resource, delay, isDummy)
        {
        }

        public virtual void GetAction(List<MoveAction> moveActions, List<BuildingAction> buildingActions) { }
    }
}
