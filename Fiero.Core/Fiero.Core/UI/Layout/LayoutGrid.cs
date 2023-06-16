namespace Fiero.Core;


public class LayoutGrid : IEnumerable<LayoutGrid>
{
    public record class CellControl(Type Type, Action<UIControl> Initialize)
    {
        public UIControl Instance { get; set; }
    }

    public readonly LayoutGrid Parent;
    protected readonly List<LayoutGrid> Children = new();

    private LayoutPoint _offset;
    public int Cols = 0, Rows = 0;
    public LayoutPoint Position { get; set; }
    public LayoutPoint Size { get; internal set; } = new(new(0, 1), new(0, 1));

    public Coord ComputedPosition { get; internal set; }
    public Coord ComputedSize { get; internal set; }

    public string Id { get; set; }
    public string Class { get; set; }
    public Coord Subdivisions => new(Cols, Rows);
    public bool IsCell => Cols == 0 && Rows == 0;
    public List<CellControl> Controls { get; private set; } = new();
    //public Type ControlType { get; protected set; } = typeof(Layout);
    //public UIControl ControlInstance { get; internal set; } = null;
    //internal Action<UIControl> InitializeControl { get; private set; } = null;
    protected List<LayoutRule> Styles { get; private set; }

    public bool Is<T>() => IsCell && Controls.Any(c => typeof(T).IsAssignableTo(c.Type));
    public bool HasClass(string cls) => Class != null && Class.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(cls);
    public bool HasAnyClass(params string[] cls) => Class != null && Class.Split(' ', StringSplitOptions.RemoveEmptyEntries).Intersect(cls).Any();
    public bool HasAllClasses(params string[] cls) => Class != null && Class.Split(' ', StringSplitOptions.RemoveEmptyEntries).Intersect(cls).Count() == cls.Length;

    public IEnumerable<UIControl> GetAllControlInstances()
    {
        foreach (var c in Controls)
            yield return c.Instance;
        foreach (var c in Children)
        {
            foreach (var control in c.GetAllControlInstances())
            {
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
            .Select<LayoutRule, Action<UIControl>>(s => control =>
            {
                if (Query(l => l.Controls.Any(c => c.Instance == control)).SingleOrDefault() is { } l && s.Match(l))
                {
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
        foreach (var c in Children)
        {
            foreach (var q in c.Query(pred))
            {
                yield return q;
            }
        }
    }

    public IEnumerable<Action<UIControl>> GetStyles(Type controlType)
    {
        return ((IEnumerable)typeof(LayoutGrid).GetMethod(nameof(GetStyles), 1, new Type[] { })
            .MakeGenericMethod(controlType)
            .Invoke(this, null))
            .Cast<Action<UIControl>>();
    }

    public LayoutGrid(LayoutPoint size, LayoutGrid parent = null)
    {
        Parent = parent;
        Styles = new List<LayoutRule>();
        Size = size;
    }

    public LayoutGrid Style<T>(Func<LayoutStyleBuilder<T>, LayoutStyleBuilder<T>> configure)
        where T : UIControl
    {
        var builder = configure(new LayoutStyleBuilder<T>());
        Styles.Add(builder.Build());
        return this;
    }

    public LayoutGrid Style(LayoutRule rule)
    {
        Styles.Add(rule);
        return this;
    }

    public LayoutGrid Top()
    {
        if (Parent == null)
            return this;
        var top = Parent;
        while (top.Parent != null)
        {
            top = top.Parent;
        }
        return top;
    }
    public LayoutGrid End() => Parent ?? this;
    public LayoutGrid Col(float w = 1, bool px = false, string @class = null, string @id = null)
    {
        var unit = LayoutUnit.FromBool(w, px);
        var ret = new LayoutGrid(size: new(unit, LayoutUnit.FromBool(0, false)), parent: this)
        {
            Class = @class == null ? Class : @class + " " + (Class ?? ""),
            Id = id,
            Position = _offset
        };
        _offset = _offset with { X = _offset.X + unit };
        Cols++;
        Children.Add(ret);
        return ret;
    }
    public LayoutGrid Row(float h = 1, bool px = false, string @class = null, string @id = null)
    {
        var unit = LayoutUnit.FromBool(h, px);
        var ret = new LayoutGrid(size: new(LayoutUnit.FromBool(0, false), unit), parent: this)
        {
            Class = @class == null ? Class : @class + " " + (Class ?? ""),
            Id = id,
            Position = _offset
        };
        _offset = _offset with { Y = _offset.Y + unit };
        Rows++;
        Children.Add(ret);
        return ret;
    }

    public LayoutGrid Cell<T>(Action<T> initialize = null)
        where T : UIControl
    {
        if (!IsCell)
        {
            throw new ArgumentException();
        }
        Controls.Add(new(typeof(T), x => initialize?.Invoke((T)x)));
        return this;
    }

    public LayoutGrid Cell<T>(T instance)
        where T : UIControl
    {
        if (!IsCell)
        {
            throw new ArgumentException();
        }
        Controls.Add(new(typeof(T), null) { Instance = instance });
        return this;
    }

    public LayoutGrid Repeat(int count, Func<int, LayoutGrid, LayoutGrid> action)
    {
        var ret = this;
        for (int i = 0; i < count; i++)
        {
            ret = action(i, ret);
        }
        return ret;
    }

    public IEnumerator<LayoutGrid> GetEnumerator() => Children.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Children).GetEnumerator();
}
