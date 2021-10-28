using System;
using System.Collections.Generic;
using System.Linq;

using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Managment
{
    public class GameLog
    {
        public GameLog OldGameLog { get; set; }

        public Dictionary<int, Building?> planetInfos = new Dictionary<int, Building?>();

        public GameLog() { }
        public GameLog(GameLog gameLog)
        {
            OldGameLog = gameLog;

            if (Manager.CurrentManager.MyPlanets != null)
                foreach (var planet in Manager.CurrentManager.MyPlanets.Values
                    .Where(_ => _.Planet.Building.HasValue
                             || Manager.CurrentManager.Orders[_.Planet.Id].BuildingType.HasValue))
                    planetInfos[planet.Planet.Id] = planet.Planet.Building;
        }

        public bool IsChanged => !planetInfos.SequenceEqual(OldGameLog.planetInfos);

        public override string ToString()
        {
            var name = string.Empty;
            var changes = planetInfos.Intersect(OldGameLog.planetInfos);

            foreach (var change in changes)
                name += string.Format("Planet {0} {1} ", change.Key, change.Value);

            return "Nothing";
        }
    }
}
