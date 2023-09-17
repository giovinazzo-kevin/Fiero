using System.Collections.Immutable;

namespace Fiero.Core
{
    public class LayoutThemeBuilder
    {
        private readonly System.Collections.Immutable.ImmutableList<LayoutRule> _rules;

        public LayoutThemeBuilder(IEnumerable<LayoutRule> rules = null)
        {
            _rules = ImmutableList.CreateRange(rules ?? Enumerable.Empty<LayoutRule>());
        }

        public LayoutThemeBuilder Style<T>(Func<LayoutRuleBuilder<T>, LayoutRuleBuilder<T>> configure)
            where T : UIControl
        {
            var builder = configure(new());
            return new(_rules.Add(builder.Build()));
        }

        public LayoutTheme Build() => new(_rules);
    }
}
