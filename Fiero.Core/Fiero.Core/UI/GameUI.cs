using LightInject;
using Microsoft.Extensions.DependencyInjection;
using SFML.Graphics;
using System;

namespace Fiero.Core
{

    public class GameUI
    {
        protected readonly IServiceFactory ServiceProvider;

        public GameUI(IServiceFactory sp)
        {
            ServiceProvider = sp;
        }

        public LayoutBuilder CreateLayout() => new(ServiceProvider);
    }
}
