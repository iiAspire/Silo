using System.Collections.Generic;

public class AgentRecord
{
    public int AgentId;
    public string Job;
    public int Age;
    public int Health;
    public int Hunger;
    public int Fatigue;
    public int Happiness;
    public int Agility;
    public int Dexterity;
    public int Intellect;
    public int Repetition;
    public int Social;
    public int Fairness;
    public int Compassion;
    public int Power;

    public Node CurrentNode;
    public Node TargetNode;
    public bool IsAlive = true;

    public List<Node> CurrentPath = new List<Node>();
    public int PathIndex = 0;
    public float WaitTimer = 0f;
}