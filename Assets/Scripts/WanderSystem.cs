using System.Collections.Generic;
using UnityEngine;

public class WanderSystem : IWorldSystem
{
    private readonly System.Random rng = new System.Random();

    public void Tick(WorldState world)
    {
        var walkNodes = NodeRegistry.Instance != null ? NodeRegistry.Instance.walk : null;

        if (walkNodes == null || walkNodes.Count == 0)
            return;

        foreach (var agent in world.Agents)
        {
            if (!agent.IsAlive)
                continue;

            if (agent.WaitTimer > 0f)
            {
                agent.WaitTimer -= 1f;
                continue;
            }

            bool hasPath = agent.CurrentPath != null && agent.CurrentPath.Count > 0 && agent.PathIndex < agent.CurrentPath.Count;
            if (hasPath)
                continue;

            if (agent.CurrentNode == null)
                continue;

            Node target = PickRandomTarget(walkNodes, agent.CurrentNode);
            if (target == null)
                continue;

            List<Node> path = NodePathfinder.FindPath(agent.CurrentNode, target);
            if (path == null || path.Count <= 1)
            {
                agent.WaitTimer = RandomRange(1f, 3f);
                continue;
            }

            agent.TargetNode = target;
            agent.CurrentPath = path;
            agent.PathIndex = 1;
            agent.WaitTimer = RandomRange(0.5f, 2f);
        }
    }

    private Node PickRandomTarget(List<Node> walkNodes, Node current)
    {
        if (walkNodes.Count == 0)
            return null;

        for (int i = 0; i < 10; i++)
        {
            Node candidate = walkNodes[rng.Next(walkNodes.Count)];
            if (candidate != null && candidate != current)
                return candidate;
        }

        return current;
    }

    private float RandomRange(float min, float max)
    {
        return (float)(min + rng.NextDouble() * (max - min));
    }
}