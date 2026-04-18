using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
    public int slotCount = 8;
    public float radius = 3f;
    public GameObject slotPrefab;
    public GameObject stairNodePrefab;

    private readonly List<Node> nodes = new List<Node>();

    public void Initialize()
    {
        //Debug.Log($"Initializing {name} | slotPrefab={(slotPrefab ? slotPrefab.name : "NULL")} | stairNodePrefab={(stairNodePrefab ? stairNodePrefab.name : "NULL")}");
        GenerateSlots();
        ConnectNodes();
        //Debug.Log($"{name} created {nodes.Count} nodes.");
    }

    private void GenerateSlots()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        nodes.Clear();

        for (int i = 0; i < slotCount; i++)
        {
            float angle = i * Mathf.PI * 2f / slotCount;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

            GameObject slot = Instantiate(slotPrefab, transform);
            slot.transform.localPosition = pos;
            Node node = slot.GetComponent<Node>();

            if (node == null)
                Debug.LogError($"{name}: slotPrefab has no Node component.");
            else
                nodes.Add(node);
        }

        GameObject stair = Instantiate(stairNodePrefab, transform);
        stair.transform.localPosition = Vector3.zero;
        Node stairNode = stair.GetComponent<Node>();

        if (stairNode == null)
            Debug.LogError($"{name}: stairNodePrefab has no Node component.");
        else
            nodes.Add(stairNode);
    }

    private void ConnectNodes()
    {
        int ringCount = nodes.Count - 1;
        if (ringCount <= 0)
            return;

        Node stairNode = nodes[ringCount];

        for (int i = 0; i < ringCount; i++)
        {
            Node current = nodes[i];
            Node next = nodes[(i + 1) % ringCount];
            Node prev = nodes[(i - 1 + ringCount) % ringCount];

            if (!current.neighbors.Contains(next)) current.neighbors.Add(next);
            if (!current.neighbors.Contains(prev)) current.neighbors.Add(prev);
        }

        Node entryA = nodes[0];
        Node entryB = nodes[ringCount / 2];

        if (!entryA.neighbors.Contains(stairNode)) entryA.neighbors.Add(stairNode);
        if (!entryB.neighbors.Contains(stairNode)) entryB.neighbors.Add(stairNode);
        if (!stairNode.neighbors.Contains(entryA)) stairNode.neighbors.Add(entryA);
        if (!stairNode.neighbors.Contains(entryB)) stairNode.neighbors.Add(entryB);
    }
}