namespace Fiero.Business
{
    [TransientDependency]
    public class IdleActionProvider : ActionProvider
    {
        public IdleActionProvider(MetaSystem sys) : base(sys)
        {
        }

        public override bool RequestDelay => false;

        public override bool TryTarget(Actor a, TargetingShape shape, bool autotargetSuccesful) => false;
        public override IAction GetIntent(Actor actor) => new WaitAction();

    }
}
