using Fiero.Core;
using Fiero.Core.Structures;
using System;
using System.Collections.Generic;

namespace Fiero.Business
{
    [SingletonDependency]
    public class QuickSlotHelper
    {
        protected readonly record struct SlotMapping(string ActionName, Func<IAction> Action);
        public readonly record struct Slot(int Index, string ActionName, DrawableEntity Drawable);


        protected static readonly GameDatum<VirtualKeys>[] Keys = new[] {
            Data.Hotkeys.QuickSlot1, Data.Hotkeys.QuickSlot2, Data.Hotkeys.QuickSlot3,
            Data.Hotkeys.QuickSlot4, Data.Hotkeys.QuickSlot5, Data.Hotkeys.QuickSlot6,
            Data.Hotkeys.QuickSlot7, Data.Hotkeys.QuickSlot8, Data.Hotkeys.QuickSlot9
        };
        protected readonly GameInput Input;
        protected readonly GameDataStore Store;
        protected readonly Dictionary<VirtualKeys, OrderedPair<DrawableEntity, SlotMapping>> Map = new();

        public event Action<QuickSlotHelper> QuickSlotChanged;
        public event Action<QuickSlotHelper, int> SlotActivated;

        public QuickSlotHelper(GameInput input, GameDataStore store)
        {
            Input = input;
            Store = store;

            foreach (var slot in Keys)
            {
                slot.ValueChanged += Slot_ValueChanged;
            }

            void Slot_ValueChanged(GameDatumChangedEventArgs<VirtualKeys> obj)
            {
                if (Map.TryGetValue(obj.OldValue, out var getter))
                {
                    Map.Remove(obj.OldValue);
                    Map[obj.NewValue] = getter;
                }
            }
        }

        public void Set(int slot, DrawableEntity item, string actionName, Func<IAction> getter)
        {
            if (slot <= 0 || slot > 9)
                throw new ArgumentOutOfRangeException(nameof(slot));
            var key = Store.Get(Keys[slot - 1]);
            Map[key] = new(item, new(actionName, () => new UseQuickSlotAction(this, slot, getter())));
            QuickSlotChanged?.Invoke(this);
        }

        public void OnActionResolved(int slot)
        {
            SlotActivated?.Invoke(this, slot);
        }

        public void Refresh()
        {
            QuickSlotChanged?.Invoke(this);
        }

        private void UnsetInternal(int slot)
        {
            if (slot <= 0 || slot > 9)
                throw new ArgumentOutOfRangeException(nameof(slot));
            var key = Store.Get(Keys[slot - 1]);
            Map.Remove(key);
        }

        public void Unset(int slot)
        {
            UnsetInternal(slot);
            QuickSlotChanged?.Invoke(this);
        }

        public void UnsetAll()
        {
            for (int i = 1; i <= 9; i++)
            {
                UnsetInternal(i);
            }
            QuickSlotChanged?.Invoke(this);
        }

        public bool TryGetAction(out IAction action)
        {
            action = default;
            for (int i = 0; i < Keys.Length; i++)
            {
                var key = Store.Get(Keys[i]);
                if (Input.IsKeyPressed(key) && Map.TryGetValue(key, out var getter))
                {
                    action = getter.Right.Action();
                    return true;
                }
            }
            return false;
        }

        public IEnumerable<Slot> GetSlots()
        {
            for (int i = 0; i < Keys.Length; i++)
            {
                var key = Store.Get(Keys[i]);
                if (Map.TryGetValue(key, out var getter))
                {
                    yield return new(i + 1, getter.Right.ActionName, getter.Left);
                }
            }
        }
    }
}
