using UnityEngine;

public class TowerGenerator : MonoBehaviour
{
    public Transform floorsParent;
    public GameObject floorPrefab;
    public int floorCount = 144;
    public float floorHeight = 2.5f;

    private void Start()
    {
        GenerateFloors();
    }

    private void GenerateFloors()
    {
        for (int i = 0; i < floorCount; i++)
        {
            GameObject floor = Instantiate(floorPrefab, floorsParent);
            floor.name = "Floor_" + i.ToString("D3");
            floor.transform.localPosition = new Vector3(0f, i * floorHeight, 0f);
        }

        InitializeFloors();
    }

    private void InitializeFloors()
    {
        Floor[] floors = floorsParent.GetComponentsInChildren<Floor>();
        foreach (Floor floor in floors)
            floor.Initialize();
    }
}