using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class RoomTree : IFloorGenerationPrefab
    {
        public record class Node(Room Room, HashSet<(Corridor Corridor, Node Node)> Links)
        {
            public static Node Root(Room r) => new(r, new());

            public bool TryLink(Corridor corridor, Node child)
            {
                if (Links.Any(l => l.Corridor == corridor))
                    return false;
                Links.Add((corridor, child));
                child.Links.Add((corridor, this));
                return true;
            }

            public bool IsUnlinked => Links?.Count == 0;
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
                visited.Add(root);
                foreach (var (c, n) in root.Links)
                {
                    if (visited.Contains(n))
                        continue;
                    yield return (root, c, n);
                    foreach (var rest in Inner(n, visited))
                        yield return rest;
                }
            }
        }

        public static RoomTree Build(IEnumerable<Room> rooms, IEnumerable<Corridor> corridors)
        {
            var seen = new HashSet<Node>();
            var dict = new Dictionary<Room, Node>();
            var stack = new Stack<Node>();
            foreach (var room in rooms)
            {
                var node = dict[room] = GetOrCreate(room);
                stack.Push(node);
            }
            while (stack.TryPop(out var node))
            {
                if (seen.Contains(node))
                    continue;

                var linksBwd = corridors
                    .Where(c => c.End.Owner == node.Room);
                var linksFwd = corridors
                    .Where(c => c.Start.Owner == node.Room);

                foreach (var bwd in linksBwd)
                {
                    var r = GetOrCreate(bwd.Start.Owner);
                    r.TryLink(bwd, node);
                    stack.Push(r);
                }

                foreach (var fwd in linksFwd)
                {
                    var r = GetOrCreate(fwd.End.Owner);
                    node.TryLink(fwd, r);
                    stack.Push(r);
                }

                seen.Add(node);
            }
            return new(dict.Values.First());

            Node GetOrCreate(Room r) => dict.TryGetValue(r, out var ret) ? ret : dict[r] = Node.Root(r);
        }

        public void Draw(FloorGenerationContext ctx)
        {
            var drawnRooms = new HashSet<Node>();
            foreach (var (_, corridor, child) in Traverse())
            {
                if (child != null && !drawnRooms.Contains(child))
                {
                    child.Room.Draw(ctx);
                    drawnRooms.Add(child);
                }
                if (corridor != null)
                    corridor.Draw(ctx);
            }
        }
    }
}
