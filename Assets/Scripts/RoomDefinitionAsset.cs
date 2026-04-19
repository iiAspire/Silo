using UnityEngine;

[CreateAssetMenu(menuName = "Rooms/Room Definition")]
public class RoomDefinitionAsset : ScriptableObject
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

    public bool allowMultiplePerFloor = true;
    public int minFloorGapBetweenPlacements = 0;
}