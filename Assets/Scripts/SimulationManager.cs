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
        StartCoroutine(StartCoroutine());
    }


    private IEnumerator StartCoroutine()
    {
        LoadData();

        yield return null; // wait 1 frame

        // Wait for rooms to be generated
        while (NodeRegistry.Instance.work.Count == 0)
            yield return null;

        NodeRegistry.Instance.Rebuild(); // refresh the registry

        AssignInitialNodes();
        SpawnViews();
        AssignWorkplaces();
        AssignShifts();
        BuildSystems();
    }

    private void Update()
    {
        accumulator += Time.deltaTime;

        if (World.Tick == 0)
        {
            var a = World.Agents[0];
            var node = a.AssignedWorkNode;

            Debug.Log(
                $"First update work == null? {node == null}, " +
                $"ReferenceEquals null? {ReferenceEquals(node, null)}, " +
                $"node raw name: {(ReferenceEquals(node, null) ? "real-null" : node.name)}"
            );
        }

        while (accumulator >= tickInterval)
        {
            accumulator -= tickInterval;
            World.Tick++;

            AdvanceWorldClock(10);  // example: 10 in-game minutes per sim tick

            if (World.Tick % 20 == 0)
            {
                Debug.Log($"Before systems Tick={World.Tick}: WorkNodes={World.Agents.Count(a => a.AssignedWorkNode != null)}/{World.Agents.Count}");
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

                Debug.Log(
                    $"Tick={World.Tick} Time={World.MinuteOfDay} " +
                    $"Jobs={withJob}/{World.Agents.Count} " +
                    $"WorkNodes={withWorkNode}/{World.Agents.Count} " +
                    $"Intent={withIntent}/{World.Agents.Count} " +
                    $"Target={withTarget}/{World.Agents.Count} " +
                    $"Path={withPath}/{World.Agents.Count}"
                );
            }
        }

        SyncViews();
    }

    private void LoadData()
    {
        if (jobCsv == null || agentCsv == null)
        {
            Debug.LogError("SimulationManager is missing one or more CSV TextAssets.");
            return;
        }

        World.JobDefinitions = CsvLoader.LoadJobs(jobCsv.text);
        World.Agents = CsvLoader.LoadAgents(agentCsv.text);

        Debug.Log($"Loaded {World.Agents.Count} agents and {World.JobDefinitions.Count} jobs.");
    }

    private void AssignInitialNodes()
    {
        if (NodeRegistry.Instance == null)
        {
            Debug.LogError("No NodeRegistry found in scene.");
            return;
        }

        var spawnNodes = NodeRegistry.Instance.walk;

        //Debug.Log($"Walk nodes available: {spawnNodes.Count}");

        if (spawnNodes == null || spawnNodes.Count == 0)
        {
            Debug.LogError("No walk nodes available for spawning.");
            return;
        }

        for (int i = 0; i < World.Agents.Count; i++)
        {
            Node node = spawnNodes[i % spawnNodes.Count];
            World.Agents[i].CurrentNode = node;
        }

        Debug.Log($"Assigned initial nodes to {World.Agents.Count} agents.");
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
            Debug.LogWarning("SimulationManager missing worker prefab or visual parent.");
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
                Debug.LogError("Worker prefab is missing WorkerView.");
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
            Debug.LogError("No NodeRegistry found in scene.");
            return;
        }

        List<Node> allNodes = NodeRegistry.Instance.all;
        if (allNodes == null || allNodes.Count == 0)
        {
            Debug.LogWarning("No nodes available for workplace assignment.");
            return;
        }

        foreach (var agent in World.Agents)
        {
            if (string.IsNullOrWhiteSpace(agent.Job))
                continue;

            if (agent.AssignedWorkNode != null)
                agent.AssignedWorkNode.UnassignWorker(agent.AgentId);

            Node workNode = WorkplaceAssignmentUtility.FindBestWorkNodeForJob(agent.Job, allNodes);

            agent.AssignedWorkNode = workNode;

            if (workNode != null)
                workNode.AssignWorker(agent.AgentId);

             Debug.Log($"Agent {agent.AgentId} ({agent.Job}) assigned to {(workNode != null ? workNode.name : "NO WORK NODE")}");
        }

        int withWorkNode = World.Agents.Count(a => a.AssignedWorkNode != null);
        Debug.Log($"Assigned workplaces to {withWorkNode}/{World.Agents.Count} agents.");
    }

    private void AssignShifts()
    {
        foreach (var agent in World.Agents)
        {
            JobDefinition job = World.JobDefinitions.Find(j => j.Job == agent.Job);
            if (job == null || job.ShiftStartMinutes.Count == 0)
                continue;

            int sameJobIndex = World.Agents
                .Where(a => a.Job == agent.Job && a.AgentId <= agent.AgentId)
                .Count() - 1;

            int shiftIndex = sameJobIndex % job.ShiftStartMinutes.Count;

            agent.AssignedShiftIndex = shiftIndex;
            agent.AssignedShiftStartMinute = job.ShiftStartMinutes[shiftIndex];
            agent.AssignedShiftLengthMinutes = job.ShiftLength;
        }

        int withShift = World.Agents.Count(a => a.AssignedShiftStartMinute >= 0);
        Debug.Log($"Assigned shifts to {withShift}/{World.Agents.Count} agents.");
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
}