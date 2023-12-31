using Ergo.Lang;
using Ergo.Lang.Ast;
using Unconcern.Common;

namespace Fiero.Business
{
    public class ScriptEffect(FieroScript script, string args) : Effect
    {
        private static int _id;
        public readonly int Id = Interlocked.Increment(ref _id);

        public readonly FieroScript Script = script;
        public override EffectName Name => EffectName.Script;
        public override string DisplayName => Script.Name;
        public override string DisplayDescription => Script.Name;
        public readonly ITerm Arguments = script.VM.KB.Scope.Parse<ITerm>(args).GetOr(WellKnown.Literals.Discard);


        [Term(Marshalling = TermMarshalling.Named)]
        public readonly record struct EffectBeganEvent(int EffectId, Entity Owner, ITerm Arguments);
        [Term(Marshalling = TermMarshalling.Named)]
        public readonly record struct EffectEndedEvent(int EffectId);

        protected override void OnStarted(MetaSystem systems, Entity owner)
        {
            base.OnStarted(systems, owner);
            Script.EffectBeganHook.SetArg(0, TermMarshall.ToTerm(new EffectBeganEvent(Id, owner, Arguments)));
            // Define a temporary virtual predicate end/1 that lets us end this specific effect instance.
            // Calling end(_) will therefore end all instances of an effect!
            var endHead = new Complex(new Atom("end"), new Atom(Id));
            var endPred = Predicate.FromOp(ErgoModules.Effect, endHead, vm =>
            {
                End(systems, owner);
                vm.KB.Retract(endHead);
            }, dynamic: true);
            Script.VM.KB.AssertZ(endPred);
            var ctx = Script.VM.ScopedInstance();
            ctx.Query = Script.EffectBeganOp;
            ctx.Run();
        }

        protected override void OnEnded(MetaSystem systems, Entity owner)
        {
            base.OnEnded(systems, owner);
            Script.EffectEndedHook.SetArg(0, TermMarshall.ToTerm(new EffectEndedEvent(Id)));
            var ctx = Script.VM.ScopedInstance();
            ctx.Query = Script.EffectEndedOp;
            ctx.Run();
        }

        protected override IEnumerable<Subscription> RouteEvents(MetaSystem systems, Entity owner)
        {
            yield break;
        }
    }
}
