namespace Fiero.Business
{
    public class ItemComponent : EcsComponent
    {
        public int Rarity { get; set; }
        public int BuyValue { get; set; }
        public int SellValue => BuyValue / 2;
        public bool Identified { get; set; }
        public string UnidentifiedName { get; set; }
        public string ItemSprite { get; set; }
        public string OwnerTag { get; set; }
    }
}
