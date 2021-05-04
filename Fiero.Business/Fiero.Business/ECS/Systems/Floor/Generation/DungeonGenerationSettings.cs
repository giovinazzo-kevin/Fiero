namespace Fiero.Business
{
    public class DungeonGenerationSettings
    {
        public int NumRooms { get; set; }
        public int NumItemRooms { get; set; }
        public int NumBossRooms { get; set; }
        public int NumSecretRooms { get; set; }
        public int NumShopRooms { get; set; }

        public double InterconnectWeight { get; set; }
        public double RoomMergeChance { get; set; }
        public double PathRandomness { get; set; }

        public static readonly DungeonGenerationSettings Default = new() {
            NumRooms = 16,
            NumItemRooms = 1,
            NumShopRooms = 1,
            NumSecretRooms = 1,
            NumBossRooms = 1,

            PathRandomness = 1,
            InterconnectWeight = 0.5,
            RoomMergeChance = 0.5
        };
    }
}
