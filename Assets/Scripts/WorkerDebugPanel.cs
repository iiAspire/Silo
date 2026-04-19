using TMPro;
using UnityEngine;

public class WorkerDebugPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text infoText;

    public void Show(WorkerView worker)
    {
        if (worker == null || infoText == null)
            return;

        AgentRecord agent = worker.Agent;
        if (agent == null)
        {
            infoText.text = "No agent bound.";
            return;
        }

        int now = SimulationManager.Instance.World.MinuteOfDay;
        bool onShift = IsOnShift(agent, now);

        string currentNode = agent.CurrentNode != null ? agent.CurrentNode.name : "None";
        string targetNode = agent.TargetNode != null ? agent.TargetNode.name : "None";
        string workNode = agent.AssignedWorkNode != null ? GetNodeDisplayName(agent.AssignedWorkNode) : "None";
        string homeNode = agent.AssignedHomeNode != null ? agent.AssignedHomeNode.name : "None";

        infoText.text =
            $"Agent: {agent.AgentId}\n" +
            $"Job: {agent.Job}\n" +
            $"Intent: {agent.CurrentIntent}\n" +
            $"On Shift: {onShift}\n" +
            $"Shift Start: {FormatMinutes(agent.AssignedShiftStartMinute)}\n" +
            $"Shift Length: {agent.AssignedShiftLengthMinutes}m\n" +
            $"Current Node: {currentNode}\n" +
            $"Target Node: {targetNode}\n" +
            $"Work Node: {workNode}\n" +
            $"Home Node: {homeNode}\n" +
            $"Path Index: {agent.PathIndex}\n" +
            $"Path Count: {(agent.CurrentPath != null ? agent.CurrentPath.Count : 0)}\n" +
            $"Wait Timer: {agent.WaitTimer:F1}";
    }

    private bool IsOnShift(AgentRecord agent, int minuteOfDay)
    {
        if (agent.AssignedShiftStartMinute < 0 || agent.AssignedShiftLengthMinutes <= 0)
            return false;

        int start = agent.AssignedShiftStartMinute;
        int end = (start + agent.AssignedShiftLengthMinutes) % 1440;

        if (start < end)
            return minuteOfDay >= start && minuteOfDay < end;

        return minuteOfDay >= start || minuteOfDay < end;
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

    private string FormatMinutes(int totalMinutes)
    {
        if (totalMinutes < 0)
            return "None";

        int hour = totalMinutes / 60;
        int minute = totalMinutes % 60;
        return $"{hour:00}:{minute:00}";
    }
}