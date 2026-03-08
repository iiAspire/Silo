using UnityEngine;

public class FloorSlot : MonoBehaviour
{
    public GameObject floorPrefab;
    private GameObject spawnedFloor;

    void Start()
    {
        BuildFloor();
    }

    public void BuildFloor()
    {
        if (floorPrefab == null || spawnedFloor != null)
            return;

        spawnedFloor = Instantiate(floorPrefab, transform);

        spawnedFloor.transform.localPosition = Vector3.zero;
        spawnedFloor.transform.localRotation = Quaternion.identity;
    }
}