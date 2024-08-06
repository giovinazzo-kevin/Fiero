using Ergo.Lang.Ast;

namespace Fiero.Business;
public partial class FieroLib
{
    public static class Types
    {
        public const string Entity = nameof(Entity);
        public const string EntityType = nameof(EntityType);
        public const string Tile = nameof(Tile);
        public const string Coord = nameof(Coord);
        public const string Component = nameof(Component);
        public const string ComponentType = nameof(ComponentType);
        public const string ComponentProperty = nameof(ComponentProperty);
        public const string EntityID = nameof(EntityID);
    }
    public static class Modules
    {
        public static readonly Atom Fiero = new("fiero");
        public static readonly Atom Script = new("script");
        public static readonly Atom Animation = new("anim");
        public static readonly Atom Sound = new("sound");
        public static readonly Atom Entity = new("entity");
        public static readonly Atom Dialogue = new("dialogue");
        public static readonly Atom Effect = new("effect");
        public static readonly Atom Data = new("data");
        public static readonly Atom Event = new("event");
        public static readonly Atom Random = new("random");
        public static readonly Atom Map = new("map");
        public static readonly Atom MapGen = new("mapgen");
    }
}
