using UnityEngine;
using UnityEngine.UI;

public class SimulationSpeedController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button verySlowButton;
    [SerializeField] private Button slowButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button fastButton;

    [Header("Colors")]
    [SerializeField] private Color activeColor = new Color(0.2f, 0.7f, 1f, 1f);
    [SerializeField] private Color inactiveColor = new Color(1f, 1f, 1f, 1f);

    private Button currentSelectedButton;

    private void Start()
    {
        SetNormalSpeed();
    }

    public void TogglePause()
    {
        if (Time.timeScale > 0f)
        {
            Time.timeScale = 0f;
            SetSelectedButton(pauseButton);
        }
        else
        {
            Time.timeScale = 1f;
            SetSelectedButton(normalButton);
        }
    }

    public void SetPaused()
    {
        Time.timeScale = 0f;
        SetSelectedButton(pauseButton);
    }

    public void SetVerySlow()
    {
        Time.timeScale = 0.05f;
        SetSelectedButton(verySlowButton);
    }

    public void SetSlow()
    {
        Time.timeScale = 0.25f;
        SetSelectedButton(slowButton);
    }

    public void SetNormalSpeed()
    {
        Time.timeScale = 1f;
        SetSelectedButton(normalButton);
    }

    public void SetFast()
    {
        Time.timeScale = 2f;
        SetSelectedButton(fastButton);
    }

    private void SetSelectedButton(Button selected)
    {
        currentSelectedButton = selected;

        SetButtonColor(pauseButton, pauseButton == selected ? activeColor : inactiveColor);
        SetButtonColor(verySlowButton, verySlowButton == selected ? activeColor : inactiveColor);
        SetButtonColor(slowButton, slowButton == selected ? activeColor : inactiveColor);
        SetButtonColor(normalButton, normalButton == selected ? activeColor : inactiveColor);
        SetButtonColor(fastButton, fastButton == selected ? activeColor : inactiveColor);
    }

    private void SetButtonColor(Button button, Color color)
    {
        if (button == null)
            return;

        Image image = button.GetComponent<Image>();
        if (image != null)
            image.color = color;
    }
}