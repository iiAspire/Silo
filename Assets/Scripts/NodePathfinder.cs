using System.Collections.Generic;

public static class NodePathfinder
{
    public static List<Node> FindPath(Node start, Node goal)
    {
        if (start == null || goal == null)
            return null;

        Queue<Node> frontier = new Queue<Node>();
        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();

        frontier.Enqueue(start);
        cameFrom[start] = null;

        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();
            if (current == goal)
                break;

            foreach (Node next in current.neighbors)
            {
                if (next == null || cameFrom.ContainsKey(next))
                    continue;

                frontier.Enqueue(next);
                cameFrom[next] = current;
            }
        }

        if (!cameFrom.ContainsKey(goal))
            return null;

        List<Node> path = new List<Node>();
        Node step = goal;

        while (step != null)
        {
            path.Add(step);
            step = cameFrom[step];
        }

        path.Reverse();
        return path;
    }
}