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
            Layout.Position.V = obj.NewValue / 4;
            Layout.Size.V = obj.NewValue / 2;
        }

        protected override void BeforePresentation()
        {
            var windowSize = UI.Store.Get(Data.UI.WindowSize);
            OnWindowSizeChanged(new(Data.UI.WindowSize, windowSize, windowSize));
        }
    }
}
