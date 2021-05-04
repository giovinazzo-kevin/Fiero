using System.Collections.Immutable;

namespace Fiero.Business
{
    public class Dungeon
    {
        public readonly ImmutableHashSet<DungeonGenerationNode> Nodes;
        public readonly DungeonGenerationSettings GenerationSettings;

        public Dungeon(ImmutableHashSet<DungeonGenerationNode> nodes, DungeonGenerationSettings settings)
        {
            Nodes = nodes;
            GenerationSettings = settings;
        }
    }
}
