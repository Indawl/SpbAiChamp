using SpbAiChamp.Model;

namespace SpbAiChamp.Bots
{
    public interface IBot
    {
        public void SetGame(Game game);
        public Action GetAction();        
    }
}
