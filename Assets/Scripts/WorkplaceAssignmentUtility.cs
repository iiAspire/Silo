using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class WorkplaceAssignmentUtility
{
    public static Node FindPrimaryWorkNodeForAgent(
        AgentRecord agent,
        JobDefinition primaryJob,
        List<Node> allNodes)
    {
        if (agent == null || primaryJob == null || allNodes == null || allNodes.Count == 0)
            return null;

        WorkplaceType[] validTypes = JobWorkplaceMapper.GetWorkplaceTypesForJob(agent.BaseJob);
        if (validTypes == null || validTypes.Length == 0)
            return null;

        int shiftIndex = agent.AssignedShiftIndex;
        int shiftCount = primaryJob.ShiftCount;

        if (shiftIndex < 0 || shiftCount <= 0 || shiftIndex >= shiftCount)
            return null;

        Node bestNode = null;
        float bestScore = float.MaxValue;

        var workplaceTypes = JobWorkplaceMapper.GetWorkplaceTypesForJob(agent.BaseJob);

        //Debug.Log(
        //    $"Mapper Agent={agent.AgentId} Job='{agent.Job}' BaseJob='{agent.BaseJob}' " +
        //    $"Workplaces=[{string.Join(", ", workplaceTypes)}]"
        //);

        var candidates = allNodes
            .Where(n =>
                n != null &&
                n.type == NodeType.Work &&
                workplaceTypes.Contains(n.workplaceType))
            .ToList();  

        //Debug.Log(
        //    $"Primary candidates for Agent={agent.AgentId} BaseJob='{agent.BaseJob}': " +
        //    $"{candidates.Count} => {string.Join(", ", candidates.Select(c => $"{c.name}({c.workplaceType})"))}"
        //);

        for (int i = 0; i < allNodes.Count; i++)
        {
            Node node = allNodes[i];
            if (!IsCandidateNode(node, validTypes))
                continue;

            node.EnsureShiftStaffing(shiftCount);

            if (!node.HasOpenPrimarySlot(shiftIndex, shiftCount))
                continue;

            float score = ScorePrimaryNode(agent, node, shiftIndex, shiftCount);

            if (bestNode == null || score < bestScore)
            {
                bestNode = node;
                bestScore = score;
            }
        }

        return bestNode;
    }

    public static Node FindShadowWorkNodeForAgent(
        AgentRecord agent,
        JobDefinition primaryJob,
        List<Node> allNodes,
        int maxShadowsForRole,
        int currentShadowsForRole,
        int maxShadowsPerNodeTotal)
    {
        if (agent == null || !agent.IsShadowWorker || primaryJob == null || allNodes == null || allNodes.Count == 0)
            return null;

        if (currentShadowsForRole >= maxShadowsForRole)
            return null;

        WorkplaceType[] validTypes = JobWorkplaceMapper.GetWorkplaceTypesForJob(agent.BaseJob);
        if (validTypes == null || validTypes.Length == 0)
            return null;

        int shiftIndex = agent.AssignedShiftIndex;
        int shiftCount = primaryJob.ShiftCount;

        if (shiftIndex < 0 || shiftCount <= 0 || shiftIndex >= shiftCount)
            return null;

        Node bestNode = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < allNodes.Count; i++)
        {
            Node node = allNodes[i];
            if (!IsCandidateNode(node, validTypes))
                continue;

            node.EnsureShiftStaffing(shiftCount);

            if (node.GetPrimaryAgentId(shiftIndex) < 0)
                continue;

            if (!node.HasOpenShadowSlot(shiftIndex, shiftCount, maxShadowsPerNodeTotal))
                continue;

            float score = ScoreShadowNode(agent, node, shiftIndex);

            if (bestNode == null || score < bestScore)
            {
                bestNode = node;
                bestScore = score;
            }
        }

        return bestNode;
    }

    private static bool IsCandidateNode(Node node, WorkplaceType[] validTypes)
    {
        if (node == null)
            return false;

        if (node.type != NodeType.Work)
            return false;

        for (int i = 0; i < validTypes.Length; i++)
        {
            if (node.workplaceType == validTypes[i])
                return true;
        }

        return false;
    }

    private static float ScorePrimaryNode(
        AgentRecord agent,
        Node node,
        int shiftIndex,
        int shiftCount)
    {
        float distanceScore = GetHomeDistance(agent, node);
        float totalLoadScore = node.AssignedWorkerCount * 10f;
        float shiftLoadScore = GetAssignedCountForShift(node, shiftIndex, shiftCount) * 25f;

        return distanceScore + totalLoadScore + shiftLoadScore;
    }

    private static float ScoreShadowNode(
        AgentRecord agent,
        Node node,
        int shiftIndex)
    {
        float distanceScore = GetHomeDistance(agent, node);
        float totalShadowLoadScore = node.TotalAssignedShadows() * 20f;
        float sameShiftShadowPenalty = GetShadowCountForShift(node, shiftIndex) * 30f;

        return distanceScore + totalShadowLoadScore + sameShiftShadowPenalty;
    }

    private static float GetHomeDistance(AgentRecord agent, Node node)
    {
        if (agent == null || agent.AssignedHomeNode == null || node == null)
            return 0f;

        return Vector3.Distance(agent.AssignedHomeNode.transform.position, node.transform.position);
    }

    private static int GetAssignedCountForShift(Node node, int shiftIndex, int shiftCount)
    {
        if (node == null)
            return 0;

        node.EnsureShiftStaffing(shiftCount);

        int count = 0;

        if (node.GetPrimaryAgentId(shiftIndex) >= 0)
            count++;

        List<int> shadows = node.GetShadowAgentIds(shiftIndex);
        if (shadows != null)
            count += shadows.Count;

        return count;
    }

    private static int GetShadowCountForShift(Node node, int shiftIndex)
    {
        if (node == null)
            return 0;

        List<int> shadows = node.GetShadowAgentIds(shiftIndex);
        return shadows != null ? shadows.Count : 0;
    }
}