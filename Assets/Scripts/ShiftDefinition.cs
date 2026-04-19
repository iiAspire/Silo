using System.Collections.Generic;

[System.Serializable]
public class ShiftDefinition
{
    public string ShiftId;
    public int StartMinute;     // 0-1439
    public int DurationMinutes; // e.g. 480
    public int RequiredWorkers; // optional for staffing checks

    public int EndMinute => (StartMinute + DurationMinutes) % 1440;
}

[System.Serializable]
public class JobScheduleDefinition
{
    public string JobId;
    public List<ShiftDefinition> Shifts = new();
}