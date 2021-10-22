using System;
using SpbAiChamp.Bots.Raund1.Collections;

namespace SpbAiChamp.Bots.Raund1.Graphs
{
    public class Node : FastPriorityQueueNode, IEquatable<Node>
    {
        public int id;

        public Node(int planetId) => this.id = planetId;

        public bool Equals(Node other) => id == other.id;
        public override bool Equals(object obj) => Equals((Node)obj);
        public override int GetHashCode() => id;
    }
}
