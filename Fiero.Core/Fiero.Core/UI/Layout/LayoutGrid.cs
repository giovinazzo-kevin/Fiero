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

        private float _c = 0, _r = 0;
        public int Cols = 0, Rows = 0;
        public float X = 0, Y = 0;
        public readonly float Width = 1, Height = 1;

        public string Id { get; set; }
        public string Class { get; set; }
        public Coord Subdivisions => new(Cols, Rows);
        public Vec Size => new(Width, Height);
        public Vec Position => new(X, Y);
        public bool IsCell => Cols == 0 && Rows == 0;
        public Type ControlType { get; protected set; } = typeof(Layout);
        public UIControl ControlInstance { get; internal set; } = null;
        internal Action<UIControl> InitializeControl { get; private set; } = null;
        protected List<LayoutRule> Styles { get; private set; }

        public bool Is<T>() => IsCell && ControlType.IsAssignableFrom(typeof(T));
        public bool HasClass(string cls) => Class != null && Class.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(cls);
        public bool HasAnyClass(params string[] cls) => Class != null && Class.Split(' ', StringSplitOptions.RemoveEmptyEntries).Intersect(cls).Any();
        public bool HasAllClasses(params string[] cls) => Class != null && Class.Split(' ', StringSplitOptions.RemoveEmptyEntries).Intersect(cls).Count() == cls.Length;

        public IEnumerable<UIControl> GetAllControlInstances()
        {
            if (ControlInstance != null)
                yield return ControlInstance;
            foreach (var c in Children) {
                foreach (var control in c.GetAllControlInstances()) {
                    yield return control;
                }
            }
        }

        public IEnumerable<Action<T>> GetStyles<T>()
            where T : UIControl
        {
            var myStyles = Styles
                .Where(x => x.ControlType.IsAssignableFrom(typeof(T)))
                .OrderByDescending(x => x.Priority)
                .Select<LayoutRule, Action<UIControl>>(s => control => {
                    if(Query(c => c.ControlInstance == control).FirstOrDefault() is { } c && s.Match(c)) {
                        s.Apply(control);
                    }
                })
                .ToList();
            var parentStyles = Parent?.GetStyles<T>() ?? Enumerable.Empty<Action<T>>();
            return myStyles.Concat(parentStyles);
        }

        public IEnumerable<LayoutGrid> Query(Func<LayoutGrid, bool> pred)
        {
            if (pred(this))
                yield return this;
            foreach (var c in Children) {
                foreach (var q in c.Query(pred)) {
                    yield return q;
                }
            }
        }

        public IEnumerable<Action<UIControl>> GetStyles(Type controlType)
        {
            return ((IEnumerable)typeof(LayoutGrid).GetMethod(nameof(GetStyles), 1, new Type[] {  })
                .MakeGenericMethod(controlType)
                .Invoke(this, null))
                .Cast<Action<UIControl>>();
        }

        public LayoutGrid(LayoutGrid parent = null, float w = 1, float h = 1)
        {
            Parent = parent;
            Styles = new List<LayoutRule>();
            Width = w;
            Height = h;
        }
        public LayoutGrid Style<T>(Action<T> configure, int priority = 0, Func<LayoutGrid, bool> match = null)
            where T : UIControl
        {
            Styles.Add(new LayoutRule(typeof(T), match ?? (g => true), t => configure((T)t), priority));
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
        public LayoutGrid Col(float w = 1, float h = 1, string @class = null, string @id = null)
        {
            var ret = new LayoutGrid(this, w, h) {
                Class = @class ?? Class,
                Id = id,
                X = _c,
                Y = _r
            };
            _c += w;
            Cols++;
            Children.Add(ret);
            return ret;
        }
        public LayoutGrid Row(float w = 1, float h = 1, string @class = null, string @id = null)
        {
            var ret = new LayoutGrid(this, w, h) {
                Class = @class ?? Class,
                Id = id,
                X = _c,
                Y = _r
            };
            _r += h;
            Rows++;
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
            InitializeControl = x => initialize?.Invoke((T)x);
            return this;
        }

        public LayoutGrid Repeat(int count, Func<int, LayoutGrid, LayoutGrid> action)
        {
            var ret = this;
            for (int i = 0; i < count; i++) {
                ret = action(i, ret);
            }
            return ret;
        }

        public IEnumerator<LayoutGrid> GetEnumerator() => Children.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Children).GetEnumerator();
    }
}
