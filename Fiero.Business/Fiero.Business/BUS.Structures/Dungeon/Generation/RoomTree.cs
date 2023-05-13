using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class RoomTree : IFloorGenerationPrefab
    {
        public record class Node(Room Room, HashSet<(Corridor Corridor, Node Node)> Links)
        {
            public Node Parent { get; private set; }
            public static Node Root(Room r) => new(r, new());

            public bool TryLink(Corridor corridor, Node child)
            {
                if (Links.Any(l => l.Corridor == corridor))
                    return false;
                child.Parent = this;
                Links.Add((corridor, child));
                return true;
            }

            public bool TryLink(Corridor corridor, Room room, out Node next)
            {
                next = default;
                if (Links.Any(l => l.Corridor == corridor))
                    return false;
                Links.Add((corridor, next = new Node(room, new()) { Parent = this }));
                return true;
            }

            public bool IsUnlinked => Parent == null && Links?.Count == 0;
        }
        public readonly Node Root;
        public RoomTree(Node root)
        {
            Root = root;
        }

        public IEnumerable<(Node Parent, Corridor Link, Node Child)> Traverse()
        {
            yield return (null, null, Root);
            var visited = new HashSet<Node>();
            foreach (var rest in Inner(Root, visited))
                yield return rest;
            IEnumerable<(Node P, Corridor C, Node N)> Inner(Node root, HashSet<Node> visited)
            {
                foreach (var (c, n) in root.Links)
                {
                    if (visited.Contains(n))
                        continue;
                    if (n.Parent != root)
                    {
                        foreach (var rest in Inner(n.Parent, visited))
                            yield return rest;
                    }
                    yield return (root, c, n);
                    foreach (var rest in Inner(n, visited))
                        yield return rest;
                }
                visited.Add(root);
            }
        }

        public static RoomTree Build(IEnumerable<Room> rooms, IEnumerable<Corridor> corridors)
        {
            var dict = new Dictionary<Room, Node>();
            foreach (var room in rooms)
            {
                var node = dict[room] = GetOrCreate(room);

                var linksBwd = corridors
                    .Where(c => c.End.Owner == room);
                var linksFwd = corridors
                    .Where(c => c.Start.Owner == room);

                foreach (var bwd in linksBwd)
                {
                    var r = GetOrCreate(bwd.Start.Owner);
                    r.TryLink(bwd, node);
                }

                foreach (var fwd in linksFwd)
                {
                    var r = GetOrCreate(fwd.End.Owner);
                    node.TryLink(fwd, r);
                }
            }

            return new(dict.Values.First());

            Node GetOrCreate(Room r) => dict.TryGetValue(r, out var ret) ? ret : dict[r] = Node.Root(r);
        }

        public void Draw(FloorGenerationContext ctx)
        {
            foreach (var (_, corridor, child) in Traverse())
            {
                if (corridor != null)
                    corridor.Draw(ctx);
                if (child != null)
                    child.Room.Draw(ctx);
            }
        }
    }
}
