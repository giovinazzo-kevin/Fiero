using Fiero.Core;
using System;
using System.Collections;
using System.Linq;

namespace Fiero.Business
{
    public class InventoryModal : Modal
    {
        public readonly InventoryComponent Inventory;
        public UIControlProperty<int> CurrentPage { get; private set; } = new(nameof(CurrentPage), 0);
        public UIControlProperty<int> PageSize { get; private set; } = new(nameof(PageSize), 20);

        public InventoryModal(GameUI ui, InventoryComponent inventory) : base(ui)
        {
            Inventory = inventory;
            Hotkeys.Add(new Hotkey(UI.Store.Get(Data.Hotkeys.ToggleInventory)), () => Close(ModalWindowButtons.None));
        }

        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => builder
            .AddRule<UIControl>(style => style
                .Match(x => x.HasAnyClass("modal-title", "modal-controls", "modal-content"))
                .Apply(x => x.Background.V = UI.Store.Get(Data.UI.DefaultBackground)))
            .AddRule<UIControl>(style => style
                .Match(x => x.HasClass("row-even"))
                .Apply(x => x.Background.V = UI.Store.Get(Data.UI.DefaultBackground).AddRgb(16, 16, 16)))
            ;

        protected override LayoutGrid RenderContent(LayoutGrid layout)
        {
            var winSize = UI.Store.Get(Data.UI.WindowSize);
            var items = (Inventory?.GetItems() ?? Enumerable.Empty<Item>())
                .ToList();
            var pages = (items.Count - 1) / PageSize.V + 1;
            return layout
                .Repeat(PageSize.V, (index, grid) => grid
                    .Row(@class: index % 2 == 0 ? "row-even" : "row-odd")
                        .Col(w: 0.06f)
                            .Cell<Picture<TextureName>>(p => {
                                p.HorizontalAlignment.V = HorizontalAlignment.Right;
                                p.TextureName.V = TextureName.Atlas;
                                p.LockAspectRatio.V = true;
                                CurrentPage.ValueChanged += (_, __) => {
                                    Update();
                                };
                                Update();
                                void Update()
                                {
                                    var i = CurrentPage.V * PageSize.V + index;
                                    if (i >= items.Count) {
                                        p.SpriteName.V = "None";
                                    }
                                    else {
                                        p.SpriteName.V = items[i].Render.SpriteName;
                                        p.Sprite.Color = items[i].Render.Sprite.Color;
                                    }
                                }
                            })
                        .End()
                        .Col(w: 1.94f)
                            .Cell<Button>(l => {
                                l.CenterContentH.V = false;
                                l.FontSize.V = 18;
                                l.Padding.V = new(16, 0);
                                if (index % 2 == 0) {
                                    l.Background.V = UI.Store.Get(Data.UI.DefaultBackground);
                                }
                                l.Clicked += (_, __, ___) => {
                                    var i = CurrentPage.V * PageSize.V + index;
                                    if (i < items.Count) {
                                        items[i].ItemProperties.Identified ^= true;
                                        Update();
                                    }
                                    return false;
                                };
                                CurrentPage.ValueChanged += (_, __) => {
                                    Update();
                                };
                                Update();
                                void Update()
                                {
                                    var i = CurrentPage.V * PageSize.V + index;
                                    if (i >= items.Count) {
                                        l.Text.V = String.Empty;
                                        return;
                                    }
                                    l.Text.V = items[i].DisplayName;
                                }
                            })
                        .End()
                    .End())
                .Row()
                    .Col(w: 0.25f)
                        .Cell<Button>(b => {
                            b.Text.V = "<";
                            b.Clicked += (_, __, ___) => {
                                CurrentPage.V = Mod(CurrentPage.V - 1, pages);
                                return false;
                            };
                        })
                    .End()
                    .Col(w: 2.50f)
                        .Cell<Label>(l => {
                            CurrentPage.ValueChanged += (_, __) => {
                                Update();
                            };
                            Update();
                            void Update()
                            {
                                l.Text.V = $"{CurrentPage.V + 1}/{pages}";
                            }
                        })
                    .End()
                    .Col(w: 0.25f)
                        .Cell<Button>(b => {
                            b.Text.V = ">";
                            b.Clicked += (_, __, ___) => {
                                CurrentPage.V = Mod(CurrentPage.V + 1, pages);
                                return false;
                            };
                        })
                    .End()
                .End();

            static int Mod(int x, int m)
            {
                return (x % m + m) % m;
            }
        }
    }
}
