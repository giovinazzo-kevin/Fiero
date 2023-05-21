using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Fiero.Business
{
    public class RoomTree : IFloorGenerationPrefab
    {
        public record class Node(Room Room, HashSet<(Corridor Corridor, Node Node)> Links)
        {
            public static Node Root(Room r) => new(r, new());

            public float Centrality { get; private set; }

            internal float ComputeCentrality(Node root, HashSet<Node> seen = null, float partialSum = 0)
            {
                seen ??= new();
                seen.Add(this);
                if (this == root) return partialSum;
                var newSum = partialSum;
                foreach (var link in Links)
                {
                    if (seen.Contains(link.Node)) continue;
                    newSum += link.Node.ComputeCentrality(root, seen, partialSum + 1);
                }
                return Centrality = newSum;
            }

            public bool TryLink(Corridor corridor, Node child)
            {
                if (Links.Any(l => l.Corridor == corridor))
                    return false;
                Links.Add((corridor, child));
                child.TryLink(corridor, this);
                return true;
            }

            public bool IsUnlinked => Links?.Count == 0;
        }
        public readonly Node Root;
        public RoomTree(Node root)
        {
            Root = root;
        }

        public IEnumerable<Node> Nodes => Traverse().Select(x => x.Child).Distinct();
        public IEnumerable<Corridor> Corridors => Traverse().Select(x => x.Link).Where(x => x is not null).Distinct();
        public IEnumerable<(Node Parent, Corridor Link, Node Child)> Traverse()
        {
            var visitedRooms = new Dictionary<Room, int>();
            var visitedCorridors = new Dictionary<Corridor, int>();
            visitedRooms[Root.Room] = 1;
            yield return (null, null, Root);
            foreach (var result in Inner(Root, null, visitedRooms, visitedCorridors))
                yield return result;

            IEnumerable<(Node P, Corridor C, Node N)> Inner(Node current, Corridor prevCorridor, Dictionary<Room, int> visitedRooms, Dictionary<Corridor, int> visitedCorridors)
            {
                foreach (var (corridor, next) in current.Links)
                {
                    if (visitedCorridors.ContainsKey(corridor) || visitedRooms.ContainsKey(next.Room) && visitedRooms[next.Room] > 1)
                        continue;

                    visitedRooms[next.Room] = visitedRooms.ContainsKey(next.Room) ? visitedRooms[next.Room] + 1 : 1;
                    visitedCorridors[corridor] = visitedCorridors.ContainsKey(corridor) ? visitedCorridors[corridor] + 1 : 1;

                    yield return (current, corridor, next);
                    foreach (var result in Inner(next, corridor, visitedRooms, visitedCorridors))
                        yield return result;
                }
            }
        }

        public void SetTheme(DungeonTheme theme, Func<IFloorGenerationPrefab, bool> pred = null)
        {
            pred ??= _ => true;
            if (pred(Root.Room))
                Root.Room.Theme = theme;
            foreach (var (_, corridor, child) in Traverse())
            {
                if (pred(corridor))
                    corridor.Theme = theme;
                if (pred(child.Room))
                    child.Room.Theme = theme;
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

                var any = false;
                foreach (var bwd in linksBwd)
                {
                    var r = GetOrCreate(bwd.Start.Owner);
                    r.TryLink(bwd, node);
                    stack.Push(r);
                    any = true;
                }

                foreach (var fwd in linksFwd)
                {
                    var r = GetOrCreate(fwd.End.Owner);
                    node.TryLink(fwd, r);
                    stack.Push(r);
                    any = true;
                }

                if (!any)
                {
                    // throw new ArgumentException(nameof(node));
                }

                seen.Add(node);
            }
            var tree = new RoomTree(dict.Values.First());
            // Now that the tree is built we can post-process it in order to:
            // - Calculate the centrality of the graph to determine the spawn point
            foreach (var node in seen)
            {
                node.ComputeCentrality(tree.Root);
            }
            return tree;

            Node GetOrCreate(Room r) => dict.TryGetValue(r, out var ret) ? ret : dict[r] = Node.Root(r);
        }

        public void Draw(FloorGenerationContext ctx)
        {
            var drawnRooms = new HashSet<Node>();
            foreach (var child in Nodes)
            {
                if (child != null && !drawnRooms.Contains(child))
                {
                    child.Room.Draw(ctx);
                    drawnRooms.Add(child);
                }
            }
            //foreach (var corridor in Corridors)
            //{
            //    corridor.Draw(ctx);
            //}
            new CorridorLayer(Corridors).Draw(ctx);
        }
    }
}
