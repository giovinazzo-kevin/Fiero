using Fiero.Core;
using SFML.Window;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Fiero.Business
{
    public static class GameUIExtensions
    {
        public static InventoryModal Inventory(this GameUI ui, Actor actor, string title = null)
            => ui.ShowModal(
                new InventoryModal(ui, actor),
                title,
                ModalWindowButtons.Close,
                title != null ? ModalWindowStyles.Default
                              : ModalWindowStyles.Default & ~ModalWindowStyles.Title
            );
        public static DialogueModal Dialogue(this GameUI ui, IDialogueTrigger trigger, DialogueNode node, Drawable speaker, params Drawable[] listeners)
            => ui.ShowModal(
                new DialogueModal(ui, trigger, node, speaker, listeners),
                null,
                ModalWindowButtons.Ok,
                ModalWindowStyles.None
            );
        public static ChoicePopUp<T> NecessaryChoice<T>(this GameUI ui, T[] choices, string title = null)
            => ui.ShowModal(
                new ChoicePopUp<T>(ui, choices), 
                title, 
                ModalWindowButtons.Ok,
                title != null ? ModalWindowStyles.Default
                              : ModalWindowStyles.Default & ~ModalWindowStyles.Title
            );
        public static ChoicePopUp<T> OptionalChoice<T>(this GameUI ui, T[] choices, string title = null)
            => ui.ShowModal(
                new ChoicePopUp<T>(ui, choices), 
                title, 
                ModalWindowButtons.Ok | ModalWindowButtons.Cancel,
                title != null ? ModalWindowStyles.Default
                              : ModalWindowStyles.Default & ~ModalWindowStyles.Title
            );
    }
}
