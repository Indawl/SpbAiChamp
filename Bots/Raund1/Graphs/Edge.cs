namespace SpbAiChamp.Bots.Raund1.Graphs
{
    public class Edge
    {
        public Node toNode;
        public int cost;

        public Edge(Node toNode, int cost = default)
        {
            this.cost = cost;
            this.toNode = toNode;
        }
    }
}
