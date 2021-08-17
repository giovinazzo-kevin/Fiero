using Fiero.Core;
using SFML.Window;
using System;
using System.Collections.Generic;

namespace Fiero.Business
{
    [SingletonDependency]
    public class QuickSlotHelper
    {
        protected static readonly GameDatum<Keyboard.Key>[] Keys = new[] {
            Data.Hotkeys.QuickSlot1, Data.Hotkeys.QuickSlot2, Data.Hotkeys.QuickSlot3, 
            Data.Hotkeys.QuickSlot4, Data.Hotkeys.QuickSlot5, Data.Hotkeys.QuickSlot6,
            Data.Hotkeys.QuickSlot7, Data.Hotkeys.QuickSlot8, Data.Hotkeys.QuickSlot9
        };
        protected readonly GameInput Input;
        protected readonly GameDataStore Store;
        protected readonly Dictionary<Keyboard.Key, OrderedPair<DrawableEntity, Func<IAction>>> Map = new();

        public QuickSlotHelper(GameInput input, GameDataStore store)
        {
            Input = input;
            Store = store;

            foreach (var slot in Keys) {
                slot.ValueChanged += Slot_ValueChanged;
            }

            void Slot_ValueChanged(GameDatumChangedEventArgs<Keyboard.Key> obj)
            {
                if(Map.TryGetValue(obj.OldValue, out var getter)) {
                    Map.Remove(obj.OldValue);
                    Map[obj.NewValue] = getter;
                }
            }
        }

        public void Set(int slot, DrawableEntity item, Func<IAction> getter)
        {
            if (slot <= 0 || slot > 9)
                throw new ArgumentOutOfRangeException(nameof(slot));
            var key = Store.Get(Keys[slot - 1]);
            Map[key] = new(item, getter);
        }

        public void Unset(int slot)
        {
            if (slot <= 0 || slot > 9)
                throw new ArgumentOutOfRangeException(nameof(slot));
            var key = Store.Get(Keys[slot - 1]);
            Map.Remove(key);
        }

        public void UnsetAll()
        {
            for (int i = 1; i <= 9; i++) {
                Unset(i);
            }
        }

        public bool TryGetAction(out IAction action)
        {
            action = default;
            for (int i = 0; i < Keys.Length; i++) {
                var key = Store.Get(Keys[i]);
                if (Input.IsKeyPressed(key) && Map.TryGetValue(key, out var getter)) {
                    action = getter.Right();
                    return true;
                }
            }
            return false;
        }

        public IEnumerable<OrderedPair<int, SpriteDef>> GetSprites()
        {
            for (int i = 0; i < Keys.Length; i++) {
                var key = Store.Get(Keys[i]);
                if (Map.TryGetValue(key, out var getter)) {
                    yield return new(i + 1, new(
                        getter.Left.Render.TextureName,
                        getter.Left.Render.SpriteName,
                        getter.Left.Render.Color,
                        new(),
                        new(1, 1)
                    ));
                }
            }
        }
    }
}
