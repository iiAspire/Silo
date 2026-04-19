using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FloorVisibilityController : MonoBehaviour
{
    public Transform floorsParent;
    public int currentFloor = 0;

    [Header("View Ranges")]
    public int closeVisibleRange = 1;
    //public int mediumVisibleRange = 10;
    public int farVisibleRange = 6;

    [Header("Current Mode")]
    public int visibleRange = 1;

    private Transform[] floors;

    private void Start()
    {
        RebuildFloorList();
        SetCloseView();
    }

    private void Update()
    {
        if (Keyboard.current == null || floors == null || floors.Length == 0)
            return;

        if (Keyboard.current.pageUpKey.wasPressedThisFrame)
        {
            currentFloor = Mathf.Clamp(currentFloor + 1, 0, floors.Length - 1);
            UpdateVisibility();
        }

        if (Keyboard.current.pageDownKey.wasPressedThisFrame)
        {
            currentFloor = Mathf.Clamp(currentFloor - 1, 0, floors.Length - 1);
            UpdateVisibility();
        }
    }

    public void RebuildFloorList()
    {
        if (floorsParent == null)
        {
            floors = new Transform[0];
            Debug.LogWarning("FloorVisibilityController has no floorsParent assigned.");
            return;
        }

        List<Transform> floorList = new List<Transform>();

        foreach (Transform child in floorsParent.GetComponentsInChildren<Transform>(true))
        {
            if (child != floorsParent && child.name.StartsWith("Floor_"))
                floorList.Add(child);
        }

        floors = floorList.ToArray();
        System.Array.Sort(floors, (a, b) => b.position.y.CompareTo(a.position.y));

        //Debug.Log($"FloorVisibilityController rebuilt floor list: {floors.Length} floors found.");
    }

    public void UpdateVisibility()
    {
        if (floors == null || floors.Length == 0)
            return;

        currentFloor = Mathf.Clamp(currentFloor, 0, floors.Length - 1);

        for (int i = 0; i < floors.Length; i++)
        {
            bool shouldBeVisible = Mathf.Abs(i - currentFloor) <= visibleRange;
            floors[i].gameObject.SetActive(shouldBeVisible);

            //Debug.Log($"Floor {i} ({floors[i].name}) visible = {shouldBeVisible}");
        }
    }

    public void SetCloseView()
    {
        visibleRange = closeVisibleRange;
        UpdateVisibility();
    }

    //public void SetMediumView()
    //{
    //    visibleRange = mediumVisibleRange;
    //    UpdateVisibility();
    //}

    public void SetFarView()
    {
        visibleRange = farVisibleRange;
        UpdateVisibility();
    }

    public void SetViewRange(int range)
    {
        visibleRange = Mathf.Max(0, range);
        UpdateVisibility();
    }

    public int GetFloorCount()
    {
        return floors != null ? floors.Length : 0;
    }
}