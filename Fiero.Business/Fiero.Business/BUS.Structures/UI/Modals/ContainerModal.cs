﻿using Fiero.Core;
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
        public const int RowHeight = 32; // px
        public const int PaginatorHeight = 32; // px

        public readonly TContainer Container;
        public event Action<Item, TActions> ActionPerformed;


        protected readonly VirtualKeys[] Letters = new[]
        {
            VirtualKeys.N1, VirtualKeys.N2, VirtualKeys.N3, VirtualKeys.N4, VirtualKeys.N5, VirtualKeys.N6,
            VirtualKeys.N7, VirtualKeys.N8, VirtualKeys.N9, VirtualKeys.N0,
            VirtualKeys.A, VirtualKeys.B, VirtualKeys.C, VirtualKeys.D, VirtualKeys.E, VirtualKeys.F, VirtualKeys.G,
            VirtualKeys.H, VirtualKeys.I, VirtualKeys.J, VirtualKeys.K, VirtualKeys.L, VirtualKeys.M, VirtualKeys.N,
            VirtualKeys.O, VirtualKeys.P, VirtualKeys.Q, VirtualKeys.R, VirtualKeys.S, VirtualKeys.T, VirtualKeys.U,
            VirtualKeys.V, VirtualKeys.W, VirtualKeys.X, VirtualKeys.Y, VirtualKeys.Z
        };

        protected int KeyToIndex(VirtualKeys k)
        {
            var i = Array.IndexOf(Letters, k);
            if (i == -1)
                i = 0;
            return i;
        }

        protected VirtualKeys IndexToKey(int i) => Letters[i % Letters.Length];

        protected readonly List<Item> Items = new();
        public UIControlProperty<int> CurrentPage { get; private set; } = new(nameof(CurrentPage), 0, invalidate: true);
        public UIControlProperty<int> PageSize { get; private set; } = new(nameof(PageSize), 20, invalidate: true);
        protected int NumPages => (Items.Count - 1) / PageSize.V + 1;

        public ContainerModal(GameUI ui, GameResources resources, TContainer cont, ModalWindowButton[] buttons)
            : base(ui, resources, buttons, ModalWindowStyles.Default)
        {
            Container = cont;
            Items.AddRange(cont.Inventory?.GetItems() ?? Enumerable.Empty<Item>());
            CurrentPage.ValueChanged += (_, __) => Invalidate();
        }

        protected override void OnLayoutRebuilt(Layout oldValue)
        {
            base.OnLayoutRebuilt(oldValue);
            Layout.Size.ValueChanged += (_, __) =>
            {
                UpdatePageSize();
            };
        }

        public override void Open(string title)
        {
            base.Open(title);
            UpdatePageSize();
        }

        protected virtual void UpdatePageSize()
        {
            if (Layout is null) return;
            // Update PageSize dynamically
            var contentElem = Layout.Dom.Query(g => g.Id == "modal-content").Single();
            var availableSpace = contentElem.ComputedSize.Y - PaginatorHeight;
            var newPageSize = (int)Math.Ceiling(availableSpace / (float)RowHeight);
            var dirty = false;
            if (newPageSize != PageSize.V)
            {
                PageSize.V = newPageSize;
                dirty = true;
            }
            if (CurrentPage.V + 1 > NumPages)
            {
                CurrentPage.V = NumPages - 1;
                dirty = true;
            }
            if (dirty)
                RebuildLayout();
            Invalidate();
        }


        public void KeyboardSelectItem(int index, bool shift)
        {
            var items = Layout.Query<Button>(l => true, g => g.HasAnyClass("item-name"));
            var item = items.ElementAtOrDefault(index + (shift ? Letters.Length : 0));
            item?.Click(item.Position, Mouse.Button.Left);
        }

        protected abstract bool ShouldRemoveItem(Item i, TActions a);

        protected override void RegisterHotkeys(ModalWindowButton[] buttons)
        {
            base.RegisterHotkeys(buttons);
            int i = 0;
            Hotkeys.Add(new Hotkey(VirtualKeys.Prior), () => PrevPage());
            Hotkeys.Add(new Hotkey(VirtualKeys.Left), () => PrevPage());
            Hotkeys.Add(new Hotkey(VirtualKeys.Next), () => NextPage());
            Hotkeys.Add(new Hotkey(VirtualKeys.Right), () => NextPage());
            foreach (var item in Letters)
            {
                var j = i++;
                Hotkeys.Add(new Hotkey(item), () => KeyboardSelectItem(j, false));
                Hotkeys.Add(new Hotkey(item, shift: true), () => KeyboardSelectItem(j, true));
            }
        }

        protected virtual bool OnItemClicked(Button b, int index, Mouse.Button mouseButton)
        {
            if (mouseButton != Mouse.Button.Left)
            {
                return false;
            }
            var i = CurrentPage.V * PageSize.V + index;
            if (i >= Items.Count)
            {
                return false;
            }
            var modal = UI.OptionalChoice(
                GetAvailableActions(Items[i]).Distinct().ToArray(),
                Items[i].DisplayName
            );
            modal.Confirmed += (_, __) =>
            {
                ActionPerformed?.Invoke(Items[i], modal.SelectedOption);
                if (ShouldRemoveItem(Items[i], modal.SelectedOption))
                {
                    Items.RemoveAt(i);
                }
                Invalidate();
            };
            Invalidate();
            return false;
        }

        protected abstract IEnumerable<TActions> GetAvailableActions(Item i);

        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => base.DefineStyles(builder)
            .AddRule<UIControl>(s => s
                .Match(x => x.HasClass("paginator"))
                .Apply(x =>
                {
                    x.Background.V = UI.GetColor(ColorName.UIBorder);
                    x.Foreground.V = UI.GetColor(ColorName.UIBackground);
                    x.OutlineColor.V = UI.GetColor(ColorName.UIBorder);
                    x.OutlineThickness.V = 1;
                }))
            .AddRule<Picture>(s => s
                .Match(x => x.HasClass("item-sprite"))
                .Apply(x =>
                {
                    x.VerticalAlignment.V = VerticalAlignment.Middle;
                    x.LockAspectRatio.V = true;
                }))
            .AddRule<Button>(s => s
                .Match(x => x.HasClass("item-name"))
                .Apply(x =>
                {
                    x.HorizontalAlignment.V = HorizontalAlignment.Left;
                    x.Padding.V = new(8, 0);
                }))
            ;
        protected override LayoutGrid RenderContent(LayoutGrid layout)
        {
            return layout
                .Repeat(PageSize.V, (index, grid) => grid
                .Row(h: RowHeight, px: true, @class: index % 2 == 0 ? "row row-even" : "row row-odd")
                    .Col(w: RowHeight, px: true, @class: "item-sprite")
                        .Cell<Picture>(p =>
                        {
                            Invalidated += () => RefreshItemSprite(p, index);
                            p.OutlineThickness.V = 1;
                            p.OutlineColor.V = UI.GetColor(ColorName.UIBorder);
                        })
                    .End()
                    .Col(@class: "item-name")
                        .Cell<Button>(b =>
                        {
                            Invalidated += () => RefreshItemButton(b, index);
                            b.Clicked += (_, __, button) => OnItemClicked(b, index, button);
                            b.MouseEntered += (x, __) => x.Foreground.V = UI.GetColor(ColorName.UIAccent);
                            b.MouseLeft += (x, __) => x.Foreground.V = UI.GetColor(ColorName.UIPrimary);
                            b.OutlineThickness.V = 1;
                            b.OutlineColor.V = UI.GetColor(ColorName.UIBorder);
                        })
                    .End()
                .End())
                .Row(h: PaginatorHeight, px: true)
                    .Col(@class: "paginator spacer")
                        .Cell<Layout>(x => x.Background.V = UI.Store.Get(Data.UI.DefaultBackground))
                    .End()
                    .Col(w: 32, px: true, @class: "paginator paginator-prev")
                        .Cell<Button>(b =>
                        {
                            b.Text.V = "<";
                            b.FontSize.V = new Coord(16, 24);
                            b.HorizontalAlignment.V = HorizontalAlignment.Right;
                            b.Clicked += (_, __, ___) =>
                            {
                                PrevPage();
                                return false;
                            };
                        })
                    .End()
                    .Col(w: 64, px: true, @class: "paginator paginator-current")
                        .Cell<Label>(l =>
                        {
                            l.FontSize.V = new Coord(16, 24);
                            l.HorizontalAlignment.V = HorizontalAlignment.Center;
                            Invalidated += () => RefreshPageLabel(l);
                        })
                    .End()
                    .Col(w: 32, px: true, @class: "paginator paginator-next")
                        .Cell<Button>(b =>
                        {
                            b.Text.V = ">";
                            b.FontSize.V = new Coord(16, 24);
                            b.HorizontalAlignment.V = HorizontalAlignment.Left;
                            b.Clicked += (_, __, ___) =>
                            {
                                NextPage();
                                return false;
                            };
                        })
                    .End()
                    .Col(@class: "paginator spacer")
                        .Cell<Layout>(x => x.Background.V = UI.Store.Get(Data.UI.DefaultBackground))
                    .End()
                .End();

            void RefreshItemSprite(Picture p, int index)
            {
                var i = CurrentPage.V * PageSize.V + index;
                if (i >= Items.Count)
                {
                    p.Sprite.V = Resources.Sprites.Get(TextureName.Items, "None", ColorName.White);
                }
                else
                {
                    p.Sprite.V = Resources.Sprites.Get(TextureName.Items, Items[i].Render.Sprite, Items[i].Render.Color);
                }
            }

            void RefreshItemButton(Button b, int index)
            {
                var i = CurrentPage.V * PageSize.V + index;
                if (i >= Items.Count)
                {
                    b.Text.V = String.Empty;
                    return;
                }

                if (Container.TryCast<Actor>(out var actor))
                {
                    b.Foreground.V = actor.Equipment.IsEquipped(Items[i])
                        ? UI.Store.Get(Data.UI.DefaultAccent)
                        : UI.Store.Get(Data.UI.DefaultForeground);
                }
                var shift = index >= Letters.Length;
                if (shift)
                {
                    index -= Letters.Length;
                }
                var key = IndexToKey(index);
                var letter = WinKeyboardState.GetCharsFromKeys(key, UI.Input.KeyboardState, shift: shift);
                b.Text.V = $"{letter.Last()}) {Items[i].DisplayName}";
            }

            void RefreshPageLabel(Label l)
            {
                l.Text.V = $"{CurrentPage.V + 1}/{NumPages}";
            }
        }

        public void NextPage()
        {
            CurrentPage.V = (CurrentPage.V + 1).Mod(NumPages);
        }

        public void PrevPage()
        {
            CurrentPage.V = (CurrentPage.V - 1).Mod(NumPages);
        }

        public override void Update()
        {
            base.Update();
            if (UI.Input.IsMouseWheelScrollingUp())
                PrevPage();
            if (UI.Input.IsMouseWheelScrollingDown())
                NextPage();
        }

    }

}
