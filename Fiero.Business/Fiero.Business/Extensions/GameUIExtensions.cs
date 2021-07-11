using Fiero.Core;
using SFML.Window;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Fiero.Business
{
    public static class GameUIExtensions
    {
        public static InventoryModal Inventory(this GameUI ui, Actor actor, string title = "Inventory")
            => ui.ShowModal(new InventoryModal(ui, actor), title, ModalWindowButtons.Close);
        public static ChoicePopUp<T> NecessaryChoice<T>(this GameUI ui, T[] choices, string title = "Choose one")
            => ui.ShowModal(new ChoicePopUp<T>(ui, choices), title, ModalWindowButtons.Ok);
        public static ChoicePopUp<T> OptionalChoice<T>(this GameUI ui, T[] choices, string title = "Choose one")
            => ui.ShowModal(new ChoicePopUp<T>(ui, choices), title, ModalWindowButtons.Ok | ModalWindowButtons.Cancel);
    }
}
