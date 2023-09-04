using Fiero.Core;

namespace Fiero.Business
{
    public abstract class PopUp : Modal
    {


        public PopUp(GameUI ui, GameResources resources, ModalWindowButton[] buttons, ModalWindowStyles? styles)
            : base(ui, resources, buttons, styles)
        {
            IsResponsive = false;
        }

        public override void Minimize()
        {
            Layout.Size.V = UI.Store.Get(Data.UI.PopUpSize);
            Layout.Position.V = Layout.Size.V / 2;
        }
    }
}
