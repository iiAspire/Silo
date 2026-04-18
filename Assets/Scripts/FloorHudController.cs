using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloorHudController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FloorVisibilityController floorVisibilityController;
    [SerializeField] private FloorCameraController floorCameraController;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;

    [Header("Jump UI")]
    [SerializeField] private TMP_InputField floorJumpInput;
    [SerializeField] private Button jumpButton;

    [Header("Quick Jump UI")]
    [SerializeField] private Button jumpTopButton;
    [SerializeField] private Button jumpBottomButton;

    [Header("Label")]
    [SerializeField] private string floorPrefix = "Floor ";
    [SerializeField] private bool oneBasedFloorNumbers = true;
    [SerializeField] private bool showTotalFloors = true;

    private TMP_Text placeholderText;

    private void Start()
    {
        if (floorVisibilityController == null)
            floorVisibilityController = FindFirstObjectByType<FloorVisibilityController>();

        if (floorCameraController == null)
            floorCameraController = FindFirstObjectByType<FloorCameraController>();

        if (previousButton != null)
        {
            previousButton.onClick.RemoveAllListeners();
            previousButton.onClick.AddListener(OnPreviousClicked);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextClicked);
        }

        if (jumpButton != null)
        {
            jumpButton.onClick.RemoveAllListeners();
            jumpButton.onClick.AddListener(OnJumpClicked);
        }

        if (floorJumpInput != null)
        {
            floorJumpInput.onSubmit.RemoveAllListeners();
            floorJumpInput.onSubmit.AddListener(OnJumpSubmitted);

            if (floorJumpInput.placeholder != null)
                placeholderText = floorJumpInput.placeholder.GetComponent<TMP_Text>();
        }

        if (jumpTopButton != null)
        {
            jumpTopButton.onClick.RemoveAllListeners();
            jumpTopButton.onClick.AddListener(OnJumpTopClicked);
        }

        if (jumpBottomButton != null)
        {
            jumpBottomButton.onClick.RemoveAllListeners();
            jumpBottomButton.onClick.AddListener(OnJumpBottomClicked);
        }

        Invoke(nameof(RefreshAll), 0.2f);
    }

    public void RefreshAll()
    {
        if (floorVisibilityController != null)
            floorVisibilityController.RebuildFloorList();

        if (floorCameraController != null)
        {
            floorCameraController.RebuildFloorList();
            floorCameraController.SnapToCurrentFloor();
        }

        RefreshUI();
    }

    public void OnPreviousClicked()
    {
        floorCameraController?.PreviousFloor();
        RefreshUI();
    }

    public void OnNextClicked()
    {
        floorCameraController?.NextFloor();
        RefreshUI();
    }

    public void OnJumpClicked()
    {
        TryJumpToFloor();
    }

    public void OnJumpSubmitted(string _)
    {
        TryJumpToFloor();
    }

    private void TryJumpToFloor()
    {
        if (floorJumpInput == null || floorVisibilityController == null || floorCameraController == null)
            return;

        int totalFloors = floorVisibilityController.GetFloorCount();
        if (totalFloors <= 0)
            return;

        string raw = floorJumpInput.text.Trim();
        if (!int.TryParse(raw, out int requestedFloor))
            return;

        int targetIndex = oneBasedFloorNumbers ? requestedFloor - 1 : requestedFloor;
        targetIndex = Mathf.Clamp(targetIndex, 0, totalFloors - 1);

        floorVisibilityController.currentFloor = targetIndex;
        floorVisibilityController.UpdateVisibility();
        floorCameraController.RefreshAfterFloorChange();

        floorJumpInput.text = "";
        RefreshUI();
    }

    private void Update()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (floorVisibilityController == null)
            return;

        int currentFloor = floorVisibilityController.currentFloor;
        int totalFloors = floorVisibilityController.GetFloorCount();

        int displayFloor = oneBasedFloorNumbers ? currentFloor + 1 : currentFloor;
        int displayTotal = oneBasedFloorNumbers ? totalFloors : Mathf.Max(0, totalFloors - 1);

        string label = showTotalFloors
            ? $"{floorPrefix}{displayFloor} / {displayTotal}"
            : $"{floorPrefix}{displayFloor}";

        if (placeholderText != null)
            placeholderText.text = label;

        if (previousButton != null)
            previousButton.interactable = currentFloor > 0;

        if (nextButton != null)
            nextButton.interactable = totalFloors > 0 && currentFloor < totalFloors - 1;

        if (jumpTopButton != null)
            jumpTopButton.interactable = totalFloors > 0 && currentFloor > 0;

        if (jumpBottomButton != null)
            jumpBottomButton.interactable = totalFloors > 0 && currentFloor < totalFloors - 1;
    }

    public void OnJumpTopClicked()
    {
        JumpToTopFloor();
    }

    public void OnJumpBottomClicked()
    {
        JumpToBottomFloor();
    }

    private void JumpToTopFloor()
    {
        if (floorVisibilityController == null || floorCameraController == null)
            return;

        int totalFloors = floorVisibilityController.GetFloorCount();
        if (totalFloors <= 0)
            return;

        floorVisibilityController.currentFloor = 0;
        floorVisibilityController.UpdateVisibility();
        floorCameraController.RefreshAfterFloorChange();
        RefreshUI();
    }

    private void JumpToBottomFloor()
    {
        if (floorVisibilityController == null || floorCameraController == null)
            return;

        int totalFloors = floorVisibilityController.GetFloorCount();
        if (totalFloors <= 0)
            return;

        floorVisibilityController.currentFloor = totalFloors - 1;
        floorVisibilityController.UpdateVisibility();
        floorCameraController.RefreshAfterFloorChange();
        RefreshUI();
    }
}