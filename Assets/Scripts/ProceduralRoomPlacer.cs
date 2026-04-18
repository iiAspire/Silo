using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProceduralRoomPlacer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform floorsParent;
    [SerializeField] private NodeRegistry nodeRegistry;

    [Header("Room Prefabs")]
    [SerializeField] private GameObject apartmentPrefab;
    [SerializeField] private GameObject canteenPrefab;
    [SerializeField] private GameObject generatorPrefab;
    [SerializeField] private GameObject schoolPrefab;
    [SerializeField] private GameObject marketPrefab;
    [SerializeField] private GameObject sheriffStationPrefab;
    [SerializeField] private GameObject mayorOfficePrefab;
    [SerializeField] private GameObject porterHubPrefab;
    [SerializeField] private GameObject itPrefab;
    [SerializeField] private GameObject securityPrefab;
    [SerializeField] private GameObject judgesChambersPrefab;
    [SerializeField] private GameObject clinicPrefab;
    [SerializeField] private GameObject hospitalPrefab;
    [SerializeField] private GameObject farmPrefab;
    [SerializeField] private GameObject butcherPrefab;
    [SerializeField] private GameObject bakerPrefab;
    [SerializeField] private GameObject manufacturingPrefab;
    [SerializeField] private GameObject processingPrefab;
    [SerializeField] private GameObject supplyPrefab;
    [SerializeField] private GameObject carpenterPrefab;
    [SerializeField] private GameObject builderPrefab;
    [SerializeField] private GameObject cleanerPrefab;
    [SerializeField] private GameObject prisonerPrefab;
    [SerializeField] private GameObject recreationPrefab;

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

    [Header("Top Band Weights")]
    [SerializeField] private int topApartmentWeight = 80;
    [SerializeField] private int topCanteenWeight = 15;
    [SerializeField] private int topGeneratorWeight = 5;

    [Header("Middle Band Weights")]
    [SerializeField] private int midApartmentWeight = 55;
    [SerializeField] private int midCanteenWeight = 30;
    [SerializeField] private int midGeneratorWeight = 15;

    [Header("Bottom Band Weights")]
    [SerializeField] private int bottomApartmentWeight = 20;
    [SerializeField] private int bottomCanteenWeight = 20;
    [SerializeField] private int bottomGeneratorWeight = 60;

    [Header("Service Guarantees")]
    [SerializeField] private int canteenEveryNFloors = 12;
    [SerializeField] private int generatorEveryNFloors = 144;

    [Header("Apartment Gating")]
    [SerializeField] private int apartmentGateSlotA = 1;
    [SerializeField] private int apartmentGateSlotB = 2;
    [SerializeField] private float apartmentGateWeightMultiplier = 1.0f;
    [SerializeField] private float apartmentClusterWeightMultiplier = 2.5f;

    [Header("Naming")]
    [SerializeField] private string floorPrefix = "Floor_";
    [SerializeField] private string slotMarkerNameContains = "Slot";

    private Transform[] floors;
    private System.Random rng;

    private enum RoomType
    {
        Apartment,
        Canteen,
        Generator
    }

    private class SlotInfo
    {
        public Transform transform;
        public int slotNumber;
    }

    private void Start()
    {
        if (!generateOnStart)
            return;

        StartCoroutine(GenerateAfterDelay());
    }

    private IEnumerator GenerateAfterDelay()
    {
        yield return null;
        GenerateRooms();
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

        if (apartmentPrefab == null || canteenPrefab == null || generatorPrefab == null)
        {
            Debug.LogError("ProceduralRoomPlacer: One or more room prefabs are missing.");
            return;
        }

        rng = useRandomSeed ? new System.Random() : new System.Random(randomSeed);

        if (clearExistingRoomsFirst)
            ClearPreviouslyPlacedRooms();

        int totalPlaced = 0;
        int apartmentsPlaced = 0;
        int canteensPlaced = 0;
        int generatorsPlaced = 0;

        for (int floorIndex = 0; floorIndex < floors.Length; floorIndex++)
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

            bool forceCanteen = canteenEveryNFloors > 0 && floorIndex % canteenEveryNFloors == 0;
            bool forceGenerator = generatorEveryNFloors > 0 && floorIndex % generatorEveryNFloors == 0;

            int placedThisFloor = 0;

            if (forceCanteen && slots.Count > 0)
            {
                SlotInfo slot = TakeRandomSlot(slots);
                if (PlaceRoom(canteenPrefab, slot.transform))
                {
                    totalPlaced++;
                    canteensPlaced++;
                    placedThisFloor++;
                }
            }

            if (forceGenerator && slots.Count > 0 && placedThisFloor < roomsToPlace)
            {
                SlotInfo slot = TakeRandomSlot(slots);
                if (PlaceRoom(generatorPrefab, slot.transform))
                {
                    totalPlaced++;
                    generatorsPlaced++;
                    placedThisFloor++;
                }
            }

            bool apartmentEnabledOnFloor = false;

            if (placedThisFloor < roomsToPlace)
            {
                apartmentEnabledOnFloor = TryApartmentGatePlacement(floorIndex, slots, ref totalPlaced, ref apartmentsPlaced, ref placedThisFloor);
            }

            while (slots.Count > 0 && placedThisFloor < roomsToPlace)
            {
                RoomType type = PickWeightedRoomTypeForFloor(
                    floorIndex,
                    floors.Length,
                    apartmentEnabledOnFloor,
                    isGatePhase: false
                );

                GameObject prefab = GetPrefab(type);
                if (prefab == null)
                    break;

                SlotInfo slot = TakeRandomSlot(slots);
                if (PlaceRoom(prefab, slot.transform))
                {
                    totalPlaced++;
                    placedThisFloor++;

                    switch (type)
                    {
                        case RoomType.Apartment: apartmentsPlaced++; break;
                        case RoomType.Canteen: canteensPlaced++; break;
                        case RoomType.Generator: generatorsPlaced++; break;
                    }
                }
            }
        }

        if (nodeRegistry == null)
            nodeRegistry = FindFirstObjectByType<NodeRegistry>();

        if (nodeRegistry != null)
            nodeRegistry.Rebuild();

        Debug.Log(
            $"ProceduralRoomPlacer complete. Total rooms: {totalPlaced}, " +
            $"Apartments: {apartmentsPlaced}, Canteens: {canteensPlaced}, Generators: {generatorsPlaced}"
        );
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

        Debug.Log($"ProceduralRoomPlacer: found {floors.Length} floors.");
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

    private bool TryApartmentGatePlacement(
        int floorIndex,
        List<SlotInfo> slots,
        ref int totalPlaced,
        ref int apartmentsPlaced,
        ref int placedThisFloor)
    {
        List<SlotInfo> gateSlots = slots
            .Where(s => s.slotNumber == apartmentGateSlotA || s.slotNumber == apartmentGateSlotB)
            .OrderBy(s => s.slotNumber)
            .ToList();

        if (gateSlots.Count == 0)
            return false;

        foreach (SlotInfo gateSlot in gateSlots)
        {
            int apartmentWeight, canteenWeight, generatorWeight;
            GetBandWeights(floorIndex, floors.Length, out apartmentWeight, out canteenWeight, out generatorWeight);

            apartmentWeight = Mathf.RoundToInt(apartmentWeight * apartmentGateWeightMultiplier);

            RoomType choice = PickWeightedRoomType(apartmentWeight, canteenWeight, generatorWeight);

            slots.Remove(gateSlot);

            if (choice == RoomType.Apartment)
            {
                if (PlaceRoom(apartmentPrefab, gateSlot.transform))
                {
                    totalPlaced++;
                    apartmentsPlaced++;
                    placedThisFloor++;
                    return true;
                }
            }
            else
            {
                GameObject prefab = GetPrefab(choice);
                if (prefab != null && PlaceRoom(prefab, gateSlot.transform))
                {
                    totalPlaced++;
                    placedThisFloor++;
                }
            }
        }

        return false;
    }

    private SlotInfo TakeRandomSlot(List<SlotInfo> slots)
    {
        int index = rng.Next(0, slots.Count);
        SlotInfo slot = slots[index];
        slots.RemoveAt(index);
        return slot;
    }

    private bool PlaceRoom(GameObject prefab, Transform slot)
    {
        if (prefab == null || slot == null)
            return false;

        GameObject room = Instantiate(prefab, slot);
        room.name = prefab.name;

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

        return true;
    }

    private RoomType PickWeightedRoomTypeForFloor(
        int floorIndex,
        int totalFloors,
        bool apartmentEnabledOnFloor,
        bool isGatePhase)
    {
        int apartmentWeight, canteenWeight, generatorWeight;
        GetBandWeights(floorIndex, totalFloors, out apartmentWeight, out canteenWeight, out generatorWeight);

        if (isGatePhase)
        {
            apartmentWeight = Mathf.RoundToInt(apartmentWeight * apartmentGateWeightMultiplier);
        }
        else
        {
            if (!apartmentEnabledOnFloor)
            {
                apartmentWeight = 0;
            }
            else
            {
                apartmentWeight = Mathf.RoundToInt(apartmentWeight * apartmentClusterWeightMultiplier);
            }
        }

        return PickWeightedRoomType(apartmentWeight, canteenWeight, generatorWeight);
    }

    private void GetBandWeights(int floorIndex, int totalFloors, out int apartmentWeight, out int canteenWeight, out int generatorWeight)
    {
        float normalized = totalFloors <= 1 ? 0f : (float)floorIndex / (totalFloors - 1);

        if (normalized < topResidentialBand)
        {
            apartmentWeight = topApartmentWeight;
            canteenWeight = topCanteenWeight;
            generatorWeight = topGeneratorWeight;
        }
        else if (normalized < topResidentialBand + middleMixedBand)
        {
            apartmentWeight = midApartmentWeight;
            canteenWeight = midCanteenWeight;
            generatorWeight = midGeneratorWeight;
        }
        else
        {
            apartmentWeight = bottomApartmentWeight;
            canteenWeight = bottomCanteenWeight;
            generatorWeight = bottomGeneratorWeight;
        }
    }

    private RoomType PickWeightedRoomType(int apartmentWeight, int canteenWeight, int generatorWeight)
    {
        int totalWeight = apartmentWeight + canteenWeight + generatorWeight;
        if (totalWeight <= 0)
            return RoomType.Generator;

        int roll = rng.Next(0, totalWeight);

        if (roll < apartmentWeight)
            return RoomType.Apartment;

        roll -= apartmentWeight;
        if (roll < canteenWeight)
            return RoomType.Canteen;

        return RoomType.Generator;
    }

    private GameObject GetPrefab(RoomType type)
    {
        switch (type)
        {
            case RoomType.Apartment: return apartmentPrefab;
            case RoomType.Canteen: return canteenPrefab;
            case RoomType.Generator: return generatorPrefab;
            default: return null;
        }
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

                if ((apartmentPrefab != null && child.name == apartmentPrefab.name) ||
                    (canteenPrefab != null && child.name == canteenPrefab.name) ||
                    (generatorPrefab != null && child.name == generatorPrefab.name))
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        DestroyImmediate(child.gameObject);
                    else
                        Destroy(child.gameObject);
#else
                    Destroy(child.gameObject);
#endif
                }
            }
        }
    }
}