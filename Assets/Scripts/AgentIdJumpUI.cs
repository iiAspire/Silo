using TMPro;
using UnityEngine;

public class AgentIdJumpUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField agentIdInput;
    [SerializeField] private AgentSelectionManager selectionManager;

    public void JumpToTypedAgentId()
    {
        if (agentIdInput == null || selectionManager == null)
            return;

        if (!int.TryParse(agentIdInput.text, out int agentId))
        {
            //Debug.LogWarning($"Invalid AgentId input: '{agentIdInput.text}'");
            return;
        }

        selectionManager.SelectByAgentId(agentId, true);
    }

    public void JumpToAgentId(int agentId)
    {
        if (selectionManager == null)
            return;

        selectionManager.SelectByAgentId(agentId, true);
    }
}