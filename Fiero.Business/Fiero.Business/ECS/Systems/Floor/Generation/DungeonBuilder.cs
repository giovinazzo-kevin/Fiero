using Fiero.Core;
using LightInject;
using System;
using System.Collections.Generic;

namespace Fiero.Business
{
    public sealed class DungeonBuilder
    {
        private readonly GameEntities _entities;
        private readonly GameEntityBuilders _builders;
        private readonly IServiceFactory _serviceFactory;
        private readonly List<Action<DungeonGenerationContext>> _steps;

        internal DungeonBuilder(
            GameEntities entities, 
            GameEntityBuilders builders,
            IServiceFactory services
        ) {
            _entities = entities;
            _builders = builders;
            _serviceFactory = services;
            _steps = new();
        }

        public DungeonBuilder WithStep(Action<DungeonGenerationContext> step)
        {
            _steps.Add(step);
            return this;
        }

        public IEnumerable<Floor> Build()
        {
            var context = new DungeonGenerationContext();
            foreach (var step in _steps) {
                step(context);
            }
            foreach (var node in context.GetFloors()) {
                var builder = new FloorBuilder(node.Size, _entities, _builders)
                    .WithStep(ctx => ctx.AddConnections(node.Connections));
                var generator = (BranchGenerator)_serviceFactory.GetInstance(node.Builder);
                var floor = generator.GenerateFloor(node.Id, builder);
                yield return floor;
            }
        }
    }
}
