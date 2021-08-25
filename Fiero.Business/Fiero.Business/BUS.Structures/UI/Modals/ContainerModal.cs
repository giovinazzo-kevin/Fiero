using Fiero.Core;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public abstract class ContainerModal<TContainer, TActions> : Modal
        where TContainer : PhysicalEntity
        where TActions : struct, Enum
    {
        public readonly TContainer Container;
        public event Action<Item, TActions> ActionPerformed;


        protected readonly List<Item> Items = new();
        public UIControlProperty<int> CurrentPage { get; private set; } = new(nameof(CurrentPage), 0);
        public UIControlProperty<int> PageSize { get; private set; } = new(nameof(PageSize), 20);
        protected int NumPages => (Items.Count - 1) / PageSize.V + 1;

        public ContainerModal(GameUI ui, GameResources resources, TContainer cont) : base(ui, resources)
        {
            Container = cont;
            Items.AddRange(cont.Inventory?.GetItems() ?? Enumerable.Empty<Item>());
            CurrentPage.ValueChanged += (_, __) => Invalidate();
        }

        protected abstract bool ShouldRemoveItem(Item i, TActions a);

        protected override void OnWindowSizeChanged(GameDatumChangedEventArgs<Coord> obj)
        {
            var modalSize = UI.Store.Get(Data.UI.PopUpSize) * 2;
            Layout.Size.V = modalSize;
            Layout.Position.V = obj.NewValue / 2 - modalSize / 2;
        }

        protected override void BeforePresentation()
        {
            var windowSize = UI.Store.Get(Data.UI.WindowSize);
            OnWindowSizeChanged(new(Data.UI.WindowSize, windowSize, windowSize));
        }

        protected override void RegisterHotkeys(ModalWindowButton[] buttons)
        {
            base.RegisterHotkeys(buttons);
            Hotkeys.Add(new Hotkey(UI.Store.Get(Data.Hotkeys.Inventory)), () => Close(ModalWindowButton.ImplicitNo));
        }

        protected virtual bool OnItemClicked(Button b, int index, Mouse.Button mouseButton)
        {
            if (mouseButton != Mouse.Button.Left) {
                return false;
            }
            var i = CurrentPage.V * PageSize.V + index;
            if (i >= Items.Count) {
                return false;
            }
            var modal = UI.OptionalChoice(
                GetAvailableActions(Items[i]).Distinct().ToArray(),
                Items[i].DisplayName
            );
            modal.Confirmed += (_, __) => {
                ActionPerformed?.Invoke(Items[i], modal.SelectedOption);
                if(ShouldRemoveItem(Items[i], modal.SelectedOption)) {
                    Items.RemoveAt(i);
                }
                Invalidate();
            };
            Invalidate();
            return false;
        }

        protected abstract IEnumerable<TActions> GetAvailableActions(Item i);

        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => base.DefineStyles(builder)
            .AddRule<Picture>(s => s
                .Match(x => x.HasClass("item-sprite"))
                .Apply(x => {
                    x.HorizontalAlignment.V = HorizontalAlignment.Right;
                    x.LockAspectRatio.V = true;
                }))
            .AddRule<Button>(s => s
                .Match(x => x.HasClass("item-name"))
                .Apply(x => {
                    x.CenterContentH.V = false;
                    x.FontSize.V = 18;
                    x.Padding.V = new(16, 0);
                }))
            ;


        protected override LayoutGrid RenderContent(LayoutGrid layout)
        {
            return layout
                .Repeat(PageSize.V, (index, grid) => grid
                .Row(@class: index % 2 == 0 ? "row-even" : "row-odd")
                    .Col(w: 0.08f, @class: "item-sprite")
                        .Cell<Picture>(p => {
                            Invalidated += () => RefreshItemSprite(p, index);
                        })
                    .End()
                    .Col(w: 1.94f, @class: "item-name")
                        .Cell<Button>(b => {
                            Invalidated += () => RefreshItemButton(b, index);
                            b.Clicked += (_, __, button) => OnItemClicked(b, index, button);
                        })
                    .End()
                .End())
                .Row()
                    .Col(w: 0.25f, @class: "paginator paginator-prev")
                        .Cell<Button>(b => {
                            b.Text.V = "<";
                            b.Clicked += (_, __, ___) => {
                                CurrentPage.V = (CurrentPage.V - 1).Mod(NumPages);
                                return false;
                            };
                        })
                    .End()
                    .Col(w: 2.50f, @class: "paginator paginator-current")
                        .Cell<Label>(l => {
                            Invalidated += () => RefreshPageLabel(l);
                        })
                    .End()
                    .Col(w: 0.25f, @class: "paginator paginator-next")
                        .Cell<Button>(b => {
                            b.Text.V = ">";
                            b.Clicked += (_, __, ___) => {
                                CurrentPage.V = (CurrentPage.V + 1).Mod(NumPages);
                                return false;
                            };
                        })
                    .End()
                .End();

            void RefreshItemSprite(Picture p, int index)
            {
                var i = CurrentPage.V * PageSize.V + index;
                if (i >= Items.Count) {
                    p.Sprite.V = Resources.Sprites.Get(TextureName.Items, "None", ColorName.White);
                }
                else {
                    p.Sprite.V = Resources.Sprites.Get(TextureName.Items, Items[i].Render.Sprite, Items[i].Render.Color);
                }
            }

            void RefreshItemButton(Button b, int index)
            {
                var i = CurrentPage.V * PageSize.V + index;
                if (i >= Items.Count) {
                    b.Text.V = String.Empty;
                    return;
                }

                if(Container.TryCast<Actor>(out var actor)) {
                    b.Foreground.V = actor.Equipment.IsEquipped(Items[i])
                        ? UI.Store.Get(Data.UI.DefaultAccent)
                        : UI.Store.Get(Data.UI.DefaultForeground);
                }

                b.Text.V = Items[i].DisplayName;
            }

            void RefreshPageLabel(Label l)
            {
                l.Text.V = $"{CurrentPage.V + 1}/{NumPages}";
            }
        }
    }
}
