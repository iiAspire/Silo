using System.Collections.Generic;
using UnityEngine;

public static class WorkplaceAssignmentUtility
{
    public static Node FindBestWorkNodeForJob(string job, List<Node> allNodes)
    {
        WorkplaceType[] validTypes = JobWorkplaceMapper.GetWorkplaceTypesForJob(job);

        Node bestNode = null;
        int bestLoad = int.MaxValue;

        for (int i = 0; i < allNodes.Count; i++)
        {
            Node node = allNodes[i];
            if (node == null)
                continue;

            if (!IsValidForAny(node, validTypes))
                continue;

            if (!node.HasAssignmentCapacity())
                continue;

            int load = node.AssignedWorkerCount;

            if (bestNode == null || load < bestLoad)
            {
                bestNode = node;
                bestLoad = load;
            }
        }

        return bestNode;
    }

    private static bool IsValidForAny(Node node, WorkplaceType[] validTypes)
    {
        for (int i = 0; i < validTypes.Length; i++)
        {
            if (node.workplaceType == validTypes[i])
                return true;
        }

        return false;
    }
}