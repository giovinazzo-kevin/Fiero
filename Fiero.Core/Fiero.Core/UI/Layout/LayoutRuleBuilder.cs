namespace Fiero.Core
{
    public struct LayoutRuleBuilder<T>
        where T : UIControl
    {
        private Func<LayoutGrid, bool> _match;
        private Func<UIControl, bool> _where;
        private Action<UIControl> _apply;
        private int _priority;

        public LayoutRuleBuilder()
        {
            _match = _ => true;
            _where = _ => true;
            _apply = _ => { };
            _priority = 0;
        }

        private LayoutRuleBuilder(Func<LayoutGrid, bool> match, Func<UIControl, bool> where, Action<UIControl> apply, int priority)
        {
            _match = match;
            _apply = apply;
            _where = where;
            _priority = priority;
        }

        public LayoutRuleBuilder<T> Match(Func<LayoutGrid, bool> match)
        {
            var m = _match;
            return new(x => match(x) && m(x), _where, _apply, _priority); ;
        }
        public LayoutRuleBuilder<T> Filter(Func<UIControl, bool> where)
        {
            var w = _where;
            return new(_match, x => where(x) && w(x), _apply, _priority);
        }
        public LayoutRuleBuilder<T> Apply(Action<T> apply)
        {
            var a = _apply;
            return new(_match, _where, x => { apply((T)x); a(x); }, _priority);
        }
        public LayoutRuleBuilder<T> WithPriority(int newPriority) => new(_match, _where, _apply, newPriority);
        public LayoutRule Build()
        {
            var w = _where; var a = _apply;
            return new(typeof(T), _match, x => { if (w(x)) { a(x); } }, _priority);
        }
    }
}
