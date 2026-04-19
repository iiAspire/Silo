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

            bool hasPath = agent.CurrentPath != null &&
                           agent.CurrentPath.Count > 0 &&
                           agent.PathIndex < agent.CurrentPath.Count;

            if (hasPath)
                continue;

            if (agent.CurrentNode == null)
                continue;

            if (IsOnShift(world, agent) && agent.AssignedWorkNode != null)
            {
                TryAssignPath(agent, agent.AssignedWorkNode, RandomRange(0.2f, 0.8f));
                continue;
            }

            Node target = PickRandomTarget(walkNodes, agent.CurrentNode);
            if (target == null)
                continue;

            TryAssignPath(agent, target, RandomRange(0.5f, 2f));
        }
    }

    private bool IsOnShift(WorldState world, AgentRecord agent)
    {
        if (agent.AssignedShiftStartMinute < 0 || agent.AssignedShiftLengthMinutes <= 0)
            return false;

        int start = agent.AssignedShiftStartMinute;
        int end = (start + agent.AssignedShiftLengthMinutes) % 1440;
        int now = world.MinuteOfDay;

        if (start < end)
            return now >= start && now < end;

        return now >= start || now < end;
    }

    private void TryAssignPath(AgentRecord agent, Node target, float failWaitTime)
    {
        if (target == null || agent.CurrentNode == null)
            return;

        if (agent.CurrentNode == target)
        {
            agent.TargetNode = target;
            agent.WaitTimer = RandomRange(1f, 3f);
            return;
        }

        List<Node> path = NodePathfinder.FindPath(agent.CurrentNode, target);
        if (path == null || path.Count <= 1)
        {
            agent.WaitTimer = failWaitTime;
            return;
        }

        agent.TargetNode = target;
        agent.CurrentPath = path;
        agent.PathIndex = 1;
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