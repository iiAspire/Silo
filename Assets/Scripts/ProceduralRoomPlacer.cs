using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProceduralRoomPlacer : MonoBehaviour
{
    [System.Serializable]
    public class RoomDefinition
    {
        public string name;
        public GameObject prefab;
        public NodeType nodeType = NodeType.Work;
        public WorkplaceType workplaceType = WorkplaceType.None;

        [Range(0f, 1f)] public float topBandWeight = 0f;
        [Range(0f, 1f)] public float middleBandWeight = 0f;
        [Range(0f, 1f)] public float bottomBandWeight = 0f;

        public int minCount = 0;
        public int maxCount = 9999;

        public int minFloor = 0;
        public int maxFloor = 9999;

        public bool useGuarantee = false;
        public int everyNFloors = 0;

        public int skipGuaranteedOccurrence = -1;

        [Header("Placement Constraints")]
        public bool allowMultiplePerFloor = true;
        public int minFloorGapBetweenPlacements = 0;
    }

    [Header("References")]
    [SerializeField] private Transform floorsParent;
    [SerializeField] private NodeRegistry nodeRegistry;

    [Header("Room Definitions")]
    [SerializeField] private List<RoomDefinitionAsset> roomDefinitions;

    [Header("Generation")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool clearExistingRoomsFirst = true;
    [SerializeField] private int randomSeed = 12345;
    [SerializeField] private bool useRandomSeed = false;

    [Header("Floor Capacity")]
    [SerializeField] private int minRoomsPerFloor = 1;
    [SerializeField] private int maxRoomsPerFloor = 6;

    [Header("Band Ratios")]
    [SerializeField, Range(0f, 1f)] private float topResidentialBand = 0.60f;
    [SerializeField, Range(0f, 1f)] private float middleMixedBand = 0.25f;

    [Header("Naming")]
    [SerializeField] private string floorPrefix = "Floor_";
    [SerializeField] private string slotMarkerNameContains = "Slot";

    private Transform[] floors;
    private System.Random rng;

    private class SlotInfo
    {
        public Transform transform;
        public int slotNumber;
    }

    public class PlacedRoomInstance : MonoBehaviour
    {
        public string DefinitionName;
        public WorkplaceType WorkplaceType;
    }

    private void Start()
    {
        if (!generateOnStart)
            return;

        StartCoroutine(GenerateAfterDelay());
    }

    private IEnumerator GenerateAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);

        GenerateRooms();

        SimulationManager.Instance.StartSimulation();
    }

    [ContextMenu("Generate Rooms")]
    public void GenerateRooms()
    {
        RebuildFloorList();

        if (floors == null || floors.Length == 0)
        {
            Debug.LogError("ProceduralRoomPlacer: No floors found.");
            return;
        }

        if (roomDefinitions == null || roomDefinitions.Count == 0)
        {
            Debug.LogError("ProceduralRoomPlacer: No room definitions configured.");
            return;
        }

        rng = useRandomSeed ? new System.Random() : new System.Random(randomSeed);

        if (clearExistingRoomsFirst)
            ClearPreviouslyPlacedRooms();

        var placedCounts = new Dictionary<RoomDefinitionAsset, int>();
        var placedFloors = new Dictionary<RoomDefinitionAsset, List<int>>();

        foreach (var def in roomDefinitions)
        {
            placedCounts[def] = 0;
            placedFloors[def] = new List<int>();
        }

        EnsureMinimumCounts(placedCounts, placedFloors);

        List<int> floorOrder = Enumerable.Range(0, floors.Length).ToList();
        Shuffle(floorOrder);

        foreach (int floorIndex in floorOrder)
        {
            Transform floor = floors[floorIndex];
            List<SlotInfo> slots = GetAvailableSlots(floor);

            if (slots.Count == 0)
                continue;

            slots = slots.OrderBy(s => s.slotNumber).ToList();

            int roomsToPlace = Mathf.Clamp(
                rng.Next(minRoomsPerFloor, maxRoomsPerFloor + 1),
                1,
                slots.Count
            );

            int placedThisFloor = 0;

            foreach (var def in roomDefinitions)
            {
                if (!def.useGuarantee || def.everyNFloors <= 0)
                    continue;

                if (!ShouldPlaceGuaranteedRoom(def, floorIndex))
                    continue;

                if (!CanPlaceDefinitionOnFloor(def, floorIndex, placedCounts, placedFloors))
                    continue;

                if (slots.Count == 0 || placedThisFloor >= roomsToPlace)
                    break;

                SlotInfo slot = TakeRandomSlot(slots);
                if (PlaceRoom(def, slot.transform))
                {
                    placedCounts[def]++;
                    placedFloors[def].Add(floorIndex);
                    placedThisFloor++;
                }
            }

            int safety = 0;
            while (slots.Count > 0 && placedThisFloor < roomsToPlace && safety < 100)
            {
                safety++;

                RoomDefinitionAsset choice = PickWeightedDefinition(floorIndex, floors.Length, placedCounts, placedFloors);
                if (choice == null || choice.prefab == null)
                    continue;

                SlotInfo slot = TakeRandomSlot(slots);
                if (PlaceRoom(choice, slot.transform))
                {
                    placedCounts[choice]++;
                    placedFloors[choice].Add(floorIndex);
                    placedThisFloor++;
                }
            }
        }

        if (nodeRegistry == null)
            nodeRegistry = FindFirstObjectByType<NodeRegistry>();

        if (nodeRegistry != null)
            nodeRegistry.Rebuild();

        foreach (var kvp in placedCounts.OrderBy(k => k.Key.name))
            Debug.Log($"Placed count - {kvp.Key.name}: {kvp.Value}");
    }

    private void EnsureMinimumCounts(
        Dictionary<RoomDefinitionAsset, int> placedCounts,
        Dictionary<RoomDefinitionAsset, List<int>> placedFloors)
    {
        foreach (var def in roomDefinitions)
        {
            if (def == null || def.prefab == null)
                continue;

            if (def.minCount <= 0)
                continue;

            int safety = 0;
            while (placedCounts[def] < def.minCount && safety < 5000)
            {
                safety++;

                List<int> candidateFloors = Enumerable.Range(0, floors.Length)
                    .Where(floorIndex => CanPlaceDefinitionOnFloor(def, floorIndex, placedCounts, placedFloors))
                    .OrderBy(_ => rng.Next())
                    .ToList();

                bool placed = false;

                foreach (int floorIndex in candidateFloors)
                {
                    List<SlotInfo> slots = GetAvailableSlots(floors[floorIndex]);
                    if (slots.Count == 0)
                        continue;

                    SlotInfo slot = TakeRandomSlot(slots);
                    if (PlaceRoom(def, slot.transform))
                    {
                        placedCounts[def]++;
                        placedFloors[def].Add(floorIndex);
                        placed = true;
                        break;
                    }
                }

                if (!placed)
                    break;
            }
        }
    }

    public void RebuildFloorList()
    {
        if (floorsParent == null)
        {
            floors = new Transform[0];
            Debug.LogWarning("ProceduralRoomPlacer: floorsParent is not assigned.");
            return;
        }

        List<Transform> foundFloors = new List<Transform>();

        foreach (Transform child in floorsParent.GetComponentsInChildren<Transform>(true))
        {
            if (child != floorsParent && child.name.StartsWith(floorPrefix))
                foundFloors.Add(child);
        }

        floors = foundFloors.ToArray();
        System.Array.Sort(floors, (a, b) => b.position.y.CompareTo(a.position.y));
    }

    private List<SlotInfo> GetAvailableSlots(Transform floor)
    {
        List<SlotInfo> slots = new List<SlotInfo>();

        foreach (Transform child in floor.GetComponentsInChildren<Transform>(true))
        {
            if (child == floor)
                continue;

            if (!child.name.Contains(slotMarkerNameContains))
                continue;

            if (child.childCount > 0)
                continue;

            slots.Add(new SlotInfo
            {
                transform = child,
                slotNumber = ExtractSlotNumber(child.name)
            });
        }

        return slots;
    }

    private int ExtractSlotNumber(string name)
    {
        string digits = new string(name.Where(char.IsDigit).ToArray());
        if (int.TryParse(digits, out int number))
            return number;

        return int.MaxValue;
    }

    private RoomDefinitionAsset PickWeightedDefinition(
        int floorIndex,
        int totalFloors,
        Dictionary<RoomDefinitionAsset, int> placedCounts,
        Dictionary<RoomDefinitionAsset, List<int>> placedFloors)
    {
        List<RoomDefinitionAsset> candidates = new List<RoomDefinitionAsset>();

        foreach (var def in roomDefinitions)
        {
            if (def == null || def.prefab == null)
                continue;

            if (!CanPlaceDefinitionOnFloor(def, floorIndex, placedCounts, placedFloors))
                continue;

            int weight = GetWeightForFloor(def, floorIndex, totalFloors);
            if (weight > 0)
                candidates.Add(def);
        }

        if (candidates.Count == 0)
            return null;

        int totalWeight = 0;
        foreach (var def in candidates)
            totalWeight += GetWeightForFloor(def, floorIndex, totalFloors);

        if (totalWeight <= 0)
            return candidates[0];

        int roll = rng.Next(0, totalWeight);

        foreach (var def in candidates)
        {
            int weight = GetWeightForFloor(def, floorIndex, totalFloors);
            if (roll < weight)
                return def;

            roll -= weight;
        }

        return candidates[candidates.Count - 1];
    }

    private int GetWeightForFloor(RoomDefinitionAsset def, int floorIndex, int totalFloors)
    {
        float normalized = totalFloors <= 1 ? 0f : (float)floorIndex / (totalFloors - 1);

        float weight = normalized < topResidentialBand
            ? def.topBandWeight
            : normalized < topResidentialBand + middleMixedBand
                ? def.middleBandWeight
                : def.bottomBandWeight;

        return Mathf.Max(0, Mathf.CeilToInt(weight * 100f));
    }

    private bool IsDefinitionAllowedOnFloor(RoomDefinitionAsset def, int floorIndex, int totalFloors)
    {
        if (def == null)
            return false;

        if (floorIndex < def.minFloor || floorIndex > def.maxFloor)
            return false;

        return true;
    }

    private bool CanPlaceDefinitionOnFloor(
        RoomDefinitionAsset def,
        int floorIndex,
        Dictionary<RoomDefinitionAsset, int> placedCounts,
        Dictionary<RoomDefinitionAsset, List<int>> placedFloors)
    {
        if (def == null)
            return false;

        if (!IsDefinitionAllowedOnFloor(def, floorIndex, floors.Length))
            return false;

        if (placedCounts[def] >= def.maxCount)
            return false;

        if (!def.allowMultiplePerFloor && placedFloors[def].Contains(floorIndex))
            return false;

        if (def.minFloorGapBetweenPlacements > 0)
        {
            foreach (int priorFloor in placedFloors[def])
            {
                if (Mathf.Abs(priorFloor - floorIndex) < def.minFloorGapBetweenPlacements)
                    return false;
            }
        }

        return true;
    }

    private SlotInfo TakeRandomSlot(List<SlotInfo> slots)
    {
        int index = rng.Next(0, slots.Count);
        SlotInfo slot = slots[index];
        slots.RemoveAt(index);
        return slot;
    }

    private bool PlaceRoom(RoomDefinitionAsset def, Transform slot)
    {
        if (def == null || def.prefab == null || slot == null)
            return false;

        GameObject room = Instantiate(def.prefab, slot);
        room.name = def.prefab.name;

        room.transform.localPosition = Vector3.zero;
        room.transform.localRotation = Quaternion.identity;

        Renderer[] renderers = room.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            float worldLift = bounds.extents.y;
            room.transform.position += Vector3.up * worldLift;
        }

        var placedRoom = room.GetComponent<PlacedRoomInstance>();
        if (placedRoom == null)
            placedRoom = room.AddComponent<PlacedRoomInstance>();

        placedRoom.DefinitionName = def.name;
        placedRoom.WorkplaceType = def.workplaceType;

        return true;
    }

    private void ClearPreviouslyPlacedRooms()
    {
        if (floors == null)
            return;

        foreach (Transform floor in floors)
        {
            if (floor == null)
                continue;

            foreach (Transform child in floor.GetComponentsInChildren<Transform>(true))
            {
                if (child == floor)
                    continue;

                if (!child.name.Contains(slotMarkerNameContains))
                    continue;

                for (int i = child.childCount - 1; i >= 0; i--)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        DestroyImmediate(child.GetChild(i).gameObject);
                    else
                        Destroy(child.GetChild(i).gameObject);
#else
                    Destroy(child.GetChild(i).gameObject);
#endif
                }
            }
        }
    }

    private bool ShouldPlaceGuaranteedRoom(RoomDefinitionAsset def, int floorIndex)
    {
        if (def == null || !def.useGuarantee || def.everyNFloors <= 0)
            return false;

        int floorNumber = floorIndex + 1;

        if (floorNumber % def.everyNFloors != 0)
            return false;

        int occurrence = floorNumber / def.everyNFloors;

        if (def.skipGuaranteedOccurrence > 0 && occurrence == def.skipGuaranteedOccurrence)
            return false;

        return true;
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}