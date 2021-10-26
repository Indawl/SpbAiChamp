using System.Linq;
using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Managment
{
    public class ResourceDetail
    {
        public Resource Resource { get; }
        public int Number { get; } = 0;

        public BuildingType BuildingType { get; }

        public ResourceDetail(Resource resource)
        {
            Resource = resource;

            BuildingType = Manager.CurrentManager.Game.BuildingProperties
                .First(_ => _.Value.ProduceResource.HasValue && _.Value.ProduceResource.Value == resource).Key;

            Number = Manager.CurrentManager.Game.Planets.Sum(_ => _.Resources.ContainsKey(Resource) ? _.Resources[Resource] : 0);
        }
        
    }
}
