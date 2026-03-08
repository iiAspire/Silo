using UnityEngine;
using System.Collections.Generic;

public class TowerNavigation : MonoBehaviour
{
    void Start()
    {
        Invoke(nameof(LinkStairs), 0.3f);
    }

    void LinkStairs()
    {
        Node[] nodes = FindObjectsByType<Node>(FindObjectsSortMode.None);

        List<Node> stairs = new List<Node>();

        foreach (Node n in nodes)
        {
            if (n.name.Contains("Stair"))
                stairs.Add(n);
        }

        stairs.Sort((a, b) =>
            a.transform.position.y.CompareTo(b.transform.position.y));

        for (int i = 0; i < stairs.Count - 1; i++)
        {
            Node lower = stairs[i];
            Node upper = stairs[i + 1];

            lower.neighbors.Add(upper);
            upper.neighbors.Add(lower);
        }

        Debug.Log("Linked stair nodes: " + stairs.Count);
    }
}