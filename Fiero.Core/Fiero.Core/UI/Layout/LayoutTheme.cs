using System.Collections.Immutable;

namespace Fiero.Core
{
    public struct LayoutTheme : IEnumerable<LayoutRule>
    {
        public readonly ImmutableArray<LayoutRule> Rules;
        public LayoutTheme()
        {
            Rules = ImmutableArray.Create<LayoutRule>();
        }
        private LayoutTheme(ImmutableArray<LayoutRule> rules) { Rules = rules; }
        public LayoutTheme(IEnumerable<LayoutRule> rules) { Rules = ImmutableArray.CreateRange(rules); }

        public LayoutTheme Style<T>(Func<LayoutRuleBuilder<T>, LayoutRuleBuilder<T>> configure)
            where T : UIControl
        {
            var builder = configure(new LayoutRuleBuilder<T>());
            return new(Rules.Add(builder.Build()));
        }

        public LayoutTheme Style(LayoutRule rule)
        {
            return new(Rules.Add(rule));
        }

        public IEnumerator<LayoutRule> GetEnumerator() => ((IEnumerable<LayoutRule>)Rules).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Rules).GetEnumerator();
    }
}
