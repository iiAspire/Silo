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

public class Node : MonoBehaviour
{
    public NodeType type = NodeType.Walk;
    public List<Node> neighbors = new List<Node>();

    public WorkerView occupant;
    public WorkerView reservedBy;

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

    private void OnDrawGizmos()
    {
        switch (type)
        {
            case NodeType.Walk: Gizmos.color = Color.cyan; break;
            case NodeType.Stair: Gizmos.color = Color.black; break;
            case NodeType.Home: Gizmos.color = Color.blue; break;
            case NodeType.Work: Gizmos.color = Color.green; break;
            case NodeType.Food: Gizmos.color = Color.red; break;
            case NodeType.Clinic: Gizmos.color = Color.lightPink; break;
            case NodeType.Hospital: Gizmos.color = Color.deepPink; break;
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