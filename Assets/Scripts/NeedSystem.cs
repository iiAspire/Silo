public class NeedSystem : IWorldSystem
{
    public void Tick(WorldState world)
    {
        foreach (var agent in world.Agents)
        {
            if (!agent.IsAlive) continue;

            agent.Hunger = System.Math.Max(0, agent.Hunger - 1);
            agent.Fatigue = System.Math.Max(0, agent.Fatigue - 1);

            if (agent.Hunger == 0 || agent.Fatigue == 0)
                agent.Health = System.Math.Max(0, agent.Health - 1);

            if (agent.Health == 0)
                agent.IsAlive = false;
        }
    }
}