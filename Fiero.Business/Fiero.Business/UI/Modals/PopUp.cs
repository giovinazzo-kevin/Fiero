using Fiero.Core;

namespace Fiero.Business
{
    public abstract class PopUp : Modal
    {
        public PopUp(GameUI ui) : base(ui)
        {
        }

        protected override void OnWindowSizeChanged(GameDatumChangedEventArgs<Coord> obj)
        {
            Layout.Size.V = UI.Store.Get(Data.UI.PopUpSize);
            Layout.Position.V = obj.NewValue / 2 - Layout.Size.V / 2;
        }

        protected override void BeforePresentation()
        {
            var windowSize = UI.Store.Get(Data.UI.WindowSize);
            OnWindowSizeChanged(new(Data.UI.WindowSize, windowSize, windowSize));
        }
    }
}
