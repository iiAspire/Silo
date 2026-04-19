using System.Collections.Generic;
using UnityEngine.LightTransport;

public class WorldState
{
    public int Tick;
    public List<JobDefinition> JobDefinitions = new List<JobDefinition>();
    public List<AgentRecord> Agents = new List<AgentRecord>();
    public List<TaskRecord> Tasks = new List<TaskRecord>();

    public int MinuteOfDay;
    public int Day;
}
