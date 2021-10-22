namespace SpbAiChamp.Bots.Raund1.Graphs
{
    public class Edge<TNode>
    {
        public TNode toNode;
        public int cost;

        public Edge(TNode toNode, int cost = default)
        {
            this.cost = cost;
            this.toNode = toNode;
        }
    }
}
