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
            result.Add(new JobDefinition
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
            });
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
}