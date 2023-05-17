using Fiero.Core;

namespace Fiero.Business
{
    public abstract class PopUp : Modal
    {
        public PopUp(GameUI ui, GameResources resources, ModalWindowButton[] buttons, ModalWindowStyles styles = ModalWindowStyles.Default)
            : base(ui, resources, buttons, styles)
        {
        }

        protected override void SetDefaultSize()
        {
            Size.V = UI.Store.Get(Data.UI.PopUpSize);
            Position.V = Size.V / 2;
        }
    }
}
