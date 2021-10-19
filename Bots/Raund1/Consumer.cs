using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1
{
    public class Consumer : Partner
    {
        public BuildingType? BuildingType { get; private set; }
        public Consumer(int id, int number, Resource? resource = null, BuildingType? buildingType = null, int delay = 0, bool isDummy = false) : 
            base(id, number, resource, delay, isDummy)
        {
            BuildingType = buildingType;
        }
    }
}
