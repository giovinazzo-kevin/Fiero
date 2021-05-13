using System;
using System.Reflection.Metadata;

namespace Fiero.Core
{
    public readonly struct LayoutRule
    {
        public readonly Type ControlType;
        public readonly Action<UIControl> Apply;
        public readonly int Priority;

        public LayoutRule(Type type, Action<UIControl> apply, int priority)
        {
            ControlType = type;
            Apply = apply;
            Priority = priority;
        }
    }
}
