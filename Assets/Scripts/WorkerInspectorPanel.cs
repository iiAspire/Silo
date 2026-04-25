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

    [Header("Selection Tools")]
    [SerializeField] private AgentSelectionManager selectionManager;
    [SerializeField] private TMP_InputField agentIdInput;
    [SerializeField] private Button goToSelectedButton;
    [SerializeField] private Button goToTypedIdButton;
    [SerializeField] private Button fillTypedIdFromSelectedButton;

    private WorkerView selectedWorker;
    private Tab currentTab = Tab.Overview;
    private bool isPinned;
    private bool isTargetLocked;

    public bool IsPinned => isPinned;
    public bool IsTargetLocked => isTargetLocked;
    public WorkerView SelectedWorker => selectedWorker;
    public GameObject PanelRoot => panelRoot;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        //Debug.Log(
        //    $"InspectorPanel Awake | " +
        //    $"overviewBtn={(overviewTabButton != null)} " +
        //    $"relationshipsBtn={(relationshipsTabButton != null)} " +
        //    $"statsBtn={(statsTabButton != null)} " +
        //    $"goToSelectedBtn={(goToSelectedButton != null)} " +
        //    $"goToTypedIdBtn={(goToTypedIdButton != null)} " +
        //    $"fillTypedBtn={(fillTypedIdFromSelectedButton != null)} " +
        //    $"selectionManager={(selectionManager != null)} " +
        //    $"agentIdInput={(agentIdInput != null)}"
        //);

        if (overviewTabButton != null)
            overviewTabButton.onClick.AddListener(() =>
            {
                //Debug.Log("Overview tab button clicked");
                ShowTab(Tab.Overview);
            });

        if (relationshipsTabButton != null)
            relationshipsTabButton.onClick.AddListener(() =>
            {
                //Debug.Log("Relationships tab button clicked");
                ShowTab(Tab.Relationships);
            });

        if (statsTabButton != null)
            statsTabButton.onClick.AddListener(() =>
            {
                //Debug.Log("Stats tab button clicked");
                ShowTab(Tab.Stats);
            });

        if (goToSelectedButton != null)
            goToSelectedButton.onClick.AddListener(() =>
            {
                //Debug.Log("GoToSelected button clicked");
                FocusSelectedAgent();
            });

        if (goToTypedIdButton != null)
            goToTypedIdButton.onClick.AddListener(() =>
            {
                //Debug.Log("GoToTypedId button clicked");
                FocusTypedAgentId();
            });

        if (fillTypedIdFromSelectedButton != null)
            fillTypedIdFromSelectedButton.onClick.AddListener(() =>
            {
                //Debug.Log("FillTypedId button clicked");
                CopySelectedAgentIdToInput();
            });

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

        //Debug.Log(
        //    $"WorkerInspectorPanel.Show -> worker='{worker.name}' " +
        //    $"agent={(worker.Agent != null ? worker.Agent.AgentId.ToString() : "none")}"
        //);

        Refresh();
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

    public void FocusSelectedAgent()
    {
        //Debug.Log("FocusSelectedAgent called");

        if (selectionManager == null)
        {
            //Debug.LogWarning("FocusSelectedAgent failed: selectionManager is null.");
            return;
        }

        if (selectedWorker == null || selectedWorker.Agent == null)
        {
            //Debug.LogWarning("FocusSelectedAgent failed: selectedWorker or selectedWorker.Agent is null.");
            return;
        }

        selectionManager.SelectByAgentId(selectedWorker.Agent.AgentId, true);
    }

    public void FocusTypedAgentId()
    {
        //Debug.Log("FocusTypedAgentId called");

        if (selectionManager == null)
        {
            //Debug.LogWarning("FocusTypedAgentId failed: selectionManager is null.");
            return;
        }

        if (agentIdInput == null)
        {
            //Debug.LogWarning("FocusTypedAgentId failed: agentIdInput is null.");
            return;
        }

        if (!int.TryParse(agentIdInput.text, out int agentId))
        {
            //Debug.LogWarning($"Invalid AgentId input: '{agentIdInput.text}'");
            return;
        }

        selectionManager.SelectByAgentId(agentId, true);
    }

    public void CopySelectedAgentIdToInput()
    {
        //Debug.Log("CopySelectedAgentIdToInput called");

        if (agentIdInput == null)
        {
            //Debug.LogWarning("CopySelectedAgentIdToInput failed: agentIdInput is null.");
            return;
        }

        if (selectedWorker == null || selectedWorker.Agent == null)
        {
            //Debug.LogWarning("CopySelectedAgentIdToInput failed: selected worker is null.");
            return;
        }

        agentIdInput.text = selectedWorker.Agent.AgentId.ToString();
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

        if (overviewText != null) overviewText.text = "";
        if (relationshipsText != null) relationshipsText.text = "";
        if (statsText != null) statsText.text = "";
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

    private string BuildOverview(AgentRecord agent, WorldState world)
    {
        string job = string.IsNullOrWhiteSpace(agent.BaseJob) ? "None" : agent.BaseJob;
        string intent = agent.CurrentIntent.ToString();
        string onShift = SimulationManager.Instance != null && SimulationManager.Instance.IsAgentOnShift(agent) ? "Yes" : "No";

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
}