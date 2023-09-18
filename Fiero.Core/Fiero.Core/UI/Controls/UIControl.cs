using SFML.Graphics;
using SFML.Window;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Fiero.Core
{
    public abstract partial class UIControl : Drawable, IDisposable
    {
        public readonly GameInput Input;
        public readonly ObservableCollection<UIControl> Children;
        public readonly IReadOnlyList<IUIControlProperty> Properties;

        public readonly UIControlProperty<bool> IsInteractive = new(nameof(IsInteractive), false) { Inherited = false };
        public readonly UIControlProperty<Coord> Snap = new(nameof(Snap), new(1, 1));
        public readonly UIControlProperty<Coord> Margin = new(nameof(Margin), new(), invalidate: true) { Inherited = false };
        public readonly UIControlProperty<Coord> Padding = new(nameof(Padding), new(), invalidate: true) { Inherited = false };
        public readonly UIControlProperty<Coord> Position = new(nameof(Position), new());
        public readonly UIControlProperty<Coord> Size = new(nameof(Size), new(), invalidate: true);
        public readonly UIControlProperty<Vec> Origin = new(nameof(Origin), new());
        public readonly UIControlProperty<Vec> Scale = new(nameof(Scale), new(1, 1), invalidate: true);
        public readonly UIControlProperty<Color> Foreground = new(nameof(Foreground), new(255, 255, 255), invalidate: true);
        public readonly UIControlProperty<Color> Background = new(nameof(Background), new(0, 0, 0, 0), invalidate: true);
        public readonly UIControlProperty<Color> Accent = new(nameof(Accent), new(255, 0, 0), invalidate: true);
        public readonly UIControlProperty<bool> IsHidden = new(nameof(IsHidden), false, invalidate: true) { Propagated = true };
        public readonly UIControlProperty<bool> IsActive = new(nameof(IsActive), false, invalidate: true) { Propagated = true };
        public readonly UIControlProperty<int> ZOrder = new(nameof(ZOrder), 0);
        public readonly UIControlProperty<Color> BorderColor = new(nameof(BorderColor), new(255, 255, 255), invalidate: true) { Inherited = false };
        public readonly UIControlProperty<int> OutlineThickness = new(nameof(OutlineThickness), 0, invalidate: true) { Inherited = false };
        public readonly UIControlProperty<ToolTip> ToolTip = new(nameof(ToolTip), null) { Inherited = false };

        public bool IsMouseOver { get; protected set; }


        public event Action<UIControl> Invalidated;
        protected bool IsDirty { get; set; } = true;


        private RenderTexture _target;
        private HashSet<UIControl> _redrawChildren = new();
        private TimeSpan _timeoutAcc;

        public void Invalidate(UIControl source = null)
        {
            Invalidated?.Invoke(source ?? this);
            if (source != null && Children.Contains(source))
            {
                // may crash, need a lock?
                _redrawChildren.Add(source);
            }
            else if (source == null || source == this)
            {
                IsDirty = true;
            }
        }

        public Coord BorderRenderPos { get; private set; }
        public Coord ContentRenderPos { get; private set; }
        public Coord BorderRenderSize { get; private set; }
        public Coord ContentRenderSize { get; private set; }
        protected Coord TrackedMousePosition { get; private set; }

        private Coord _minimumContentSize;
        public Coord MinimumContentSize
        {
            get => _minimumContentSize; protected set
            {
                var old = _minimumContentSize;
                _minimumContentSize = value;
                MinimumContentSizeChanged?.Invoke(this, old);
            }
        }

        public event Action<UIControl, Coord> MinimumContentSizeChanged;

        // Copies all matching and propagating properties from the given control to this control. Used when instantiating children.
        public void InheritProperties(UIControl from)
        {
            foreach (var fromProp in from.Properties)
            {
                if (Properties.SingleOrDefault(x => x.Name.Equals(fromProp.Name)) is { } myProp && fromProp.Inherited)
                {
                    myProp.Value = fromProp.Value;
                }
            }
        }

        protected virtual void RecomputeBoundaries()
        {
            var outline = new Coord(OutlineThickness.V, OutlineThickness.V);
            BorderRenderPos = (Position.V + Margin.V).Align(Snap);
            BorderRenderSize = ((Size.V - Margin.V * 2).Align(Snap));
            ContentRenderPos = (Position.V + Margin.V + Padding.V + outline).Align(Snap);
            ContentRenderSize = (Size.V - Margin.V * 2 - Padding.V * 2 - outline * 2).Align(Snap);
        }

        public UIControl(GameInput input)
        {
            Input = input;
            Children = new();

            Properties = GetType()
                .GetRuntimeFields()
                .Where(p => p.FieldType.GetInterface(nameof(IUIControlProperty)) != null)
                .Select(p => (IUIControlProperty)p.GetValue(this))
                .ToList();
            var registerInvalidationEvents = typeof(UIControl)
                .GetMethod(nameof(RegisterInvalidationEvents), BindingFlags.Instance | BindingFlags.NonPublic);

            Children.CollectionChanged += (sender, e) =>
            {
                if (e.OldItems != null)
                    foreach (UIControl i in e.OldItems)
                        i.Invalidated -= PropagateUpwards;
                if (e.NewItems != null)
                    foreach (UIControl i in e.NewItems)
                        i.Invalidated += PropagateUpwards;
                Invalidate();
                void PropagateUpwards(UIControl source)
                {
                    if (source != this && source != null)
                        Invalidate(source);
                }
            };

            Size.ValueChanged += (_, __) =>
            {
                _target?.Dispose();
                _target = null;
                var (texw, texh) = ((Size.V.X * Scale.V.X), (Size.V.Y * Scale.V.Y));
                if (texw > 0 && texh > 0)
                {
                    _target = new RenderTexture((uint)texw, (uint)texh);
                }
                Invalidate();
            };

            ToolTip.ValueChanged += (_, old) =>
            {
                old?.Close(default);
                if (ToolTip.V is { } newValue)
                {
                    newValue.Open(string.Empty);
                    newValue.Layout.IsHidden.V = !IsMouseOver;
                }
            };

            Position.ValueChanged += (_, __) => RecomputeBoundaries();
            Margin.ValueChanged += (_, __) => RecomputeBoundaries();
            Padding.ValueChanged += (_, __) => RecomputeBoundaries();
            OutlineThickness.ValueChanged += (_, __) => RecomputeBoundaries();
            Snap.ValueChanged += (_, __) => RecomputeBoundaries();
            Size.ValueChanged += (_, __) => RecomputeBoundaries();
            RecomputeBoundaries();
            foreach (var prop in Properties)
            {
                prop.SetOwner(this);
                if (prop.Invalidating)
                {
                    registerInvalidationEvents.MakeGenericMethod(prop.PropertyType)
                        .Invoke(this, new object[] { prop });
                }
            }
        }

        private void RegisterInvalidationEvents<T>(UIControlProperty<T> prop)
        {
            prop.ValueChanged += (_, __) => Invalidate();
        }

        /// <summary>
        /// Like Contains but goes in depth recursively if it finds a nested object.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public virtual IEnumerable<UIControl> HitTest(Coord point)
        {
            foreach (var contained in Contains(point))
            {
                if (contained is not UIWindowAsControl wnd)
                {
                    yield return contained;
                    continue;
                }
            }
        }

        public bool Intersects(UIControl other)
        {
            var a = new IntRect(BorderRenderPos.X, BorderRenderPos.Y, BorderRenderSize.X, BorderRenderSize.Y).Inflate(-1);
            var b = new IntRect(other.BorderRenderPos.X, other.BorderRenderPos.Y, other.BorderRenderSize.X, other.BorderRenderSize.Y).Inflate(-1);
            return a.Intersects(b);
        }

        public virtual IEnumerable<UIControl> Contains(Coord point)
        {
            if (point.X >= BorderRenderPos.X * Scale.V.X
                && point.X < BorderRenderPos.X * Scale.V.X + BorderRenderSize.X * Scale.V.X
                && point.Y >= BorderRenderPos.Y * Scale.V.Y
                && point.Y < BorderRenderPos.Y * Scale.V.Y + BorderRenderSize.Y * Scale.V.Y)
            {
                foreach (var child in Children
                    .Where(c => !c.IsHidden))
                {
                    foreach (var s in child.Contains(point))
                    {
                        yield return s;
                    }
                }
                yield return this;
            }
        }

        public bool Click(Coord mousePos, Mouse.Button button)
        {
            var preventDefault = Clicked?.Invoke(this, mousePos, button) ?? false;
            return preventDefault || OnClicked(mousePos, button);
        }

        protected bool TrackMouse(Coord mousePos, out UIControl clickedControl, out Mouse.Button clickedButton)
        {
            clickedButton = default;
            clickedControl = default;
            var wasInside = Contains(TrackedMousePosition).Any();
            var isInside = Contains(mousePos).Any();
            var leftClick = Input.IsButtonReleased(Mouse.Button.Left);
            var rightClick = Input.IsButtonReleased(Mouse.Button.Right);
            var click = leftClick || rightClick;
            var preventDefault = false;
            if (!wasInside && isInside)
            {
                MouseEntered?.Invoke(this, mousePos);
                OnMouseEntered(mousePos);
                if (!click)
                {
                    foreach (var child in Children)
                    {
                        child.TrackMouse(mousePos, out _, out _);
                    }
                }
            }
            else if (wasInside && !isInside)
            {
                MouseLeft?.Invoke(this, mousePos);
                OnMouseLeft(mousePos);
                if (!click)
                {
                    foreach (var child in Children)
                    {
                        child.TrackMouse(mousePos, out _, out _);
                    }
                }
            }
            else if (wasInside && isInside)
            {
                MouseMoved?.Invoke(this, mousePos);
                OnMouseMoved(mousePos);
                if (!click)
                {
                    foreach (var child in Children)
                    {
                        child.TrackMouse(mousePos, out _, out _);
                    }
                }
            }
            if (isInside && click)
            {
                foreach (var con in Contains(mousePos)
                        .Where(con => con.IsInteractive.V))
                {
                    clickedControl = con;
                    break;
                }
                clickedControl ??= this;
                clickedButton = leftClick ? Mouse.Button.Left : Mouse.Button.Right;
                preventDefault = clickedControl.Click(mousePos, clickedButton);
            }
            TrackedMousePosition = mousePos;
            return preventDefault;
        }

        public virtual void Update(TimeSpan t, TimeSpan dt)
        {
            if (ToolTip.V is { DisplayTimeout: var timeout } tooltip)
            {
                if (IsMouseOver)
                {
                    // Show a hidden tooltip after a set timeout
                    if (tooltip.Layout.IsHidden && (_timeoutAcc += dt) > timeout)
                    {
                        _timeoutAcc = timeout;
                        tooltip.Layout.IsHidden.V = false;
                    }
                }
                else
                {
                    tooltip.Layout.IsHidden.V = true;
                    _timeoutAcc = TimeSpan.Zero;
                }
                tooltip.Update(t, dt); // Tracks mouse position
            }
            foreach (var child in Children)
            {
                child.Update(t, dt);
            }
        }

        protected virtual void DrawBackground(RenderTarget target, RenderStates states)
        {
            var outline = new Coord(OutlineThickness.V, OutlineThickness.V);
            var rect = new RectangleShape((BorderRenderSize - outline * 2).ToVector2f())
            {
                Position = (BorderRenderPos + outline).ToVector2f(),
                FillColor = Background,
                OutlineThickness = OutlineThickness,
                OutlineColor = BorderColor
            };
            target.Draw(rect, states);
        }

        protected virtual void Repaint(RenderTarget target, RenderStates states)
        {
            DrawBackground(target, states);
            foreach (var child in Children.OrderByDescending(x => x.ZOrder.V).ThenByDescending(x => x.IsActive.V ? 0 : 1))
            {
                child.Repaint(target, states);
            }
        }

        protected virtual void PostDraw(RenderTarget target, RenderStates states)
        {
            if (ToolTip.V is { } tooltip)
                tooltip.Draw(target, states);
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            if (IsHidden)
                return;
            if (_target is null) return;
            var innerStates = RenderStates.Default;
            innerStates.Transform.Translate((Position.V * Coord.NegativeOne).ToVector2f());
            if (IsDirty)
            {
                IsDirty = false;
                _target.Clear(Color.Transparent);
                Repaint(_target, innerStates);
                foreach (var child in Children)
                    child.IsDirty = false;
                _redrawChildren.Clear();
            }
            if (_redrawChildren.Count > 0)
            {
                var toRepaintAnyway = Children.Except(_redrawChildren)
                    .Where(otherChild => _redrawChildren.Any(rd => rd.Intersects(otherChild)));
                var drawList = _redrawChildren
                    .Concat(toRepaintAnyway)
                    .OrderByDescending(x => x.ZOrder.V)
                    .ThenByDescending(x => x.IsActive.V ? 0 : 1)
                    .ThenBy(Children.IndexOf) // Preserves the natural ordering where z-order is implicit
                    .ToArray();
                // If a child has a semitransparent background, we need to physically erase its shape from the texture
                // There's no need to do this for opaque objects
                foreach (var child in drawList
                    .Where(c => c.Background.V.A < 255))
                {
                    using var rect = new RectangleShape(child.BorderRenderSize)
                    {
                        FillColor = Color.Transparent,
                        Position = (child.BorderRenderPos - BorderRenderPos).ToVector2f()
                    };
                    var eraser = RenderStates.Default;
                    eraser.BlendMode = BlendMode.None;
                    _target.Draw(rect, eraser);
                }
                foreach (var child in drawList)
                {
                    child.Repaint(_target, innerStates);
                    child.IsDirty = false;
                }
                _redrawChildren.Clear();
            }
            _target.Display();
            using var sprite = new Sprite(_target.Texture) { Position = Position.V };
            target.Draw(sprite, states);
            PostDraw(target, states);
            foreach (var c in Children)
            {
                c.PostDraw(target, states);
            }
        }

        public virtual void Dispose() { }

    }
}
