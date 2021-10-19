using SpbAiChamp.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpbAiChamp.Bots.Raund1
{
    public class Price
    {
        public IDictionary<Resource, int> Resources { get; set; } = new Dictionary<Resource, int>();
        public int Number { get; set; }

        public Price() { }

        public Price(Price price)
        {
            Resources = price.Resources.Keys.ToDictionary(_ => _, _ => price.Resources[_]);
            Number = price.Number;
        }

        public Price(int number, Resource? resource, int rNumber)
        {
            if (resource.HasValue)
                Resources.Add(resource.Value, rNumber);

            Number = number;
        }

        public Price(int number, IDictionary<Resource, int> resources = null)
        {
            if (resources != null)
                Resources = resources.Keys.ToDictionary(_ => _, _ => resources[_]);

            Number = number;
        }

        public bool IsInitial => Number == 0 && Resources.Values.All(_ => _ == 0);

        //public void Union(Price price)
        //{
        //    foreach (KeyValuePair<Resource, int> resource in Resources)
        //        if (price.Resources.TryGetValue(resource.Key, out int number))
        //            Resources[resource.Key] = Math.Min(resource.Value, number);

        //    Number = Resources.Values.Sum();
        //}

        //public static Price operator -(Price a, Price b)
        //{
        //    Price price = new Price(a);

        //    foreach (KeyValuePair<Resource, int> resource in b.Resources)
        //        if (a.Resources.TryGetValue(resource.Key, out int number))
        //            a.Resources[resource.Key] -= resource.Value;
        //        else
        //            a.Resources.Add(resource.Key, -resource.Value);

        //    price.Number = a.Resources.Values.Sum() - b.Resources.Values.Sum();

        //    return price;
        //}
    }
}
