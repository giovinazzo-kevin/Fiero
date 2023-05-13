using Fiero.Core;

namespace Fiero.Business
{
    public class SecretCorridor : Corridor
    {
        public SecretCorridor(RoomConnector a, RoomConnector b, ColorName color) : base(a, b, color) { }
        protected override Chance DoorChance() => new(1, 1);
        protected override EntityBuilder<Feature> DoorFeature(GameEntityBuilders e, Coord c) => e.Feature_SecretDoor(Color);
    }
}
