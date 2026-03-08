using System.Collections.Generic;
using UnityEngine;

public class NodeRegistry : MonoBehaviour
{
    public static NodeRegistry Instance;

    public List<Node> homes = new List<Node>();
    public List<Node> work = new List<Node>();
    public List<Node> food = new List<Node>();
    public List<Node> walk = new List<Node>();
    public List<Node> stairs = new List<Node>();

    void Awake()
    {
        Instance = this;
    }

    public void Register(Node node)
    {
        switch (node.type)
        {
            case NodeType.Home: homes.Add(node); break;
            case NodeType.Work: work.Add(node); break;
            case NodeType.Food: food.Add(node); break;
            case NodeType.Stair: stairs.Add(node); break;
            case NodeType.Walk: walk.Add(node); break;
        }
    }
}