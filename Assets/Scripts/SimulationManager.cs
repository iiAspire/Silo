using System.Collections;
using System.Collections.Generic;
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

    private IEnumerator Start()
    {
        LoadData();

        yield return null;

        if (NodeRegistry.Instance != null)
            NodeRegistry.Instance.Rebuild();

        AssignInitialNodes();
        BuildSystems();
        SpawnViews();
    }

    private void Update()
    {
        accumulator += Time.deltaTime;

        while (accumulator >= tickInterval)
        {
            accumulator -= tickInterval;
            World.Tick++;

            for (int i = 0; i < systems.Count; i++)
                systems[i].Tick(World);
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

        Debug.Log($"Walk nodes available: {spawnNodes.Count}");

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

        //Debug.Log($"Assigned initial nodes to {World.Agents.Count} agents.");
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
}