using UnityEngine;
using System.Collections.Generic;

public enum NodeType
{
    Walk,
    Stair,
    Home,
    Work,
    Food
}

public class Node : MonoBehaviour
{
    public NodeType type = NodeType.Walk;

    public List<Node> neighbors = new List<Node>();

    public Worker occupant;
    public Worker reservedBy;

    public bool IsFree()
    {
        return occupant == null && reservedBy == null;
    }

    public bool Reserve(Worker w)
    {
        if (!IsFree())
            return false;

        reservedBy = w;
        return true;
    }

    public void Claim(Worker w)
    {
        reservedBy = null;
        occupant = w;
    }

    public void Release()
    {
        occupant = null;
    }

    void Start()
    {
        NodeRegistry.Instance.Register(this);

        if (type != NodeType.Walk && type != NodeType.Stair)
            Invoke(nameof(ConnectToNearestWalkNode), 0.2f);
    }

    void ConnectToNearestWalkNode()
    {
        Node closest = null;
        float bestDistance = Mathf.Infinity;

        foreach (Node walk in NodeRegistry.Instance.walk)
        {
            float d = Vector3.Distance(transform.position, walk.transform.position);

            if (d < bestDistance)
            {
                bestDistance = d;
                closest = walk;
            }
        }

        if (closest != null)
        {
            neighbors.Add(closest);
            closest.neighbors.Add(this);
        }
    }

    void OnDrawGizmos()
        {
        switch (type)
            {
                case NodeType.Walk:
                    Gizmos.color = Color.cyan;
                    break;

                case NodeType.Stair:
                    Gizmos.color = Color.yellow;
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
            }

        Gizmos.DrawSphere(transform.position, 0.15f);

        Gizmos.color = Color.white;

        foreach (Node n in neighbors)
        {
            if (n != null)
            {
                Gizmos.DrawLine(transform.position, n.transform.position);
            }
        }
    }
}