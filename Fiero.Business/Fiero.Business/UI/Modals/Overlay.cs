using Fiero.Core;
using System;

namespace Fiero.Business
{
    public abstract class Overlay : Modal
    {
        protected Overlay(GameUI ui) : base(ui)
        {
        }

        public override void Open(string title, ModalWindowButton[] _, ModalWindowStyles __)
        {
            base.Open(title, Array.Empty<ModalWindowButton>(), ModalWindowStyles.Title);
        }

        protected override void RegisterHotkeys(ModalWindowButton[] buttons)
        {
            Hotkeys.Add(new Hotkey(UI.Store.Get(Data.Hotkeys.Cancel)), () => Close(ModalWindowButton.ImplicitNo));
        }
    }
}
