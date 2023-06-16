namespace Fiero.Core
{
    public readonly struct LayoutRule
    {
        public readonly Type ControlType;
        public readonly Func<LayoutGrid, bool> Match;
        public readonly Action<UIControl> Apply;
        public readonly int Priority;

        public LayoutRule(Type type, Func<LayoutGrid, bool> match, Action<UIControl> apply, int priority)
        {
            ControlType = type;
            Match = match;
            Apply = apply;
            Priority = priority;
        }
    }
}
