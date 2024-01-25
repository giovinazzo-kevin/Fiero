namespace Fiero.Business
{
    public class ItemComponent : EcsComponent
    {
        public int Rarity { get; set; }
        public bool Identified { get; set; }
        public string UnidentifiedName { get; set; }
        public string ItemSprite { get; set; }
    }
}
