using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkerInspectorPanel : MonoBehaviour
{
    public enum Tab
    {
        Overview,
        Relationships,
        Stats
    }

    [Header("Root")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject panelRoot;

    [Header("Header")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text metaText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Sprite defaultPortrait;

    [Header("Tab Buttons")]
    [SerializeField] private Button overviewTabButton;
    [SerializeField] private Button relationshipsTabButton;
    [SerializeField] private Button statsTabButton;
    [SerializeField] private Image overviewTabHighlight;
    [SerializeField] private Image relationshipsTabHighlight;
    [SerializeField] private Image statsTabHighlight;

    [Header("Tab Panels")]
    [SerializeField] private GameObject overviewTab;
    [SerializeField] private GameObject relationshipsTab;
    [SerializeField] private GameObject statsTab;

    [Header("Tab Content")]
    [SerializeField] private TMP_Text overviewText;
    [SerializeField] private TMP_Text relationshipsText;
    [SerializeField] private TMP_Text statsText;

    private WorkerView selectedWorker;
    private Tab currentTab = Tab.Overview;
    private bool isPinned;
    private bool isTargetLocked;
    public bool IsPinned => isPinned;                   // keeps card inspector open
    public bool IsTargetLocked => isTargetLocked;       // not yet implemented, will allow player to prevent card change to new worker
    public WorkerView SelectedWorker => selectedWorker;
    public GameObject PanelRoot => panelRoot;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (overviewTabButton != null)
            overviewTabButton.onClick.AddListener(() => ShowTab(Tab.Overview));

        if (relationshipsTabButton != null)
            relationshipsTabButton.onClick.AddListener(() => ShowTab(Tab.Relationships));

        if (statsTabButton != null)
            statsTabButton.onClick.AddListener(() => ShowTab(Tab.Stats));

        HideImmediate();
        ShowTab(Tab.Overview);
    }

    private void Update()
    {
        if (selectedWorker != null)
            Refresh();
    }

    public void Show(WorkerView worker)
    {
        if (worker == null)
            return;

        selectedWorker = worker;

        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        Refresh();
    }

    private string GetNodeDisplayName(Node node)
    {
        if (node == null)
            return "None";

        var placedRoom = node.GetComponentInParent<ProceduralRoomPlacer.PlacedRoomInstance>();
        if (placedRoom != null && !string.IsNullOrEmpty(placedRoom.DefinitionName))
            return placedRoom.DefinitionName;

        if (node.transform.parent != null)
            return node.transform.parent.name;

        return node.name;
    }

    public void Hide()
    {
        if (isPinned)
            return;

        selectedWorker = null;
        HideImmediate();
    }

    public void ForceHide()
    {
        selectedWorker = null;
        isPinned = false;
        HideImmediate();
    }

    public void TogglePin()
    {
        isPinned = !isPinned;
        Refresh();
    }

    public void ShowTab(Tab tab)
    {
        currentTab = tab;

        if (overviewTab != null) overviewTab.SetActive(tab == Tab.Overview);
        if (relationshipsTab != null) relationshipsTab.SetActive(tab == Tab.Relationships);
        if (statsTab != null) statsTab.SetActive(tab == Tab.Stats);

        if (overviewTabHighlight != null) overviewTabHighlight.enabled = (tab == Tab.Overview);
        if (relationshipsTabHighlight != null) relationshipsTabHighlight.enabled = (tab == Tab.Relationships);
        if (statsTabHighlight != null) statsTabHighlight.enabled = (tab == Tab.Stats);
    }

    private void HideImmediate()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void Refresh()
    {
        if (selectedWorker == null || selectedWorker.Agent == null)
        {
            SetEmpty();
            return;
        }

        AgentRecord agent = selectedWorker.Agent;
        WorldState world = SimulationManager.Instance != null ? SimulationManager.Instance.World : null;

        if (nameText != null)
            nameText.text = GetDisplayName(agent);

        if (metaText != null)
            metaText.text = GetMetaLine(agent);

        if (portraitImage != null && defaultPortrait != null)
            portraitImage.sprite = defaultPortrait;

        if (overviewText != null)
            overviewText.text = BuildOverview(agent, world);

        if (relationshipsText != null)
            relationshipsText.text = BuildRelationships(agent);

        if (statsText != null)
            statsText.text = BuildStats(agent);
    }

    private void SetEmpty()
    {
        if (nameText != null)
            nameText.text = "No Selection";

        if (metaText != null)
            metaText.text = "";

        if (portraitImage != null && defaultPortrait != null)
            portraitImage.sprite = defaultPortrait;

        if (overviewText != null)
            overviewText.text = "";
        if (relationshipsText != null)
            relationshipsText.text = "";
        if (statsText != null)
            statsText.text = "";
    }

    private string GetDisplayName(AgentRecord agent)
    {
        if (!string.IsNullOrWhiteSpace(agent.Name))
            return agent.Name;

        return $"Worker {agent.AgentId}";
    }

    private string GetMetaLine(AgentRecord agent)
    {
        string sex = string.IsNullOrWhiteSpace(agent.Sex) ? "Unknown" : agent.Sex;
        string age = agent.Age > 0 ? agent.Age.ToString() : "?";
        string household = agent.HouseholdID >= 0 ? $"Household {agent.HouseholdID}" : "No Household";
        string role = string.IsNullOrWhiteSpace(agent.HouseholdRole) ? "No Role" : agent.HouseholdRole;

        return $"{sex} • {age} • {household} • {role}";
    }

    private string BuildOverview(AgentRecord agent, WorldState world)
    {
        string job = string.IsNullOrWhiteSpace(agent.Job) ? "None" : agent.Job;
        string intent = agent.CurrentIntent.ToString();
        string onShift = world != null && IsOnShift(world, agent) ? "Yes" : "No";

        string currentErrand = agent.ActiveErrand != null && !string.IsNullOrWhiteSpace(agent.ActiveErrand.CurrentErrand)
            ? agent.ActiveErrand.CurrentErrand
            : "None";

        string priority = agent.ActiveErrand != null
            ? agent.ActiveErrand.Priority.ToString()
            : "None";

        string targetAgent = agent.ActiveErrand != null && !string.IsNullOrWhiteSpace(agent.ActiveErrand.TargetAgentID)
            ? agent.ActiveErrand.TargetAgentID
            : "None";

        return
            $"ID: {agent.AgentId}\n" +
            $"Job: {job}\n" +
            $"Intent: {intent}\n" +
            $"On Shift: {onShift}\n" +
            $"Current Errand: {currentErrand}\n" +
            $"Priority: {priority}\n" +
            $"Target Agent: {targetAgent}\n" +
            $"Current Node: {(agent.CurrentNode != null ? agent.CurrentNode.name : "None")}\n" +
            $"Target Node: {(agent.TargetNode != null ? agent.TargetNode.name : "None")}\n" +
            $"Work Node: {(agent.AssignedWorkNode != null ? GetNodeDisplayName(agent.AssignedWorkNode) : "None")}\n" +
            $"Home Node: {(agent.AssignedHomeNode != null ? agent.AssignedHomeNode.name : "None")}";
    }

    private string BuildRelationships(AgentRecord agent)
    {
        if (agent.Relationships == null || agent.Relationships.Count == 0)
        {
            return
                "Partner: None\n" +
                "Children: 0\n" +
                "Relationship Count: 0\n" +
                "Closest Tie: None";
        }

        RelationshipLink partnerLink = null;
        RelationshipLink strongestLink = null;

        for (int i = 0; i < agent.Relationships.Count; i++)
        {
            RelationshipLink link = agent.Relationships[i];
            if (link == null)
                continue;

            if (partnerLink == null && link.Type == RelationshipType.Partner)
                partnerLink = link;

            if (strongestLink == null || link.Strength > strongestLink.Strength)
                strongestLink = link;
        }

        string partner = partnerLink != null ? $"Worker {partnerLink.OtherAgentId}" : "None";
        string children = partnerLink != null && !string.IsNullOrWhiteSpace(partnerLink.ChildrenCount)
            ? partnerLink.ChildrenCount
            : "0";

        string closestTie = strongestLink != null
            ? $"{strongestLink.Type} with Worker {strongestLink.OtherAgentId} ({strongestLink.Strength:0.00})"
            : "None";

        return
            $"Partner: {partner}\n" +
            $"Children: {children}\n" +
            $"Relationship Count: {agent.Relationships.Count}\n" +
            $"Closest Tie: {closestTie}";
    }

    private string BuildStats(AgentRecord agent)
    {
        return
            $"Health: {agent.Health}\n" +
            $"Hunger: {agent.Hunger}\n" +
            $"Fatigue: {agent.Fatigue}\n" +
            $"Happiness: {agent.Happiness}\n" +
            $"Path Index: {agent.PathIndex}\n" +
            $"Path Count: {(agent.CurrentPath != null ? agent.CurrentPath.Count : 0)}\n" +
            $"Wait Timer: {agent.WaitTimer:0.0}";
    }

    private bool IsOnShift(WorldState world, AgentRecord agent)
    {
        if (agent.AssignedShiftStartMinute < 0 || agent.AssignedShiftLengthMinutes <= 0)
            return false;

        int start = agent.AssignedShiftStartMinute;
        int end = (start + agent.AssignedShiftLengthMinutes) % 1440;
        int now = world.MinuteOfDay;

        if (start < end)
            return now >= start && now < end;

        return now >= start || now < end;
    }
}