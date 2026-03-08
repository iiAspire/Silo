using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
    public int slotCount = 8;
    public float radius = 3f;
    public GameObject slotPrefab;
    public GameObject stairNodePrefab;

    List<Node> nodes = new List<Node>();

    void Start()
    {
        GenerateSlots();
        ConnectNodes();
    }

    void GenerateSlots()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        nodes.Clear();

        for (int i = 0; i < slotCount; i++)
        {
            float angle = i * Mathf.PI * 2 / slotCount;

            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );

            GameObject slot = Instantiate(slotPrefab, transform);
            slot.transform.localPosition = pos;

            nodes.Add(slot.GetComponent<Node>());
        }

        GameObject stair = Instantiate(stairNodePrefab, transform);
        stair.transform.localPosition = Vector3.zero;

        nodes.Add(stair.GetComponent<Node>());
    }

    void ConnectNodes()
    {
        int ringCount = nodes.Count - 1; // last node is the stair node
        Node stairNode = nodes[ringCount];

        // Connect nodes in a circular ring
        for (int i = 0; i < ringCount; i++)
        {
            Node current = nodes[i];
            Node next = nodes[(i + 1) % ringCount];
            Node prev = nodes[(i - 1 + ringCount) % ringCount];

            current.neighbors.Add(next);
            current.neighbors.Add(prev);
        }

        // Connect stair node to two ring nodes
        Node entryA = nodes[0];
        Node entryB = nodes[ringCount / 2];

        entryA.neighbors.Add(stairNode);
        entryB.neighbors.Add(stairNode);

        stairNode.neighbors.Add(entryA);
        stairNode.neighbors.Add(entryB);
    }
}