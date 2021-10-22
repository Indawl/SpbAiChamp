using SpbAiChamp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpbAiChamp.Bots.Raund1.Managment
{
    public class BuildingDetail
    {
        public BuildingProperties BuildingProperties { get; }

        public BuildingDetail(BuildingProperties buildingProperties)
        {
            BuildingProperties = buildingProperties;
        }
    }
}
