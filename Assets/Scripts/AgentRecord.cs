using System.Collections.Generic;

public enum IntentType
{
    None,
    Work,
    Sleep,
    Eat,
    Wander,
    Shop,
    SendPackage,
    VisitPrisoner,
    FileReport,
    AttendAppointment,
    SocialVisit
}

public enum RelationshipType
{
    Family,
    Partner,
    Friend,
    Coworker,
    Neighbor
}

public class AgentRecord
{
    public int AgentId;
    public string Name;
    public string Sex;
    public int HouseholdID;
    public string HouseholdRole;
    public string Job;
    public string BaseJob;
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

    private Node assignedWorkNode;
    public Node AssignedWorkNode
    {
        get => assignedWorkNode;
        set => assignedWorkNode = value;
    }

    public Node AssignedHomeNode;

    public bool IsShadowWorker = false;
    public int PreferredShiftIndex = -1;

    public int AssignedShiftIndex = -1;
    public string AssignedShiftLabel;
    public int AssignedShiftStartMinute = -1;
    public int AssignedShiftLengthMinutes = 0;

    public IntentType CurrentIntent;
    public ErrandRecord ActiveErrand;
    public List<ErrandRecord> PendingErrands = new();
    public List<RelationshipLink> Relationships = new();
    public bool IsAlive = true;

    public List<Node> CurrentPath = new List<Node>();
    public int PathIndex = 0;
    public float WaitTimer = 0f;
}

public class ErrandRecord
{
    public IntentType Intent;
    public NodeType TargetNodeType;
    public Node TargetNode;
    public int Priority;
    public float EarliestStartTick;
    public float ExpireTick;
    public bool IsMandatory;
    public string CurrentErrand;
    public string TargetAgentID;
}

public class RelationshipLink
{
    public int OtherAgentId;
    public RelationshipType Type;
    public float Strength;
    public bool HouseholdTie;
    public int PartnerAgentID;
    public string ChildrenCount;
    public int RelationshipCount;
    public int ClosestRelationshipSummary;
}