using System.Diagnostics;
using SpbAiChamp.Bots;
using SpbAiChamp.Model;

namespace SpbAiChamp
{
    public class MyStrategy
    {
#if MYDEBUG
        public static Stopwatch watch = new Stopwatch();
#endif
        private IBot Bot { get; } = new Bots.Raund1.Bot();
        public Action GetAction(Game game)
        {
#if MYDEBUG
            watch.Restart();
#endif
            if (game.FlyingWorkerGroups.Length == game.MaxFlyingWorkerGroups)
                return new Action(new MoveAction[0], new BuildingAction[0], null);

            // Initialize new tick
            Bot.SetGame(game);

            // Get Action
            return Bot.GetAction();
        }
    }
}