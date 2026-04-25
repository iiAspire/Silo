using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance { get; private set; }

    [SerializeField] private float tickInterval = 0.25f;
    [SerializeField] private TextAsset agentCsv;
    [SerializeField] private TextAsset jobCsv;
    [SerializeField] private Transform workerVisualParent;
    [SerializeField] private GameObject workerPrefab;
    [SerializeField] private int maxShadowsPerNodeTotal = 1;

    public WorldState World { get; private set; }

    private float accumulator;
    private readonly List<IWorldSystem> systems = new List<IWorldSystem>();
    private readonly Dictionary<int, WorkerView> viewsByAgentId = new Dictionary<int, WorkerView>();

    private void Awake()
    {
        Instance = this;
        World = new WorldState();
    }
    public void StartSimulation()
    {
        StartCoroutine(BootstrapSimulation());
    }

    private IEnumerator BootstrapSimulation()
    {
        LoadData();

        yield return null;

        while (NodeRegistry.Instance.work.Count == 0)
            yield return null;

        NodeRegistry.Instance.Rebuild();

        AssignInitialNodes();
        AssignHomesIfNeeded();
        AssignShifts();
        AssignWorkplaces();
        SpawnViews();
        BuildSystems();
    }

    private void Update()
    {
        if (World == null || World.Agents == null)
            return;

        accumulator += Time.deltaTime;

        while (accumulator >= tickInterval)
        {
            accumulator -= tickInterval;
            World.Tick++;

            AdvanceWorldClock(10);

            if (World.Tick % 20 == 0)
            {
                //Debug.Log($"Before systems Tick={World.Tick}: WorkNodes={World.Agents.Count(a => a.AssignedWorkNode != null)}/{World.Agents.Count}");
            }

            for (int i = 0; i < systems.Count; i++)
                systems[i].Tick(World);

            if (World.Tick % 20 == 0)
            {
                int withJob = World.Agents.Count(a => !string.IsNullOrWhiteSpace(a.Job));
                int withWorkNode = World.Agents.Count(a => a.AssignedWorkNode != null);
                int withIntent = World.Agents.Count(a => a.CurrentIntent != IntentType.None);
                int withTarget = World.Agents.Count(a => a.TargetNode != null);
                int withPath = World.Agents.Count(a => a.CurrentPath != null && a.CurrentPath.Count > 0);

                //Debug.Log(
                //    $"Tick={World.Tick} Time={World.MinuteOfDay} " +
                //    $"Jobs={withJob}/{World.Agents.Count} " +
                //    $"WorkNodes={withWorkNode}/{World.Agents.Count} " +
                //    $"Intent={withIntent}/{World.Agents.Count} " +
                //    $"Target={withTarget}/{World.Agents.Count} " +
                //    $"Path={withPath}/{World.Agents.Count}"
                //);
            }
        }

        SyncViews();
    }

    private void AdvanceWorldClock(int minutes)
    {
        World.MinuteOfDay += minutes;

        while (World.MinuteOfDay >= 1440)
        {
            World.MinuteOfDay -= 1440;
            World.Day++;
        }
    }

    private void LoadData()
    {
        if (jobCsv == null || agentCsv == null)
        {
            //Debug.LogError("SimulationManager is missing one or more CSV TextAssets.");
            return;
        }

        World.JobDefinitions = CsvLoader.LoadJobs(jobCsv.text);
        World.Agents = CsvLoader.LoadAgents(agentCsv.text);

        //Debug.Log($"Loaded {World.Agents.Count} agents and {World.JobDefinitions.Count} jobs.");
        //Debug.Log($"Built role dictionary with {CsvLoader.RoleDefinitionsByBaseJob.Count} base roles.");
    }

    private void AssignInitialNodes()
    {
        if (NodeRegistry.Instance == null)
        {
            //Debug.LogError("No NodeRegistry found in scene.");
            return;
        }

        var spawnNodes = NodeRegistry.Instance.walk;

        //Debug.Log($"Walk nodes available: {spawnNodes.Count}");

        if (spawnNodes == null || spawnNodes.Count == 0)
        {
            //Debug.LogError("No walk nodes available for spawning.");
            return;
        }

        for (int i = 0; i < World.Agents.Count; i++)
        {
            Node node = spawnNodes[i % spawnNodes.Count];
            World.Agents[i].CurrentNode = node;
        }

        //Debug.Log($"Assigned initial nodes to {World.Agents.Count} agents.");
    }

    private void AssignHomesIfNeeded()
    {
        var homeNodes = NodeRegistry.Instance.homes;
        if (homeNodes == null || homeNodes.Count == 0)
            return;

        for (int i = 0; i < World.Agents.Count; i++)
        {
            if (World.Agents[i].AssignedHomeNode == null)
                World.Agents[i].AssignedHomeNode = homeNodes[i % homeNodes.Count];
        }
    }

    private CsvLoader.RoleDefinitionPair GetRolePair(string baseJob)
    {
        return CsvLoader.GetRoleDefinitionPair(baseJob);
    }

    private JobDefinition GetPrimaryJobDefinition(string baseJob)
    {
        return GetRolePair(baseJob)?.Primary;
    }

    private JobDefinition GetShadowJobDefinition(string baseJob)
    {
        return GetRolePair(baseJob)?.Shadow;
    }

    private void ApplyShift(AgentRecord agent, JobDefinition job, int shiftIndex)
    {
        agent.AssignedShiftIndex = shiftIndex;
        agent.AssignedShiftLabel = job.GetShiftLabel(shiftIndex);
        agent.AssignedShiftStartMinute = job.ShiftStartMinutes[shiftIndex];
        agent.AssignedShiftLengthMinutes = job.ShiftLength * 60;

        //Debug.Log(
        //    $"ApplyShift Agent={agent.AgentId} Job='{agent.Job}' BaseJob='{agent.BaseJob}' " +
        //    $"JobDef='{job.Job}' ShiftIndex={shiftIndex} Start={agent.AssignedShiftStartMinute} " +
        //    $"Length={agent.AssignedShiftLengthMinutes} Shadow={agent.IsShadowWorker}"
        //);
    }

    private void BuildSystems()
    {
        systems.Clear();
        systems.Add(new NeedSystem());
        systems.Add(new WanderSystem());
        systems.Add(new TrafficSystem());
    }

    private void SpawnViews()
    {
        if (workerPrefab == null || workerVisualParent == null)
        {
            //Debug.LogWarning("SimulationManager missing worker prefab or visual parent.");
            return;
        }

        //Debug.Log($"Spawning {World.Agents.Count} worker visuals.");

        foreach (var agent in World.Agents)
        {
            GameObject go = Instantiate(workerPrefab, workerVisualParent);
            go.transform.position = agent.CurrentNode != null
                ? agent.CurrentNode.transform.position + Vector3.up * 0.9f
                : workerVisualParent.position + Vector3.up * 0.9f;
            WorkerView view = go.GetComponent<WorkerView>();

            if (view == null)
            {
                //Debug.LogError("Worker prefab is missing WorkerView.");
                return;
            }

            view.Bind(agent);
            viewsByAgentId[agent.AgentId] = view;
        }
    }

    private void SyncViews()
    {
        foreach (var agent in World.Agents)
        {
            if (viewsByAgentId.TryGetValue(agent.AgentId, out var view))
                view.Sync(agent);
        }
    }

    private void AssignWorkplaces()
    {
        if (NodeRegistry.Instance == null)
        {
            //Debug.LogError("No NodeRegistry found in scene.");
            return;
        }

        List<Node> allNodes = NodeRegistry.Instance.all;
        if (allNodes == null || allNodes.Count == 0)
        {
            //Debug.LogWarning("No nodes available for workplace assignment.");
            return;
        }

        foreach (var node in allNodes)
        {
            if (node != null && node.type == NodeType.Work)
                node.ClearAllAssignments();
        }

        foreach (var agent in World.Agents)
        {
            agent.AssignedWorkNode = null;
        }

        var workers = World.Agents
            .Where(a => !string.IsNullOrWhiteSpace(a.BaseJob))
            .ToList();

        var primaryWorkers = workers
            .Where(a => !a.IsShadowWorker)
            .OrderBy(a => a.BaseJob)
            .ThenBy(a => a.AssignedShiftIndex)
            .ThenBy(a => a.AgentId)
            .ToList();

        foreach (var agent in primaryWorkers)
        {
            var rolePair = GetRolePair(agent.BaseJob);
            JobDefinition primaryJob = rolePair?.Primary;

            if (primaryJob == null || primaryJob.ShiftCount == 0)
            {
                //Debug.LogWarning($"No primary job definition found for {agent.BaseJob}.");
                continue;
            }

            //Debug.Log(
            //    $"WorkplaceResolve Agent={agent.AgentId} Job='{agent.Job}' BaseJob='{agent.BaseJob}' " +
            //    $"PrimaryJobDef='{primaryJob?.Job}'"
            //);

            Node workNode = WorkplaceAssignmentUtility.FindPrimaryWorkNodeForAgent(agent, primaryJob, allNodes);

            if (workNode != null &&
                workNode.TryAssignPrimary(agent.AgentId, agent.AssignedShiftIndex, primaryJob.ShiftCount))
            {
                agent.AssignedWorkNode = workNode;
            }

            //Debug.Log(
            //    $"PRIMARY Agent {agent.AgentId} ({agent.Job} -> {agent.BaseJob}) shift={agent.AssignedShiftIndex} " +
            //    $"=> {(agent.AssignedWorkNode != null ? agent.AssignedWorkNode.name : "NO WORK NODE")}");
        }

        var shadowWorkers = workers
            .Where(a => a.IsShadowWorker)
            .OrderBy(a => a.BaseJob)
            .ThenBy(a => a.AgentId)
            .ToList();

        Dictionary<string, int> shadowCountByRole = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);

        foreach (var agent in shadowWorkers)
        {
            var rolePair = GetRolePair(agent.BaseJob);
            JobDefinition primaryJob = rolePair?.Primary;
            JobDefinition shadowJob = rolePair?.Shadow;

            if (primaryJob == null || primaryJob.ShiftCount == 0)
            {
                //Debug.LogWarning($"No primary job definition found for shadow worker role {agent.BaseJob}.");
                continue;
            }

            int maxShadowsForRole = shadowJob != null ? shadowJob.Max : 0;
            if (maxShadowsForRole <= 0)
            {
                //Debug.Log($"No shadow capacity defined for role {agent.BaseJob}; skipping shadow assignment.");
                continue;
            }

            if (!shadowCountByRole.ContainsKey(agent.BaseJob))
                shadowCountByRole[agent.BaseJob] = 0;

            int currentShadowsForRole = shadowCountByRole[agent.BaseJob];
            if (currentShadowsForRole >= maxShadowsForRole)
            {
                //Debug.Log($"Shadow cap reached for role {agent.BaseJob}: {currentShadowsForRole}/{maxShadowsForRole}");
                continue;
            }

            Node workNode = WorkplaceAssignmentUtility.FindShadowWorkNodeForAgent(
                agent,
                primaryJob,
                allNodes,
                maxShadowsForRole,
                currentShadowsForRole,
                maxShadowsPerNodeTotal);

            if (workNode != null &&
                workNode.TryAssignShadow(
                    agent.AgentId,
                    agent.AssignedShiftIndex,
                    primaryJob.ShiftCount,
                    maxShadowsPerNodeTotal))
            {
                agent.AssignedWorkNode = workNode;
                shadowCountByRole[agent.BaseJob]++;
            }

            //Debug.Log(
            //    $"SHADOW Agent {agent.AgentId} ({agent.Job} -> {agent.BaseJob}) shift={agent.AssignedShiftIndex} " +
            //    $"roleCap={maxShadowsForRole} assigned={shadowCountByRole[agent.BaseJob]} " +
            //    $"=> {(agent.AssignedWorkNode != null ? agent.AssignedWorkNode.name : "NO WORK NODE")}");
        }

        int withWorkNode = World.Agents.Count(a => a.AssignedWorkNode != null);
        //Debug.Log($"Assigned workplaces to {withWorkNode}/{World.Agents.Count} agents.");
    }

    private void AssignShifts()
    {
        var agentsByBaseJob = World.Agents
            .Where(a => !string.IsNullOrWhiteSpace(a.BaseJob))
            .GroupBy(a => a.BaseJob);

        foreach (var jobGroup in agentsByBaseJob)
        {
            var rolePair = GetRolePair(jobGroup.Key);
            JobDefinition primaryJob = rolePair?.Primary;

            if (primaryJob == null || primaryJob.ShiftCount == 0)
            {
                //Debug.LogWarning($"No primary job definition or shifts found for base role '{jobGroup.Key}'.");
                continue;
            }

            var primaryAgents = jobGroup
                .Where(a => !a.IsShadowWorker)
                .OrderBy(a => a.AgentId)
                .ToList();

            var shadowAgents = jobGroup
                .Where(a => a.IsShadowWorker)
                .OrderBy(a => a.AgentId)
                .ToList();

            for (int i = 0; i < primaryAgents.Count; i++)
            {
                int shiftIndex = i % primaryJob.ShiftCount;
                ApplyShift(primaryAgents[i], primaryJob, shiftIndex);
            }

            foreach (var shadow in shadowAgents)
            {
                int shiftIndex =
                    shadow.PreferredShiftIndex >= 0 &&
                    shadow.PreferredShiftIndex < primaryJob.ShiftCount
                        ? shadow.PreferredShiftIndex
                        : 0;

                ApplyShift(shadow, primaryJob, shiftIndex);
            }
        }

        int withShift = World.Agents.Count(a => a.AssignedShiftStartMinute >= 0);
        //Debug.Log($"Assigned shifts to {withShift}/{World.Agents.Count} agents.");
    }

    public bool IsAgentOnShift(AgentRecord agent)
    {
        if (agent == null)
            return false;

        return IsWithinShiftWindow(
            World.MinuteOfDay,
            agent.AssignedShiftStartMinute,
            agent.AssignedShiftLengthMinutes);
    }

    public static bool IsWithinShiftWindow(int minuteOfDay, int shiftStartMinute, int shiftLengthMinutes)
    {
        if (shiftStartMinute < 0 || shiftLengthMinutes <= 0)
            return false;

        if (shiftLengthMinutes >= 1440)
            return true;

        int start = shiftStartMinute % 1440;
        int end = (shiftStartMinute + shiftLengthMinutes) % 1440;

        if (start < end)
            return minuteOfDay >= start && minuteOfDay < end;

        return minuteOfDay >= start || minuteOfDay < end;
    }

    public WorkerView GetWorkerViewByAgentId(int agentId)
    {
        viewsByAgentId.TryGetValue(agentId, out var view);
        return view;
    }

    public AgentRecord GetAgentById(int agentId)
    {
        if (World == null || World.Agents == null)
            return null;

        for (int i = 0; i < World.Agents.Count; i++)
        {
            if (World.Agents[i].AgentId == agentId)
                return World.Agents[i];
        }

        return null;
    }
}