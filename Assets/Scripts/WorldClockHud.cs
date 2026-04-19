using TMPro;
using UnityEngine;

public class WorldClockHud : MonoBehaviour
{
    [SerializeField] private TMP_Text clockText;

    private void Update()
    {
        if (clockText == null || SimulationManager.Instance == null || SimulationManager.Instance.World == null)
            return;

        var world = SimulationManager.Instance.World;
        int hour = world.MinuteOfDay / 60;
        int minute = world.MinuteOfDay % 60;

        clockText.text = $"Day {world.Day + 1}  {hour:00}:{minute:00}";
    }
}