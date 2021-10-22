using System;

namespace SpbAiChamp.Bots.Raund1.Collections
{
    internal interface IFixedSizePriorityQueue<TItem, in TPriority> : IPriorityQueue<TItem, TPriority>
        where TPriority : IComparable<TPriority>
    {
        void Resize(int maxNodes);
        int MaxSize { get; }
        void ResetNode(TItem node);
    }
}
