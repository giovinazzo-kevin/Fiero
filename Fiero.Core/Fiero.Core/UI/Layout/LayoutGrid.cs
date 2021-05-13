using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Fiero.Core
{

    public class LayoutGrid : IEnumerable<LayoutGrid>
    {
        protected readonly LayoutGrid Parent;
        protected readonly List<LayoutGrid> Children = new();

        public int Cols = 0, Rows = 0;
        public Coord Size => new(Cols, Rows);
        public bool IsCell => Cols == 0 && Rows == 0;
        public Type ControlType { get; protected set; } = typeof(Layout);
        public UIControl ControlInstance { get; internal set; } = null;
        internal Action<UIControl> InitializeControl { get; private set; } = null;
        protected List<LayoutRule> Styles { get; private set; }

        public IEnumerable<Action<T>> GetStyles<T>()
            where T : UIControl
        {
            var myStyles = Styles
                .Where(x => x.ControlType.IsAssignableFrom(typeof(T)))
                .OrderByDescending(x => x.Priority)
                .Select(s => s.Apply)
                .ToList();
            var parentStyles = Parent?.GetStyles<T>() ?? Enumerable.Empty<Action<T>>();
            return myStyles.Concat(parentStyles);
        }

        public IEnumerable<Action<UIControl>> GetStyles(Type controlType)
        {
            return ((IEnumerable)typeof(LayoutGrid).GetMethod(nameof(GetStyles), 1, new Type[] {  })
                .MakeGenericMethod(controlType)
                .Invoke(this, null))
                .Cast<Action<UIControl>>();
        }

        public LayoutGrid(LayoutGrid parent = null)
        {
            Parent = parent;
            Styles = new List<LayoutRule>();
        }
        public LayoutGrid Rule<T>(Action<T> configure, int priority = 0)
            where T : UIControl
        {
            Styles.Add(new LayoutRule(typeof(T), t => configure((T)t), priority));
            return this;
        }
        public LayoutGrid Top()
        {
            if (Parent == null)
                return this;
            var top = Parent;
            while(top.Parent != null) {
                top = top.Parent;
            }
            return top;
        }
        public LayoutGrid End() => Parent ?? this;
        public LayoutGrid Col()
        {
            Cols++;
            var ret = new LayoutGrid(this);
            Children.Add(ret);
            return ret;
        }
        public LayoutGrid Col(Func<LayoutGrid, LayoutGrid> configure)
        {
            Cols++;
            var child = configure(new LayoutGrid(this));
            Children.Add(child);
            return child;
        }
        public LayoutGrid Row()
        {
            Rows++;
            var ret = new LayoutGrid(this);
            Children.Add(ret);
            return ret;
        }
        public LayoutGrid Row(Func<LayoutGrid, LayoutGrid> configure)
        {
            Rows++;
            var ret = configure(new LayoutGrid(this));
            Children.Add(ret);
            return ret;
        }

        public LayoutGrid Cell<T>(Action<T> initialize = null)
            where T : UIControl
        {
            if(!IsCell) {
                throw new ArgumentException();
            }
            ControlType = typeof(T);
            InitializeControl = x => initialize((T)x);
            return this;
        }

        public IEnumerator<LayoutGrid> GetEnumerator() => Children.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Children).GetEnumerator();
    }
}
