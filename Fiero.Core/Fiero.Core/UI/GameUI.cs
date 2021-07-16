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
        protected readonly IServiceFactory ServiceProvider;
        protected readonly List<ModalWindow> OpenModals;

        public IEnumerable<ModalWindow> GetOpenModals() => OpenModals;

        public GameUI(IServiceFactory sp, GameInput input, GameDataStore store)
        {
            ServiceProvider = sp;
            Store = store;
            Input = input;
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
