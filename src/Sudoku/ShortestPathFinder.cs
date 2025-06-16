namespace Sudoku;

public static class ShortestPathFinder
{
    public record Node(string Name)
    {
        public Dictionary<Node, int> Next { get; } = new();
    }

    public static string GetShortestPath(Node start, Node end)
    {
        var hops = BuildHops(start, end);
        if (hops.Count == 0) return string.Empty;

        var length = 0;
        var path = new List<Node> { end };
        var current = end;
        while (current != start)
        {
            var previous = hops[current];
            length += previous.Next[current];
            path.Add(previous);
            current = previous;
        }

        path.Reverse();
        return string.Join(" -> ", path.Select(n => n.Name)) + $" total length {length}";
    }

    private static IReadOnlyDictionary<Node, Node> BuildHops(Node start, Node end)
    {
        //to a given key what node was the one with the shortest distance
        var hops = new Dictionary<Node, Node> { [start] = start };

        //for a given key what is the shortest distance from the start
        var distances = new Dictionary<Node, int> { [start] = 0 };

        //take the next node closest to the current set of observed nodes
        var frontier = new PriorityQueue<Node, int>();
        frontier.Enqueue(start, 0);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            var currentDistance = distances[current];
            foreach (var (nextNode, nextDistance) in current.Next)
            {
                var oldDistance = distances.GetValueOrDefault(nextNode, int.MaxValue);
                var newDistance = currentDistance + nextDistance;

                if (oldDistance > newDistance)
                {
                    frontier.Enqueue(nextNode, newDistance);
                    hops[nextNode] = current;
                    distances[nextNode] = newDistance;
                    //do not try to short-circuit - thus we'll contain the first path reaching the target node
                    //not the shortest path to the target node.
                }
            }
        }

        return hops.ContainsKey(end) ? hops : new Dictionary<Node, Node>();
    }
}