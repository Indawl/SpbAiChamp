using System.Collections.Generic;

namespace SpbAiChamp.Bots.Raund1.Graphs
{
    public class Graph
    {
        public Dictionary<Node, List<Edge<Node>>> edges = new Dictionary<Node, List<Edge<Node>>>();
    }
}
