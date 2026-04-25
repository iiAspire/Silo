using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeRegistry : MonoBehaviour
{
    public static NodeRegistry Instance { get; private set; }

    public readonly List<Node> homes = new List<Node>();
    public readonly List<Node> work = new List<Node>();
    public readonly List<Node> food = new List<Node>();
    public readonly List<Node> walk = new List<Node>();
    public readonly List<Node> stairs = new List<Node>();
    public readonly List<Node> all = new List<Node>();
    public readonly List<Node> clinic = new List<Node>();
    public readonly List<Node> hospital = new List<Node>();
    public readonly List<Node> recreation = new List<Node>();

    private void Awake()
    {
        Instance = this;
        Rebuild();
    }

    public void ClearAll()
    {
        homes.Clear();
        work.Clear();
        food.Clear();
        walk.Clear();
        stairs.Clear();
        all.Clear();
        clinic.Clear();
        hospital.Clear();
        recreation.Clear();
    }

    public void Register(Node node)
    {
        if (node == null || all.Contains(node))
            return;

        all.Add(node);

        switch (node.type)
        {
            case NodeType.Home: homes.Add(node); break;
            case NodeType.Work: work.Add(node); break;
            case NodeType.Food: food.Add(node); break;
            case NodeType.Stair: stairs.Add(node); break;
            case NodeType.Walk: walk.Add(node); break;
            case NodeType.Clinic: clinic.Add(node); break;
            case NodeType.Hospital: hospital.Add(node); break;
            case NodeType.Recreation: recreation.Add(node); break;
        }
    }

    public void Rebuild()
    {
        ClearAll();

        Node[] nodes = FindObjectsByType<Node>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var node in nodes)
            Register(node);

        //Debug.Log($"NodeRegistry rebuilt. Total: {all.Count}, Walk: {walk.Count}, Stair: {stairs.Count}, Home: {homes.Count}, Work: {work.Count}, Food: {food.Count}, " +
        //    $"Recreation: {recreation.Count}, Hospital: {hospital.Count}, Clinic: {clinic.Count}");
    }
}