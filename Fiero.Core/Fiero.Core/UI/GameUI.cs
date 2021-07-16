using LightInject;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Fiero.Core
{
    public class GameUI
    {
        public readonly GameDataStore Store;
        public readonly GameInput Input;
        public readonly GameWindow Window;
        protected readonly IServiceFactory ServiceProvider;
        protected readonly List<ModalWindow> OpenModals;

        public IEnumerable<ModalWindow> GetOpenModals() => OpenModals;

        public GameUI(IServiceFactory sp, GameInput input, GameDataStore store, GameWindow window)
        {
            ServiceProvider = sp;
            Store = store;
            Input = input;
            Window = window;
            OpenModals = new List<ModalWindow>();
        }

        public T ShowModal<T>(T wnd, string title, ModalWindowButton[] buttons, ModalWindowStyles styles = ModalWindowStyles.Default)
            where T : ModalWindow
        {
            OpenModals.Add(wnd);
            wnd.Closed += (_, __) => OpenModals.Remove(wnd);
            wnd.Open(title, buttons, styles);
            return wnd;
        }

        public LayoutBuilder CreateLayout() => new(ServiceProvider);
    }
}
