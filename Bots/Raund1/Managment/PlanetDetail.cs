using SpbAiChamp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpbAiChamp.Bots.Raund1.Managment
{
    public class PlanetDetail
    {
        public Planet Planet { get; }

        public PlanetDetail(Planet planet)
        {
            Planet = planet;
        }
    }
}
