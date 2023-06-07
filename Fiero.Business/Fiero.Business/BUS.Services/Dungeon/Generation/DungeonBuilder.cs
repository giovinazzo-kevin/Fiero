using Fiero.Core;
using LightInject;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    [TransientDependency]
    public sealed class DungeonBuilder
    {
        private readonly IServiceFactory _serviceFactory;
        private readonly List<Action<DungeonGenerationContext>> _steps;

        public DungeonBuilder(
            IServiceFactory services
        )
        {
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
            foreach (var step in _steps)
            {
                step(context);
            }
            foreach (var node in context.GetFloors())
            {
                var builder = _serviceFactory.GetInstance<FloorBuilder>()
                    .WithStep(ctx => ctx.AddConnections(node.Connections.ToArray()));
                var generator = (IBranchGenerator)_serviceFactory.GetInstance(node.Builder);
                var floor = generator.GenerateFloor(node.Id, builder);
                yield return floor;
            }
        }
    }
}
