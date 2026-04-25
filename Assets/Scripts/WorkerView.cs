using UnityEngine;

public class WorkerView : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float arriveDistance = 0.02f;
    [SerializeField] private float yOffset = 0.9f;
    [SerializeField] private float rotationSpeed = 720f;

    [Header("Selection")]
    [SerializeField] private GameObject selectionHighlight;
    [SerializeField] private Vector3 selectionHighlightLocalPosition = new Vector3(0f, 2.2f, 0f);
    [SerializeField] private Vector3 selectionHighlightLocalScale = new Vector3(0.8f, 0.8f, 0.8f);

    public AgentRecord Agent { get; private set; }

    private SimpleWorkerLimbSwing limbSwing;
    private bool isSelected;

    private void Awake()
    {
        limbSwing = GetComponent<SimpleWorkerLimbSwing>();
        AlignSelectionHighlight();
        SetSelected(false);
    }

    public void Bind(AgentRecord agent)
    {
        Agent = agent;

        if (agent != null && agent.CurrentNode != null)
            transform.position = GetNodeWorldPosition(agent.CurrentNode);

        AlignSelectionHighlight();
    }

    public void Sync(AgentRecord agent)
    {
        if (agent == null || !agent.IsAlive)
        {
            if (limbSwing != null)
                limbSwing.SetMoving(false);
            return;
        }

        if (agent.CurrentPath == null || agent.CurrentPath.Count == 0)
        {
            if (agent.CurrentNode != null)
                transform.position = GetNodeWorldPosition(agent.CurrentNode);

            if (limbSwing != null)
                limbSwing.SetMoving(false);

            return;
        }

        if (agent.PathIndex < 0 || agent.PathIndex >= agent.CurrentPath.Count)
        {
            if (limbSwing != null)
                limbSwing.SetMoving(false);
            return;
        }

        Node nextNode = agent.CurrentPath[agent.PathIndex];
        if (nextNode == null)
        {
            if (limbSwing != null)
                limbSwing.SetMoving(false);
            return;
        }

        Vector3 targetPos = GetNodeWorldPosition(nextNode);
        Vector3 moveDir = targetPos - transform.position;
        moveDir.y = 0f;

        bool isMovingNow = moveDir.sqrMagnitude > 0.0001f;
        if (limbSwing != null)
            limbSwing.SetMoving(isMovingNow);

        if (isMovingNow)
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

                if (limbSwing != null)
                    limbSwing.SetMoving(false);
            }
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        //Debug.Log(
        //    $"WorkerView '{name}' instance={gameObject.GetInstanceID()} " +
        //    $"agent={(Agent != null ? Agent.AgentId.ToString() : "none")} " +
        //    $"selected={selected} highlightNull={(selectionHighlight == null)}"
        //);

        if (selectionHighlight != null)
        {
            AlignSelectionHighlight();
            selectionHighlight.SetActive(selected);

            //Debug.Log(
            //    $"Highlight '{selectionHighlight.name}' activeSelf={selectionHighlight.activeSelf} " +
            //    $"activeInHierarchy={selectionHighlight.activeInHierarchy} " +
            //    $"worldPos={selectionHighlight.transform.position} " +
            //    $"localPos={selectionHighlight.transform.localPosition} " +
            //    $"localScale={selectionHighlight.transform.localScale}"
            //);
        }
    }

    public bool IsSelected()
    {
        return isSelected;
    }

    private void AlignSelectionHighlight()
    {
        if (selectionHighlight == null)
            return;

        selectionHighlight.transform.SetParent(transform, false);
        selectionHighlight.transform.localPosition = selectionHighlightLocalPosition;
        selectionHighlight.transform.localRotation = Quaternion.identity;
        selectionHighlight.transform.localScale = selectionHighlightLocalScale;
    }

    private Vector3 GetNodeWorldPosition(Node node)
    {
        Vector3 pos = node.transform.position;
        pos.y += yOffset;
        return pos;
    }
}