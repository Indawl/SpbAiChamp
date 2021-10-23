using SpbAiChamp.Model;

namespace SpbAiChamp.Bots
{
    public interface IBot
    {
        public Action GetAction();
        public void SetGame(Game game);
    }
}
