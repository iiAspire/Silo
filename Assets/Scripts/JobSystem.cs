using System.Linq;

public class JobSystem : IWorldSystem
{
    public void Tick(WorldState world)
    {
        world.Tasks.Clear();

        foreach (var agent in world.Agents.Where(a => a.IsAlive))
        {
            string taskType = PickTask(agent, world);
            if (taskType == null)
                continue;

            world.Tasks.Add(new TaskRecord
            {
                AgentId = agent.AgentId,
                TaskType = taskType,
                Reserved = false,
                Completed = false
            });
        }
    }

    private string PickTask(AgentRecord agent, WorldState world)
    {
        if (agent.Hunger < 25) return "Eat";
        if (agent.Fatigue < 25) return "Sleep";
        return agent.Job;
    }
}