using UnityEngine;

public class GameTime : MonoBehaviour
{
    public static GameTime Instance { get; private set; }

    [SerializeField] private float startHour = 6f;
    [SerializeField] private float dayLengthSeconds = 120f;
    [SerializeField] private bool paused;

    public float TimeOfDay { get; private set; }
    public float DeltaHoursThisFrame { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        TimeOfDay = startHour;
    }

    private void Update()
    {
        if (paused)
        {
            DeltaHoursThisFrame = 0f;
            return;
        }

        DeltaHoursThisFrame = Time.deltaTime * (24f / Mathf.Max(1f, dayLengthSeconds));
        TimeOfDay += DeltaHoursThisFrame;

        while (TimeOfDay >= 24f)
            TimeOfDay -= 24f;
    }

    public float CurrentHour() => TimeOfDay;
    public void SetPaused(bool value) => paused = value;
}