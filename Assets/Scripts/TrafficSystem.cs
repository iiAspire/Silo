using System.Collections.Generic;

public class TrafficSystem : IWorldSystem
{
    public readonly Dictionary<Node, int> VisitCounts = new Dictionary<Node, int>();

    public void Tick(WorldState world)
    {
        VisitCounts.Clear();

        foreach (var agent in world.Agents)
        {
            if (!agent.IsAlive || agent.CurrentNode == null)
                continue;

            if (VisitCounts.ContainsKey(agent.CurrentNode))
                VisitCounts[agent.CurrentNode]++;
            else
                VisitCounts[agent.CurrentNode] = 1;
        }
    }
}