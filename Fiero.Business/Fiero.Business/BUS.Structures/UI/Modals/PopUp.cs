using Fiero.Core;

namespace Fiero.Business
{
    public abstract class PopUp : Modal
    {
        public PopUp(GameUI ui, GameResources resources, ModalWindowButton[] buttons, ModalWindowStyles styles = ModalWindowStyles.Default)
            : base(ui, resources, buttons, styles)
        {
        }

        protected override void OnGameWindowSizeChanged(GameDatumChangedEventArgs<Coord> obj)
        {
            Size.V = UI.Store.Get(Data.UI.PopUpSize);
            Position.V = obj.NewValue / 2 - Layout.Size.V / 2;
            Invalidate();
        }
    }
}
