﻿using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class ResourceConsumer : Consumer
    {
        public ResourceConsumer(int planetId, int number, Resource resource, int delay = 0) :
            base(planetId, number, resource, delay)
        {
        }

        public override int CalculateCost(Supplier supplier)
        {
            if (supplier.Resource.HasValue && (!Resource.HasValue || Resource.Value != supplier.Resource.Value))
                return int.MaxValue / 2;
            else return 1;
        }
    }
}
