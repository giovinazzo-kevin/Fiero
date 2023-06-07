using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public sealed class DungeonGenerationContext
    {
        public class FloorNode
        {
            public readonly FloorId Id;
            public readonly HashSet<FloorConnection> Connections;
            public readonly Type Builder;

            public FloorNode(FloorId id, Type builder)
            {
                Id = id;
                Builder = builder;
                Connections = new();
            }

            public IEnumerable<FloorId> GetDestinations() => Connections.Where(c => c.From == Id).Select(c => c.To);
            public IEnumerable<FloorId> GetArrivals() => Connections.Where(c => c.To == Id).Select(c => c.From);
        }


        private readonly Dictionary<FloorId, FloorNode> _graph = new();

        public void AddBranch<T>(
            DungeonBranchName branch,
            int levels
        ) where T : IBranchGenerator
        {
            var connections = new HashSet<FloorConnection>();
            for (int i = 1; i <= levels; i++)
            {
                var id = new FloorId(branch, i);
                var node = new FloorNode(id, typeof(T));
                _graph.Add(id, node);
                if (i > 1)
                {
                    connections.Add(new(new(branch, i - 1), id));
                }
                if (i < levels)
                {
                    connections.Add(new(id, new(branch, i + 1)));
                }
            }
            foreach (var conn in connections)
            {
                Connect(conn.From, conn.To);
            }
        }

        public void Connect(FloorId downstairs, FloorId upstairs)
        {
            var connection = new FloorConnection(downstairs, upstairs);
            if (_graph.TryGetValue(downstairs, out var topFloor))
            {
                topFloor.Connections.Add(connection);
            }
            else if (downstairs != default) throw new ArgumentException(downstairs.ToString());
            if (_graph.TryGetValue(upstairs, out var bottomFloor))
            {
                bottomFloor.Connections.Add(connection);
            }
            else if (upstairs != default) throw new ArgumentException(downstairs.ToString());
        }

        public IEnumerable<FloorNode> GetFloors() => _graph.Values;
    }
}
