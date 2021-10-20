using System.Linq;

namespace SpbAiChamp.Bots.Raund1
{
    public class Auction
    {
        public int SupplierId { get; }
        public int ConsumerId { get; }
        public Supplier Supplier { get; }
        public Consumer Consumer { get; }

        public int Cost { get; private set; }
        public int Number { get; set; }
        public bool IsBase { get; set; } = false;

        public int Delta { get; set; }

        public Auction(int supplierId, int consumerId, Supplier supplier, Consumer consumer)
        {
            SupplierId = supplierId;
            ConsumerId = consumerId;

            Supplier = supplier;
            Consumer = consumer;

            Cost = CalculateCost(Supplier, Consumer);
        }

        public int CalculateCost(Supplier supplier, Consumer consumer)
        {
            int price = 0;

            if (consumer.Resource.HasValue && consumer.Resource == supplier.Resource)
                price += Bot.Game.BuildingProperties[consumer.Planet.Building.Value.BuildingType].ProduceScore;



            //if (supplier.IsDummy || consumer.IsDummy) return 1000;
            //if (consumer.BuildingType.HasValue && Bot.Game.Planets.First(_ => _.Id == consumer.PlanetId).Building.HasValue) return 1000;
            return 1;
        }
    }
}
