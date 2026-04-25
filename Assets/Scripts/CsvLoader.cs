using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

public static class CsvLoader
{
    public class RoleDefinitionPair
    {
        public string BaseJob;
        public JobDefinition Primary;
        public JobDefinition Shadow;
    }

    private static readonly Dictionary<string, RoleDefinitionPair> roleDefinitionsByBaseJob =
        new Dictionary<string, RoleDefinitionPair>(StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyDictionary<string, RoleDefinitionPair> RoleDefinitionsByBaseJob => roleDefinitionsByBaseJob;

    public static List<JobDefinition> LoadJobs(string csv)
    {
        roleDefinitionsByBaseJob.Clear();

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

            job.ShiftStartMinutes.Clear();
            AddShiftIfValid(job.ShiftStartMinutes, job.Shift1);
            AddShiftIfValid(job.ShiftStartMinutes, job.Shift2);
            AddShiftIfValid(job.ShiftStartMinutes, job.Shift3);

            result.Add(job);
            RegisterJobDefinition(job);

            if (job.IsShadowRole)
            {
                string resolved = ResolveBaseJob(job.Job);
                //if (!roleDefinitionsByBaseJob.ContainsKey(resolved))
                //    UnityEngine.Debug.LogWarning($"Shadow job row '{job.Job}' could not be matched to a known base role. Resolved as '{resolved}'.");
            }
        }

        return result;
    }

    public static List<AgentRecord> LoadAgents(string csv)
    {
        var rows = Parse(csv);
        var result = new List<AgentRecord>();

        foreach (var row in rows)
        {
            string rawJob = Get(row, "Job");
            bool isShadow = IsShadowTitle(rawJob);
            string baseJob = ResolveBaseJob(rawJob);

            result.Add(new AgentRecord
            {
                AgentId = ToInt(Get(row, "Agent")),
                Job = rawJob,
                BaseJob = baseJob,
                IsShadowWorker = isShadow,
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

    public static RoleDefinitionPair GetRoleDefinitionPair(string baseJob)
    {
        if (string.IsNullOrWhiteSpace(baseJob))
            return null;

        roleDefinitionsByBaseJob.TryGetValue(baseJob.Trim(), out var pair);
        return pair;
    }

    public static JobDefinition GetPrimaryJobDefinition(string baseJob)
    {
        return GetRoleDefinitionPair(baseJob)?.Primary;
    }

    public static JobDefinition GetShadowJobDefinition(string baseJob)
    {
        return GetRoleDefinitionPair(baseJob)?.Shadow;
    }

    public static string ResolveBaseJob(string rawJob)
    {
        if (string.IsNullOrWhiteSpace(rawJob))
            return "";

        string trimmed = rawJob.Trim();

        if (!IsShadowTitle(trimmed))
            return trimmed;

        string withoutShadow = RemoveShadowSuffix(trimmed);

        if (roleDefinitionsByBaseJob.ContainsKey(withoutShadow))
            return withoutShadow;

        string matched = TryMatchKnownBaseJob(withoutShadow);
        if (!string.IsNullOrEmpty(matched))
            return matched;

        return withoutShadow;
    }

    public static bool IsShadowTitle(string title)
    {
        return !string.IsNullOrWhiteSpace(title) &&
               title.Trim().EndsWith("Shadow", StringComparison.OrdinalIgnoreCase);
    }

    private static void RegisterJobDefinition(JobDefinition job)
    {
        if (job == null || string.IsNullOrWhiteSpace(job.Job))
            return;

        bool isShadow = IsShadowTitle(job.Job);
        string baseJob = isShadow
            ? ResolveBaseJobFromKnownRows(job.Job)
            : job.Job.Trim();

        if (!roleDefinitionsByBaseJob.TryGetValue(baseJob, out var pair))
        {
            pair = new RoleDefinitionPair
            {
                BaseJob = baseJob
            };

            roleDefinitionsByBaseJob.Add(baseJob, pair);
        }

        if (isShadow)
            pair.Shadow = job;
        else
            pair.Primary = job;
    }

    private static string ResolveBaseJobFromKnownRows(string rawJob)
    {
        string withoutShadow = RemoveShadowSuffix(rawJob);

        if (roleDefinitionsByBaseJob.ContainsKey(withoutShadow))
            return withoutShadow;

        string matched = TryMatchKnownBaseJob(withoutShadow);
        if (!string.IsNullOrEmpty(matched))
            return matched;

        return withoutShadow;
    }

    //private static string SimpleSingularize(string value)
    //{
    //    if (string.IsNullOrWhiteSpace(value))
    //        return value;

    //    value = value.Trim();

    //    if (value.EndsWith("ies", StringComparison.OrdinalIgnoreCase) && value.Length > 3)
    //        return value.Substring(0, value.Length - 3) + "y";

    //    if (value.EndsWith("sses", StringComparison.OrdinalIgnoreCase) && value.Length > 4)
    //        return value.Substring(0, value.Length - 2);

    //    if (value.EndsWith("xes", StringComparison.OrdinalIgnoreCase) && value.Length > 3)
    //        return value.Substring(0, value.Length - 2);

    //    if (value.EndsWith("ses", StringComparison.OrdinalIgnoreCase) && value.Length > 3)
    //        return value.Substring(0, value.Length - 2);

    //    if (value.EndsWith("es", StringComparison.OrdinalIgnoreCase) && value.Length > 2)
    //        return value.Substring(0, value.Length - 2);

    //    if (value.EndsWith("s", StringComparison.OrdinalIgnoreCase) && value.Length > 1)
    //        return value.Substring(0, value.Length - 1);

    //    return value;
    //}

    private static List<Dictionary<string, string>> Parse(string csv)
    {
        var lines = csv.Replace("\r", "").Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
        if (lines.Length == 0)
            return new List<Dictionary<string, string>>();

        var headers = SplitLine(lines[0]);
        var rows = new List<Dictionary<string, string>>();

        for (int i = 1; i < lines.Length; i++)
        {
            var values = SplitLine(lines[i]);
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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
        => value.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase)
        || value.Trim().Equals("True", StringComparison.OrdinalIgnoreCase);

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

    private static string TryMatchKnownBaseJob(string shadowBaseCandidate)
    {
        if (string.IsNullOrWhiteSpace(shadowBaseCandidate))
            return null;

        foreach (var kvp in roleDefinitionsByBaseJob)
        {
            string knownBase = kvp.Key;

            if (string.Equals(knownBase, shadowBaseCandidate, StringComparison.OrdinalIgnoreCase))
                return knownBase;

            if (string.Equals(knownBase + "s", shadowBaseCandidate, StringComparison.OrdinalIgnoreCase))
                return knownBase;

            if (knownBase.EndsWith("y", StringComparison.OrdinalIgnoreCase))
            {
                string iesPlural = knownBase.Substring(0, knownBase.Length - 1) + "ies";
                if (string.Equals(iesPlural, shadowBaseCandidate, StringComparison.OrdinalIgnoreCase))
                    return knownBase;
            }
        }

        return null;
    }

    private static string RemoveShadowSuffix(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        value = value.Trim();

        const string suffix = "Shadow";
        if (value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            value = value.Substring(0, value.Length - suffix.Length).Trim();

        return value;
    }
}