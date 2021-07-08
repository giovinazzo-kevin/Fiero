using Fiero.Core;
using SFML.Window;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Fiero.Business
{
    public static class GameUIExtensions
    {
        public static void ShowInventoryModal(this GameUI ui, InventoryComponent inventory)
            => ui.ShowModal(new InventoryModal(ui, inventory), "Inventory", ModalWindowButtons.Close);
    }
}
