using UnityEngine;
using System.Collections.Generic;

public class Worker : MonoBehaviour
{
    public Node currentNode;
    Node targetNode;

    public Node homeNode;
    public Node workNode;
    public Node foodNode;

    public float speed = 2f;

    List<Node> path;
    int pathIndex = 0;

    float hunger = 0f;
    float fatigue = 0f;

    void AssignNodes()
    {
        homeNode = FindClosestFreeNode(NodeRegistry.Instance.homes);
        workNode = FindClosestFreeNode(NodeRegistry.Instance.work);
        foodNode = FindClosestFreeNode(NodeRegistry.Instance.food);
    }

    void Start()
    {
        Invoke(nameof(FindClosestNode), 0.2f);
    }

    void Update()
    {
        UpdateNeeds();

        if (targetNode == null)
        {
            ChooseGoal();
        }

        FollowPath();
    }

    void UpdateNeeds()
    {
        hunger += Time.deltaTime * 0.002f;
        fatigue += Time.deltaTime * 0.001f;
    }

    Node FindClosestFreeNode(List<Node> nodes)
    {
        Node best = null;
        float bestDistance = Mathf.Infinity;

        foreach (Node n in nodes)
        {
            if (!n.IsFree())
                continue;

            float d = Vector3.Distance(transform.position, n.transform.position);

            if (d < bestDistance)
            {
                bestDistance = d;
                best = n;
            }
        }

        return best;
    }

    void FindClosestNode()
    {
        Node[] nodes = FindObjectsByType<Node>(FindObjectsSortMode.None);

        float bestDistance = Mathf.Infinity;

        foreach (Node n in nodes)
        {
            float d = Vector3.Distance(transform.position, n.transform.position);

            if (d < bestDistance)
            {
                bestDistance = d;
                currentNode = n;
            }
        }

        if (currentNode != null)
            transform.position = currentNode.transform.position;

        AssignNodes();
    }

    void ChooseGoal()
    {
        Node goal = ChooseScheduledDestination();

        if (goal == null)
            return;

        if (!goal.IsFree())
            return;

        if (!goal.Reserve(this))
            return;

        SetDestination(goal);
    }

Node ChooseScheduledDestination()
{
    float hour = GameTime.Instance.CurrentHour();

    if (hour >= 22 || hour < 6)
        return homeNode;

    if (hour >= 8 && hour < 12)
        return workNode;

    if (hour >= 13 && hour < 18)
        return workNode;

    if (hour >= 6 && hour < 8)
        return foodNode;

    if (hour >= 12 && hour < 13)
        return foodNode;

    return null;
}

    void SetDestination(Node goal)
    {
        List<Node> newPath = NodePathfinder.FindPath(currentNode, goal);

        if (newPath == null || newPath.Count == 0)
        {
            goal.Release(); // cancel reservation
            Debug.Log("No path found");
            return;
        }

        targetNode = goal;
        path = newPath;

        if (path != null)
            Debug.Log("Trying path from " + currentNode.name + " to " + goal.name);

        pathIndex = 0;
    }

    void FollowPath()
    {
        if (path == null || pathIndex >= path.Count)
            return;

        Node nextNode = path[pathIndex];

        transform.position = Vector3.MoveTowards(
            transform.position,
            nextNode.transform.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, nextNode.transform.position) < 0.05f)
        {
            if (currentNode != null)
                currentNode.Release();

            nextNode.Claim(this);

            currentNode = nextNode;
            pathIndex++;

            if (currentNode == targetNode)
            {
                targetNode = null;
            }
        }
    }
}