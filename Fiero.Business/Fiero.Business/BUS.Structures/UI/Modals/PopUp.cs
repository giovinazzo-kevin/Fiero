using Fiero.Core;

namespace Fiero.Business
{
    public abstract class PopUp : Modal
    {


        public PopUp(GameUI ui, GameResources resources, ModalWindowButton[] buttons)
            : base(ui, resources, buttons)
        {
        }

        protected override void OnLayoutRebuilt(Layout oldValue)
        {
            //base.OnLayoutRebuilt(oldValue);
            Layout.Size.V = UI.Store.Get(Data.UI.PopUpSize);
            Layout.Position.V = Layout.Size.V / 2;
        }
    }
}
