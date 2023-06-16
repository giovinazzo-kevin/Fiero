using System.Collections.Immutable;

namespace Fiero.Core
{
    public class LayoutStyleBuilder
    {
        private readonly System.Collections.Immutable.ImmutableList<LayoutRule> _rules;

        public LayoutStyleBuilder(IEnumerable<LayoutRule> rules = null)
        {
            _rules = ImmutableList.CreateRange(rules ?? Enumerable.Empty<LayoutRule>());
        }

        public LayoutStyleBuilder AddRule<T>(Func<LayoutStyleBuilder<T>, LayoutStyleBuilder<T>> configure)
            where T : UIControl
        {
            var builder = configure(new());
            return new(_rules.Add(builder.Build()));
        }

        public IEnumerable<LayoutRule> Build() => _rules;
    }

    public class LayoutStyleBuilder<T>
        where T : UIControl
    {
        private Func<LayoutGrid, bool> _match;
        private Action<UIControl> _apply;
        private int _priority;

        public LayoutStyleBuilder()
        {
            _match = _ => true;
            _apply = _ => { };
            _priority = 0;
        }

        private LayoutStyleBuilder(Func<LayoutGrid, bool> match, Action<UIControl> apply, int priority)
        {
            _match = match;
            _apply = apply;
            _priority = priority;
        }

        public LayoutStyleBuilder<T> Match(Func<LayoutGrid, bool> match) => new(x => match(x) && _match(x), _apply, _priority);
        public LayoutStyleBuilder<T> Apply(Action<T> apply) => new(_match, x => { apply((T)x); _apply(x); }, _priority);
        public LayoutStyleBuilder<T> WithPriority(int newPriority) => new(_match, _apply, newPriority);
        public LayoutRule Build() => new(typeof(T), _match, _apply, _priority);
    }
}
