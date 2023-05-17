using LightInject;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Core
{
    public class GameUI
    {
        public readonly GameDataStore Store;
        public readonly GameInput Input;
        public readonly GameWindow Window;
        public readonly IServiceFactory ServiceProvider;
        protected readonly List<UIWindow> OpenWindows;

        public IEnumerable<UIWindow> GetOpenWindows() => OpenWindows.Except(GetOpenModals());
        public IEnumerable<ModalWindow> GetOpenModals() => OpenWindows.OfType<ModalWindow>();

        public GameUI(
            IServiceFactory sp,
            GameInput input,
            GameDataStore store,
            GameWindow window
        )
        {
            ServiceProvider = sp;
            Store = store;
            Input = input;
            Window = window;
            OpenWindows = new List<UIWindow>();
        }

        public T Open<T>(T wnd, string title = null)
            where T : UIWindow
        {
            if (wnd.IsOpen)
                wnd.Close(ModalWindowButton.None);
            OpenWindows.Add(wnd);
            wnd.Closed += Wnd_Closed;
            wnd.Open(title);
            return wnd;
            void Wnd_Closed(UIWindow wnd, ModalWindowButton btn)
            {
                OpenWindows.Remove(wnd);
                wnd.Closed -= Wnd_Closed;
            }
        }
        public LayoutBuilder CreateLayout() => new(ServiceProvider);
    }
}
