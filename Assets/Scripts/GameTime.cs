using UnityEngine;

public class GameTime : MonoBehaviour
{
    public static GameTime Instance;

    public float timeOfDay = 6f; // start at 06:00
    public float dayLength = 120f; // seconds per full day

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        timeOfDay += Time.deltaTime * (24f / dayLength);

        if (timeOfDay >= 24f)
            timeOfDay -= 24f;
    }

    public float CurrentHour()
    {
        return timeOfDay;
    }
}