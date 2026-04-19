using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

public static class CsvLoader
{
    public static List<JobDefinition> LoadJobs(string csv)
    {
        var rows = Parse(csv);
        var result = new List<JobDefinition>();

        foreach (var row in rows)
        {
            var job = new JobDefinition
            {
                Job = Get(row, "Jobs"),
                Current = ToInt(Get(row, "Current")),
                Max = ToInt(Get(row, "Max")),
                MaxAge = ToInt(Get(row, "MaxAge")),
                Agility = ToInt(Get(row, "Agility")),
                Dexterity = ToInt(Get(row, "Dexterity")),
                Intellect = ToInt(Get(row, "Intellect")),
                Repetition = ToInt(Get(row, "Repetition")),
                Social = ToInt(Get(row, "Social")),
                Fairness = ToInt(Get(row, "Fairness")),
                Compassion = ToInt(Get(row, "Compassion")),
                Power = ToInt(Get(row, "Power")),
                Shift1 = Get(row, "Shift1"),
                Shift2 = Get(row, "Shift2"),
                Shift3 = Get(row, "Shift3"),
                ShiftLength = ToInt(Get(row, "ShiftLength")),
                EarlyEnd = ToBool(Get(row, "EarlyEnd")),
                Lunch = ToBool(Get(row, "Lunch"))
            };

            AddShiftIfValid(job.ShiftStartMinutes, job.Shift1);
            AddShiftIfValid(job.ShiftStartMinutes, job.Shift2);
            AddShiftIfValid(job.ShiftStartMinutes, job.Shift3);

            result.Add(job);
        }

        return result;
    }

    public static List<AgentRecord> LoadAgents(string csv)
    {
        var rows = Parse(csv);
        var result = new List<AgentRecord>();

        foreach (var row in rows)
        {
            result.Add(new AgentRecord
            {
                AgentId = ToInt(Get(row, "Agent")),
                Job = Get(row, "Job"),
                Age = ToInt(Get(row, "Age")),
                Health = ToInt(Get(row, "Health")),
                Hunger = ToInt(Get(row, "Hunger")),
                Fatigue = ToInt(Get(row, "Fatigue")),
                Happiness = ToInt(Get(row, "Happiness")),
                Agility = ToInt(Get(row, "Agility")),
                Dexterity = ToInt(Get(row, "Dexterity")),
                Intellect = ToInt(Get(row, "Intellect")),
                Repetition = ToInt(Get(row, "Repetition")),
                Social = ToInt(Get(row, "Social")),
                Fairness = ToInt(Get(row, "Fairness")),
                Compassion = ToInt(Get(row, "Compassion")),
                Power = ToInt(Get(row, "Power"))
            });
        }

        return result;
    }

    private static List<Dictionary<string, string>> Parse(string csv)
    {
        var lines = csv.Replace("\r", "").Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
        if (lines.Length == 0) return new List<Dictionary<string, string>>();

        var headers = SplitLine(lines[0]);
        var rows = new List<Dictionary<string, string>>();

        for (int i = 1; i < lines.Length; i++)
        {
            var values = SplitLine(lines[i]);
            var row = new Dictionary<string, string>();

            for (int c = 0; c < headers.Count; c++)
                row[headers[c]] = c < values.Count ? values[c] : "";

            rows.Add(row);
        }

        return rows;
    }

    private static List<string> SplitLine(string line)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char ch = line[i];

            if (ch == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        values.Add(current.ToString());
        return values;
    }

    private static string Get(Dictionary<string, string> row, string key)
        => row.TryGetValue(key, out var value) ? value.Trim() : "";

    private static int ToInt(string value)
        => int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0;

    private static bool ToBool(string value)
        => value.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) || value.Trim().Equals("True", StringComparison.OrdinalIgnoreCase);

    private static void AddShiftIfValid(List<int> list, string value)
    {
        int minutes = ParseTimeToMinutes(value);
        if (minutes >= 0)
            list.Add(minutes);
    }

    private static int ParseTimeToMinutes(string value)
    {
        value = value.Trim();
        if (string.IsNullOrEmpty(value))
            return -1;

        string[] formats = { @"h\:mm", @"hh\:mm", @"h\:mm\:ss", @"hh\:mm\:ss" };

        foreach (string format in formats)
        {
            if (TimeSpan.TryParseExact(value, format, CultureInfo.InvariantCulture, out var ts))
                return (int)ts.TotalMinutes;
        }

        if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var fallback))
            return (int)fallback.TotalMinutes;

        return -1;
    }
}