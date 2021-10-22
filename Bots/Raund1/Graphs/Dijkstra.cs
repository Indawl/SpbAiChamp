using System.Collections.Generic;
using System.Linq;
using SpbAiChamp.Bots.Raund1.Collections;

namespace SpbAiChamp.Bots.Raund1.Graphs
{
    public class Dijkstra
    {
        public Node endNode, currentNode, neighbor;

        public Dictionary<Node, int> costAsFar = new Dictionary<Node, int>();
        public Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();

        public Dijkstra(Graph graph, int start)
        {
            FastPriorityQueue<Node> frontier = new FastPriorityQueue<Node>(graph.edges.Count());

            var node = new Node(start);

            frontier.Enqueue(node, 0);
            cameFrom[node] = null;
            costAsFar[node] = 0;

            while (frontier.Count > 0)
            {
                currentNode = frontier.Dequeue();

                var neighbors = graph.edges[currentNode];
                for (int i = 0; i < neighbors.Count; i++)
                {
                    neighbor = neighbors[i].toNode;
                    var newCost = costAsFar[currentNode] + GetCost(neighbors[i]);

                    if (!costAsFar.ContainsKey(neighbor) || newCost < costAsFar[neighbor])
                    {
                        costAsFar[neighbor] = newCost;
                        if (frontier.Contains(neighbor)) frontier.UpdatePriority(neighbor, newCost);
                        else frontier.Enqueue(neighbor, newCost);
                        cameFrom[neighbor] = currentNode;
                    }
                }
            }
        }

        protected virtual int GetCost(Edge edge) => edge.cost;

        protected Node GetNextNode(Node node)
        {
            while (cameFrom[node] != null)
                node = cameFrom[node];

            return node;
        }
    }
}
