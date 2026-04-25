using System.Collections.Generic;

public class JobDefinition
{
    public string Job;
    public int Current;
    public int Max;
    public int MaxAge;
    public int Agility;
    public int Dexterity;
    public int Intellect;
    public int Repetition;
    public int Social;
    public int Fairness;
    public int Compassion;
    public int Power;

    public string Shift1;
    public string Shift2;
    public string Shift3;
    public int ShiftLength;
    public bool EarlyEnd;
    public bool Lunch;

    public List<int> ShiftStartMinutes = new List<int>();

    public bool IsShadowRole =>
        !string.IsNullOrWhiteSpace(Job) &&
        Job.Trim().EndsWith("Shadow", System.StringComparison.OrdinalIgnoreCase);

    public int ShiftCount => ShiftStartMinutes != null ? ShiftStartMinutes.Count : 0;

    public string GetShiftLabel(int shiftIndex)
    {
        return shiftIndex switch
        {
            0 => string.IsNullOrWhiteSpace(Shift1) ? "Shift 1" : Shift1,
            1 => string.IsNullOrWhiteSpace(Shift2) ? "Shift 2" : Shift2,
            2 => string.IsNullOrWhiteSpace(Shift3) ? "Shift 3" : Shift3,
            _ => $"Shift {shiftIndex + 1}"
        };
    }
}