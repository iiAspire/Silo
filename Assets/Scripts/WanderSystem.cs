using System.Collections.Generic;
using UnityEngine;

public class WanderSystem : IWorldSystem
{
    private readonly System.Random rng = new System.Random();

    private const int WorkArrivalLeadMinutes = 30;

    public void Tick(WorldState world)
    {
        var registry = NodeRegistry.Instance;
        var walkNodes = registry != null ? registry.walk : null;

        if (walkNodes == null || walkNodes.Count == 0)
            return;

        foreach (var agent in world.Agents)
        {
            if (!agent.IsAlive || agent.CurrentNode == null)
                continue;

            //if (agent.BaseJob == "Maintenance Worker" || agent.Job == "Maintenance Worker")
            //{
                //Debug.Log(
                //    $"Tick={world.Tick} Time={world.MinuteOfDay} " +
                //    $"Agent={agent.AgentId} Job='{agent.Job}' BaseJob='{agent.BaseJob}' " +
                //    $"Start={agent.AssignedShiftStartMinute} Length={agent.AssignedShiftLengthMinutes} " +
                //    $"OnShift={SimulationManager.Instance.IsAgentOnShift(agent)}"
                //);
            //}

            bool commutingToWork = ShouldCommuteToWork(world, agent);
            bool onShift = IsOnShift(world, agent);
            bool shouldPrioritizeWork = (commutingToWork || onShift) && agent.AssignedWorkNode != null;

            bool hasPath = agent.CurrentPath != null &&
                           agent.CurrentPath.Count > 0 &&
                           agent.PathIndex < agent.CurrentPath.Count;

            if (shouldPrioritizeWork)
            {
                agent.CurrentIntent = IntentType.Work;

                if (agent.CurrentNode == agent.AssignedWorkNode)
                {
                    agent.TargetNode = agent.AssignedWorkNode;
                    ClearPath(agent);
                    agent.WaitTimer = RandomRange(1f, 3f);
                    continue;
                }

                bool alreadyHeadingToWork = agent.TargetNode == agent.AssignedWorkNode;

                if (!alreadyHeadingToWork || !hasPath)
                    TryAssignPath(agent, agent.AssignedWorkNode, RandomRange(0.2f, 0.8f));

                continue;
            }

            if (agent.WaitTimer > 0f)
            {
                agent.WaitTimer -= 1f;
                continue;
            }

            if (hasPath)
                continue;

            agent.CurrentIntent = IntentType.Wander;

            Node target = PickRandomTarget(walkNodes, agent.CurrentNode);
            if (target == null)
                continue;

            TryAssignPath(agent, target, RandomRange(0.5f, 2f));
        }
    }

    private bool ShouldCommuteToWork(WorldState world, AgentRecord agent)
    {
        if (agent == null || agent.AssignedWorkNode == null)
            return false;

        if (agent.AssignedShiftStartMinute < 0 || agent.AssignedShiftLengthMinutes <= 0)
            return false;

        int commuteStart = Mod1440(agent.AssignedShiftStartMinute - WorkArrivalLeadMinutes);
        int now = world.MinuteOfDay;
        int shiftStart = Mod1440(agent.AssignedShiftStartMinute);

        if (commuteStart < shiftStart)
            return now >= commuteStart && now < shiftStart;

        return now >= commuteStart || now < shiftStart;
    }

    private bool IsOnShift(WorldState world, AgentRecord agent)
    {
        if (agent == null)
            return false;

        if (agent.AssignedShiftStartMinute < 0 || agent.AssignedShiftLengthMinutes <= 0)
            return false;

        int start = Mod1440(agent.AssignedShiftStartMinute);
        int end = Mod1440(agent.AssignedShiftStartMinute + agent.AssignedShiftLengthMinutes);
        int now = world.MinuteOfDay;

        if (agent.AssignedShiftLengthMinutes >= 1440)
            return true;

        if (start < end)
            return now >= start && now < end;

        return now >= start || now < end;
    }

    private int Mod1440(int value)
    {
        int result = value % 1440;
        return result < 0 ? result + 1440 : result;
    }

    private void TryAssignPath(AgentRecord agent, Node target, float failWaitTime)
    {
        if (target == null || agent.CurrentNode == null)
            return;

        if (agent.CurrentNode == target)
        {
            agent.TargetNode = target;
            ClearPath(agent);
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

    private void ClearPath(AgentRecord agent)
    {
        if (agent.CurrentPath == null)
            agent.CurrentPath = new List<Node>();
        else
            agent.CurrentPath.Clear();

        agent.PathIndex = 0;
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