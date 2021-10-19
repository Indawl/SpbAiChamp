using SpbAiChamp.Bots;
using SpbAiChamp.Model;

namespace SpbAiChamp
{
    public class MyStrategy
    {
        private IBot Bot { get; } = new Bots.Raund1.Bot();
        public Action GetAction(Game game)
        {
            // Initialize new tick
            Bot.SetGame(game);

            // Get Action
            return Bot.GetAction();
        }
    }
}