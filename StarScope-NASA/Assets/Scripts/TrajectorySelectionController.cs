using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TrajectorySelectionController : MonoBehaviour
{
    private static readonly string[] TrajectoryNames =
    {
        "Direct Lunar Transfer",
        "Hohmann-like Lunar Transfer",
        "Free-Return Trajectory",
        "Ballistic Lunar Transfer (BLT)",
        "Weak Stability Boundary (WSB) Transfer",
        "Low-Energy Lunar Transfer",
        "Low-Thrust Spiral Transfer",
        "Phasing Loop Transfer"
    };

    private static readonly string[] TrajectoryDescriptions =
    {
        "Leave Earth orbit and travel straight toward the Moon.",
        "Follow a simple oval-shaped orbit from Earth toward the Moon.",
        "Fly past the Moon, then naturally follow a path back to Earth.",
        "Use the gravity of Earth and the Moon to save fuel, usually with a longer trip.",
        "Use a weak-gravity region near the Moon to help capture the spacecraft. This route can take longer.",
        "Use less fuel than a direct route, usually by taking longer.",
        "Use a steady, low-powered engine to gradually move outward until reaching the Moon's distance from Earth.",
        "Make extra orbits around Earth to line up the timing for a trip to the Moon."
    };

    public Button[] trajectoryButtons;
    public Image[] trajectoryButtonImages;
    public TMP_Text descriptionTitle;
    public TMP_Text descriptionBody;
    public GameObject descriptionPanel;
    public Button nextButton;

    public static string SelectedTrajectory { get; private set; }

    private readonly Color normalButtonColor = new Color(0.10f, 0.19f, 0.30f, 0.96f);
    private readonly Color selectedButtonColor = new Color(0.12f, 0.42f, 0.58f, 1f);
    private int selectedIndex = -1;

    private void Awake()
    {
        if (trajectoryButtons == null ||
            trajectoryButtonImages == null ||
            trajectoryButtons.Length != TrajectoryNames.Length ||
            trajectoryButtonImages.Length != TrajectoryNames.Length ||
            descriptionTitle == null ||
            descriptionBody == null ||
            descriptionPanel == null ||
            nextButton == null)
        {
            Debug.LogError("Trajectory selection UI is missing required references.", this);
            enabled = false;
            return;
        }

        SelectedTrajectory = null;
        descriptionPanel.SetActive(false);
        nextButton.interactable = false;
        nextButton.onClick.AddListener(ContinueToLaunch);

        for (int i = 0; i < trajectoryButtons.Length; i++)
        {
            int optionIndex = i;
            if (trajectoryButtons[i] == null || trajectoryButtonImages[i] == null)
            {
                Debug.LogError($"Trajectory option {i} is missing its button or image.", this);
                enabled = false;
                return;
            }

            trajectoryButtons[i].transition = Selectable.Transition.None;
            trajectoryButtons[i].onClick.AddListener(() => SelectTrajectory(optionIndex));
            trajectoryButtonImages[i].color = normalButtonColor;
            AddHoverEvents(trajectoryButtons[i], optionIndex);
        }

    }

    public void SelectTrajectory(int index)
    {
        if (index < 0 || index >= TrajectoryNames.Length)
        {
            Debug.LogError($"Trajectory index {index} is out of range.", this);
            return;
        }

        selectedIndex = index;
        SelectedTrajectory = TrajectoryNames[index];
        nextButton.interactable = true;

        for (int i = 0; i < trajectoryButtonImages.Length; i++)
            trajectoryButtonImages[i].color = i == selectedIndex ? selectedButtonColor : normalButtonColor;
    }

    public void ShowDescription(int index)
    {
        if (index < 0 || index >= TrajectoryNames.Length)
            return;

        descriptionTitle.text = TrajectoryNames[index];
        descriptionBody.text = TrajectoryDescriptions[index];
        descriptionPanel.SetActive(true);
    }

    public void HideDescription()
    {
        descriptionPanel.SetActive(false);
    }

    public void ContinueToLaunch()
    {
        if (selectedIndex < 0)
        {
            Debug.LogError("Select a lunar trajectory before continuing.", this);
            return;
        }

        SceneManager.LoadScene("LaunchScene");
    }

    private void AddHoverEvents(Button button, int index)
    {
        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        if (trigger.triggers == null)
            trigger.triggers = new List<EventTrigger.Entry>();

        EventTrigger.Entry pointerEnter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        pointerEnter.callback.AddListener(_ => ShowDescription(index));
        trigger.triggers.Add(pointerEnter);

        EventTrigger.Entry pointerExit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        pointerExit.callback.AddListener(_ => HideDescription());
        trigger.triggers.Add(pointerExit);
    }
}
