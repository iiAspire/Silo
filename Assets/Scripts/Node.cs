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
    SecurityOffice,
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
    [System.Serializable]
    public class ShiftStaffing
    {
        public int PrimaryAgentId = -1;
        public List<int> ShadowAgentIds = new List<int>();
    }

    public NodeType type = NodeType.Walk;
    public WorkplaceType workplaceType = WorkplaceType.None;

    [Header("Movement / occupancy")]
    public int workerCapacity = 1;
    public WorkerView occupant;
    public WorkerView reservedBy;

    [Header("Graph")]
    public List<Node> neighbors = new List<Node>();

    [Header("Assignment state")]
    [SerializeField] private List<int> assignedAgentIds = new List<int>();
    [SerializeField] private List<ShiftStaffing> shiftStaffing = new List<ShiftStaffing>();

    public int AssignedWorkerCount => assignedAgentIds.Count;

    private void Awake()
    {
        if (NodeRegistry.Instance != null)
            NodeRegistry.Instance.Register(this);
    }

    public bool IsFree()
    {
        return occupant == null && reservedBy == null;
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

    public void AssignWorker(int agentId)
    {
        if (!assignedAgentIds.Contains(agentId))
            assignedAgentIds.Add(agentId);
    }

    public void UnassignWorker(int agentId)
    {
        assignedAgentIds.Remove(agentId);

        for (int i = 0; i < shiftStaffing.Count; i++)
        {
            if (shiftStaffing[i].PrimaryAgentId == agentId)
                shiftStaffing[i].PrimaryAgentId = -1;

            shiftStaffing[i].ShadowAgentIds.Remove(agentId);
        }
    }

    public void ClearAllAssignments()
    {
        assignedAgentIds.Clear();

        for (int i = 0; i < shiftStaffing.Count; i++)
        {
            shiftStaffing[i].PrimaryAgentId = -1;
            shiftStaffing[i].ShadowAgentIds.Clear();
        }
    }

    public void EnsureShiftStaffing(int shiftCount)
    {
        if (shiftCount < 0)
            shiftCount = 0;

        while (shiftStaffing.Count < shiftCount)
            shiftStaffing.Add(new ShiftStaffing());

        while (shiftStaffing.Count > shiftCount)
            shiftStaffing.RemoveAt(shiftStaffing.Count - 1);
    }

    public bool HasOpenPrimarySlot(int shiftIndex, int shiftCount)
    {
        if (!IsValidShiftIndex(shiftIndex, shiftCount))
            return false;

        EnsureShiftStaffing(shiftCount);
        return shiftStaffing[shiftIndex].PrimaryAgentId < 0;
    }

    public bool TryAssignPrimary(int agentId, int shiftIndex, int shiftCount)
    {
        if (!HasOpenPrimarySlot(shiftIndex, shiftCount))
            return false;

        ShiftStaffing slot = shiftStaffing[shiftIndex];

        if (slot.PrimaryAgentId >= 0 && slot.PrimaryAgentId != agentId)
            return false;

        slot.PrimaryAgentId = agentId;
        AssignWorker(agentId);
        return true;
    }

    public bool HasOpenShadowSlot(int shiftIndex, int shiftCount, int maxShadowsPerNodeTotal)
    {
        if (!IsValidShiftIndex(shiftIndex, shiftCount))
            return false;

        EnsureShiftStaffing(shiftCount);

        if (maxShadowsPerNodeTotal < 0)
            return false;

        return TotalAssignedShadows() < maxShadowsPerNodeTotal;
    }

    public bool TryAssignShadow(int agentId, int shiftIndex, int shiftCount, int maxShadowsPerNodeTotal)
    {
        if (!HasOpenShadowSlot(shiftIndex, shiftCount, maxShadowsPerNodeTotal))
            return false;

        ShiftStaffing slot = shiftStaffing[shiftIndex];

        if (!slot.ShadowAgentIds.Contains(agentId))
            slot.ShadowAgentIds.Add(agentId);

        AssignWorker(agentId);
        return true;
    }

    public int GetPrimaryAgentId(int shiftIndex)
    {
        if (shiftIndex < 0 || shiftIndex >= shiftStaffing.Count)
            return -1;

        return shiftStaffing[shiftIndex].PrimaryAgentId;
    }

    public List<int> GetShadowAgentIds(int shiftIndex)
    {
        if (shiftIndex < 0 || shiftIndex >= shiftStaffing.Count)
            return null;

        return shiftStaffing[shiftIndex].ShadowAgentIds;
    }

    public int TotalAssignedShadows()
    {
        int total = 0;

        for (int i = 0; i < shiftStaffing.Count; i++)
            total += shiftStaffing[i].ShadowAgentIds.Count;

        return total;
    }

    public int TotalAssignedPrimaries()
    {
        int total = 0;

        for (int i = 0; i < shiftStaffing.Count; i++)
        {
            if (shiftStaffing[i].PrimaryAgentId >= 0)
                total++;
        }

        return total;
    }

    public bool HasAnyAssignments()
    {
        return assignedAgentIds.Count > 0;
    }

    public bool IsAssigned(int agentId)
    {
        return assignedAgentIds.Contains(agentId);
    }

    private bool IsValidShiftIndex(int shiftIndex, int shiftCount)
    {
        if (shiftCount <= 0)
            return false;

        if (shiftIndex < 0 || shiftIndex >= shiftCount)
            return false;

        return true;
    }

    private void OnDrawGizmos()
    {
        switch (type)
        {
            case NodeType.Walk:
                Gizmos.color = Color.cyan;
                break;
            case NodeType.Stair:
                Gizmos.color = Color.black;
                break;
            case NodeType.Home:
                Gizmos.color = Color.blue;
                break;
            case NodeType.Work:
                Gizmos.color = Color.green;
                break;
            case NodeType.Food:
                Gizmos.color = Color.red;
                break;
            case NodeType.Clinic:
                Gizmos.color = new Color(1f, 0.6f, 0.8f);
                break;
            case NodeType.Hospital:
                Gizmos.color = new Color(1f, 0.1f, 0.5f);
                break;
            case NodeType.Recreation:
                Gizmos.color = Color.yellow;
                break;
        }

        Gizmos.DrawSphere(transform.position, 0.15f);
        Gizmos.color = Color.white;

        for (int i = 0; i < neighbors.Count; i++)
        {
            Node n = neighbors[i];
            if (n != null)
                Gizmos.DrawLine(transform.position, n.transform.position);
        }
    }
}