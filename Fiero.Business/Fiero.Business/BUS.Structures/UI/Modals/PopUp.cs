using Fiero.Core;

namespace Fiero.Business
{
    public abstract class PopUp : Modal
    {
        public PopUp(GameUI ui, GameResources resources, ModalWindowButton[] buttons, ModalWindowStyles styles = ModalWindowStyles.Default)
            : base(ui, resources, buttons, styles)
        {
        }
    }
}
