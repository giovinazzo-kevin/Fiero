namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public class ReplaceIntentEventResult : EventResult
        {
            public readonly int Priority;
            public readonly IAction NewIntent;

            public ReplaceIntentEventResult() : base(false) { }
            public ReplaceIntentEventResult(IAction newIntent, int priority = 0)
                : base(true)
            {
                NewIntent = newIntent;
                Priority = priority;
            }
        }
    }
}
