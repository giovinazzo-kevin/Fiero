using LightInject;
using Microsoft.Extensions.DependencyInjection;
using System;
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

        public GameUI(IServiceFactory sp, GameInput input, GameDataStore store, GameWindow window)
        {
            ServiceProvider = sp;
            Store = store;
            Input = input;
            Window = window;
            OpenWindows = new List<UIWindow>();
        }

        public T Show<T>(T wnd, string title = null)
            where T : UIWindow
        {
            OpenWindows.Add(wnd);
            wnd.Closed += (_, __) => OpenWindows.Remove(wnd);
            wnd.Open(title);
            return wnd;
        }

        public LayoutBuilder CreateLayout() => new(ServiceProvider);
    }
}
