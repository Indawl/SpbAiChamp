﻿using SpbAiChamp.Model;

namespace SpbAiChamp.Bots.Raund1.Partners
{
    public class DummySupplier : Supplier
    {
        public DummySupplier(int number, Resource? resource = null) :
            base(0, number, resource)
        {
        }
    }
}
