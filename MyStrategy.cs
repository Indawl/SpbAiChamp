using SpbAiChamp.Bots;
using SpbAiChamp.Model;

namespace SpbAiChamp
{
    public class MyStrategy
    {
        private IBot Bot { get; } = new Bots.Raund1.Bot();
        public Action GetAction(Game game)
        {
            if (game.FlyingWorkerGroups.Length == game.MaxFlyingWorkerGroups)
                return new Action(new MoveAction[0], new BuildingAction[0], null);

            // Initialize new tick
            Bot.SetGame(game);

            // Get Action
            return Bot.GetAction();
        }
    }
}