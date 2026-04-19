using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    Walk,
    Stair,
    Home,
    Work,
    Food,
    Clinic,
    Hospital,
    Recreation
}

public enum WorkplaceType
{
    None,
    Generator,
    Canteen,
    JanitorOffice,
    Security,
    Farm,
    Clinic,
    Maintenance,
    School,
    JudgesChambers,
    MayorsOffice,
    SheriffStation,
    Butcher,
    Baker,
    Builder,
    Carpenter,
    IT,
    Bazaar,
    Hospital,
    PorterHub,
    Manufacturing,
    Processing,
    Supply,
    WasteManagement,
    Recycling,
    Prison,
    Nursery
}

public class Node : MonoBehaviour
{
    public NodeType type = NodeType.Walk;
    public WorkplaceType workplaceType = WorkplaceType.None;
    public int workerCapacity = 1;

    public List<Node> neighbors = new List<Node>();

    public WorkerView occupant;
    public WorkerView reservedBy;

    [SerializeField] private List<int> assignedAgentIds = new List<int>();
    public int AssignedWorkerCount => assignedAgentIds.Count;

    public bool IsFree()
    {
        return occupant == null && reservedBy == null;
    }

    public bool HasAssignmentCapacity()
    {
        return workerCapacity <= 0 || assignedAgentIds.Count < workerCapacity;
    }

    public bool IsValidWorkNodeFor(WorkplaceType requiredType)
    {
        return workplaceType == requiredType;
    }

    public void AssignWorker(int agentId)
    {
        if (!assignedAgentIds.Contains(agentId))
            assignedAgentIds.Add(agentId);
    }

    public void UnassignWorker(int agentId)
    {
        assignedAgentIds.Remove(agentId);
    }

    public bool Reserve(WorkerView worker)
    {
        if (!IsFree())
            return false;

        reservedBy = worker;
        return true;
    }

    public void Claim(WorkerView worker)
    {
        reservedBy = null;
        occupant = worker;
    }

    public void ReleaseOccupant(WorkerView worker = null)
    {
        if (worker == null || occupant == worker)
            occupant = null;
    }

    public void ReleaseReservation(WorkerView worker = null)
    {
        if (worker == null || reservedBy == worker)
            reservedBy = null;
    }

    private void OnDrawGizmos()
    {
        switch (type)
        {
            case NodeType.Walk: Gizmos.color = Color.cyan; break;
            case NodeType.Stair: Gizmos.color = Color.black; break;
            case NodeType.Home: Gizmos.color = Color.blue; break;
            case NodeType.Work: Gizmos.color = Color.green; break;
            case NodeType.Food: Gizmos.color = Color.red; break;
            case NodeType.Clinic: Gizmos.color = new Color(1f, 0.6f, 0.8f); break;
            case NodeType.Hospital: Gizmos.color = new Color(1f, 0.1f, 0.5f); break;
            case NodeType.Recreation: Gizmos.color = Color.yellow; break;
        }

        Gizmos.DrawSphere(transform.position, 0.15f);
        Gizmos.color = Color.white;

        foreach (Node n in neighbors)
        {
            if (n != null)
                Gizmos.DrawLine(transform.position, n.transform.position);
        }
    }
}