using UnityEngine;

public class WorkerView : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float arriveDistance = 0.02f;
    [SerializeField] private float yOffset = 0.9f;
    [SerializeField] private float rotationSpeed = 720f;

    public AgentRecord Agent { get; private set; }

    private SimpleWorkerLimbSwing limbSwing;

    private void Awake()
    {
        limbSwing = GetComponent<SimpleWorkerLimbSwing>();
    }

    public void Bind(AgentRecord agent)
    {
        Agent = agent;

        if (agent != null && agent.CurrentNode != null)
            transform.position = GetNodeWorldPosition(agent.CurrentNode);
    }

    public void Sync(AgentRecord agent)
    {
        if (agent == null || !agent.IsAlive)
        {
            if (limbSwing != null) limbSwing.SetMoving(false);
            return;
        }

        if (agent.CurrentPath == null || agent.CurrentPath.Count == 0)
        {
            if (agent.CurrentNode != null)
                transform.position = GetNodeWorldPosition(agent.CurrentNode);

            if (limbSwing != null) limbSwing.SetMoving(false);
            return;
        }

        if (agent.PathIndex < 0 || agent.PathIndex >= agent.CurrentPath.Count)
        {
            if (limbSwing != null) limbSwing.SetMoving(false);
            return;
        }

        Node nextNode = agent.CurrentPath[agent.PathIndex];
        if (nextNode == null)
        {
            if (limbSwing != null) limbSwing.SetMoving(false);
            return;
        }

        Vector3 targetPos = GetNodeWorldPosition(nextNode);
        Vector3 moveDir = targetPos - transform.position;
        moveDir.y = 0f;

        bool isMoving = moveDir.sqrMagnitude > 0.0001f;
        if (limbSwing != null) limbSwing.SetMoving(isMoving);

        if (isMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) <= arriveDistance)
        {
            transform.position = targetPos;
            agent.CurrentNode = nextNode;
            agent.PathIndex++;

            if (agent.PathIndex >= agent.CurrentPath.Count)
            {
                agent.CurrentPath.Clear();
                agent.PathIndex = 0;
                if (limbSwing != null) limbSwing.SetMoving(false);
            }
        }
    }

    private Vector3 GetNodeWorldPosition(Node node)
    {
        Vector3 pos = node.transform.position;
        pos.y += yOffset;
        return pos;
    }
}