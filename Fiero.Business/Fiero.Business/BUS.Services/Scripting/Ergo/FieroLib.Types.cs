using Ergo.Interpreter.Directives;
using Ergo.Interpreter.Libraries;
using Ergo.Lang.Ast;
using Ergo.Runtime.BuiltIns;

namespace Fiero.Business;
public partial class FieroLib
{
    public static class Types
    {
        public const string Entity = nameof(Entity);
        public const string EntityType = nameof(EntityType);
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
        public static readonly Atom Effect = new("effect");
        public static readonly Atom Data = new("data");
        public static readonly Atom Event = new("event");
        public static readonly Atom Random = new("random");
    }
}

public partial class FieroLib : Library
{
    public override Atom Module => Modules.Fiero;
    public override IEnumerable<InterpreterDirective> GetExportedDirectives()
    {
        yield break;
    }
    public override IEnumerable<BuiltIn> GetExportedBuiltins()
    {
        yield break;
    }
}
