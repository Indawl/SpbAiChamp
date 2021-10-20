using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1
{
    public class Consumer : Partner
    {
        public BuildingType? BuildingType { get; private set; }
        public Consumer(Planet planet, int number, Resource? resource = null, BuildingType? buildingType = null, int delay = 0, bool isDummy = false) : 
            base(planet, number, resource, delay, isDummy)
        {
            BuildingType = buildingType;
        }

        public override string ToString() => base.ToString() + "; B: " + BuildingType;
    }
}
