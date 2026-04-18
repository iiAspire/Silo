using System.Collections.Generic;
using UnityEngine;

public class TowerNavigation : MonoBehaviour
{
    private void Start()
    {
        Invoke(nameof(LinkStairs), 0.3f);
    }

    private void LinkStairs()
    {
        Node[] nodes = FindObjectsByType<Node>(FindObjectsSortMode.None);
        List<Node> stairs = new List<Node>();

        foreach (Node n in nodes)
        {
            if (n != null && n.type == NodeType.Stair)
                stairs.Add(n);
        }

        stairs.Sort((a, b) => a.transform.position.y.CompareTo(b.transform.position.y));

        for (int i = 0; i < stairs.Count - 1; i++)
        {
            Node lower = stairs[i];
            Node upper = stairs[i + 1];

            if (!lower.neighbors.Contains(upper)) lower.neighbors.Add(upper);
            if (!upper.neighbors.Contains(lower)) upper.neighbors.Add(lower);
        }
    }
}