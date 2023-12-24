using Ergo.Lang.Ast;

namespace Fiero.Business
{
    public partial class ScriptingSystem : EcsSystem
    {
        public static readonly Atom FieroModule = new("fiero");
        public static readonly Atom ScriptModule = new("script");
        public static readonly Atom AnimationModule = new("anim");
        public static readonly Atom SoundModule = new("sound");
        public static readonly Atom EffectModule = new("effect");
        public static readonly Atom DataModule = new("data");
        public static readonly Atom EventModule = new("event");
        public static readonly Atom RandomModule = new("random");
    }
}
