using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LunarRoverLoadoutMenu : MonoBehaviour
{
    private sealed class OptionStats
    {
        public readonly string Name;
        public readonly float MassKg;
        public readonly int CostCredits;
        public readonly float HeightMeters;
        public readonly float WidthMeters;
        public readonly float? LunarPayloadCapacityKg;

        public OptionStats(
            string name,
            float massKg,
            int costCredits,
            float heightMeters,
            float widthMeters,
            float? lunarPayloadCapacityKg = null)
        {
            Name = name;
            MassKg = massKg;
            CostCredits = costCredits;
            HeightMeters = heightMeters;
            WidthMeters = widthMeters;
            LunarPayloadCapacityKg = lunarPayloadCapacityKg;
        }
    }

    private static readonly Color32 PanelColor = new Color32(7, 18, 35, 245);
    private static readonly Color32 ButtonColor = new Color32(18, 39, 61, 255);
    private static readonly Color32 SelectedColor = new Color32(238, 105, 0, 255);
    private static readonly Color32 WarningColor = new Color32(190, 40, 48, 255);
    private static readonly Color32 AccentColor = new Color32(98, 207, 240, 255);
    private static readonly OptionStats RoverStats = new OptionStats(
        "Lunar Rover", 600f, 400000, 1.8f, 2.4f);
    private static readonly OptionStats[] PowerSourceOptions =
    {
        new OptionStats("Solar panels", 42f, 35000, 0.15f, 2.0f),
        new OptionStats("Rechargeable batteries", 60f, 16000, 0.3f, 0.6f),
        new OptionStats("Primary batteries", 45f, 10000, 0.28f, 0.55f),
        new OptionStats("Radioisotope generators (RTG)", 55f, 180000, 0.65f, 0.5f),
        new OptionStats("Nuclear fission reactors", 200f, 350000, 1.1f, 0.8f)
    };
    private static readonly OptionStats[] InstrumentOptions =
    {
        new OptionStats("Navigation and hazard-detection cameras", 8f, 20000, 0.2f, 0.25f),
        new OptionStats("Mass spectrometers", 15f, 90000, 0.32f, 0.35f),
        new OptionStats("TRIDENT", 18f, 120000, 0.55f, 0.25f),
        new OptionStats("Sample collection tools", 10f, 35000, 0.35f, 0.3f),
        new OptionStats("Radiation monitors and dosimeters", 2f, 15000, 0.12f, 0.12f),
        new OptionStats("Dust, temperature and plasma sensors", 4f, 25000, 0.22f, 0.18f)
    };
    private static readonly OptionStats[] SpacecraftOptions =
    {
        new OptionStats("Nova-C", 1900f, 550000, 4.3f, 1.6f, 130f),
        new OptionStats("Blue Ghost", 2000f, 600000, 2f, 2f, 150f),
        new OptionStats("Blue Moon MK1", 2500f, 800000, 4.5f, 3f, 3000f),
        new OptionStats("Griffin-1", 2600f, 700000, 4f, 2.5f, 625f)
    };

    private readonly Dictionary<string, Button> tabButtons = new Dictionary<string, Button>();
    private readonly Dictionary<string, Button> powerSourceButtons = new Dictionary<string, Button>();
    private readonly Dictionary<string, Button> spacecraftButtons = new Dictionary<string, Button>();
    private readonly Dictionary<string, Button> instrumentButtons = new Dictionary<string, Button>();
    private readonly HashSet<string> selectedPowerSources = new HashSet<string>();
    private readonly HashSet<string> selectedInstruments = new HashSet<string>();

    private TMP_FontAsset font;
    private TextMeshProUGUI panelTitle;
    private TextMeshProUGUI powerSelectionLabel;
    private TextMeshProUGUI spacecraftSelectionLabel;
    private TextMeshProUGUI instrumentSelectionLabel;
    private TextMeshProUGUI detailsTitle;
    private TextMeshProUGUI detailsText;
    private TextMeshProUGUI spendingCounterText;
    private TextMeshProUGUI payloadMassCounterText;
    private TextMeshProUGUI spacecraftFitText;
    private Image spendingCounterBackground;
    private Image payloadMassCounterBackground;
    private Image spacecraftFitBackground;
    private Button confirmationButton;
    private Button launchButton;
    private TextMeshProUGUI launchButtonHint;
    private RectTransform panelContent;
    private GameObject panel;
    private GameObject detailsPanel;
    private GameObject totalsPanel;
    private MainMenuController sceneNavigation;
    private string openTab;
    private string selectedSpacecraft;
    private string instrumentObjective;

    private void Awake()
    {
        font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (font == null)
            font = TMP_Settings.defaultFontAsset;

        if (font == null)
        {
            Debug.LogError("The rover loadout menu could not find a TextMesh Pro font asset.", this);
            enabled = false;
            return;
        }

        EnsureEventSystem();
        sceneNavigation = GetComponent<MainMenuController>();
        if (sceneNavigation == null)
            sceneNavigation = gameObject.AddComponent<MainMenuController>();
        selectedSpacecraft = LunarLaunchSequence.RequestedSpacecraft;
        EnsureInstrumentSelection(MissionObjectiveSelection.GetSelectedObjective());
        BuildMenu();
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null)
            return;

        var eventSystemObject = new GameObject(
            "EventSystem",
            typeof(EventSystem),
            typeof(InputSystemUIInputModule));
        eventSystemObject.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
    }

    private void BuildMenu()
    {
        var canvasObject = new GameObject(
            "Rover Loadout Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        CreateText(
            "Loadout Title",
            canvas.transform,
            "ROVER MISSION DESIGN",
            38f,
            FontStyles.Bold,
            AccentColor,
            TextAlignmentOptions.Center,
            new Vector2(0f, 490f),
            new Vector2(760f, 58f));

        CreateTab(canvas.transform, "Power Source", new Vector2(-375f, 410f));
        CreateTab(canvas.transform, "Instruments", new Vector2(-125f, 410f));
        CreateTab(canvas.transform, "Spacecraft", new Vector2(125f, 410f));
        CreateTab(canvas.transform, "Rover", new Vector2(375f, 410f));
        var backButton = CreateButton(
            "Back to Mission Design",
            canvas.transform,
            "Back: Mission Design",
            Vector2.zero,
            new Vector2(320f, 60f),
            sceneNavigation.PreviousStage,
            22f);
        var backRect = backButton.GetComponent<RectTransform>();
        backRect.anchorMin = Vector2.zero;
        backRect.anchorMax = Vector2.zero;
        backRect.pivot = Vector2.zero;
        backRect.anchoredPosition = new Vector2(32f, 24f);
        BuildPanel(canvas.transform);
        BuildLaunchControls(canvas.transform);
    }

    private void BuildLaunchControls(Transform parent)
    {
        launchButton = CreateButton(
            "Launch To Moon",
            parent,
            "LAUNCH",
            Vector2.zero,
            new Vector2(280f, 68f),
            LaunchMission,
            28f);
        var launchRect = launchButton.GetComponent<RectTransform>();
        launchRect.anchorMin = new Vector2(1f, 0f);
        launchRect.anchorMax = new Vector2(1f, 0f);
        launchRect.pivot = new Vector2(1f, 0f);
        launchRect.anchoredPosition = new Vector2(-32f, 24f);

        launchButtonHint = CreateText(
            "Launch Button Hint",
            parent,
            string.Empty,
            17f,
            FontStyles.Bold,
            AccentColor,
            TextAlignmentOptions.Right,
            Vector2.zero,
            new Vector2(400f, 28f));
        var hintRect = launchButtonHint.rectTransform;
        hintRect.anchorMin = new Vector2(1f, 0f);
        hintRect.anchorMax = new Vector2(1f, 0f);
        hintRect.pivot = new Vector2(1f, 0f);
        hintRect.anchoredPosition = new Vector2(-32f, 100f);
        UpdateLaunchButton();
    }

    private void UpdateLaunchButton()
    {
        if (launchButton == null || launchButtonHint == null)
            return;

        bool canLaunch = !string.IsNullOrEmpty(selectedSpacecraft);
        launchButton.interactable = canLaunch;
        launchButton.GetComponent<Image>().color = canLaunch ? SelectedColor : ButtonColor;
        launchButtonHint.text = canLaunch
            ? "READY FOR LUNAR TRANSFER"
            : "Select a spacecraft to enable launch";
    }

    private void LaunchMission()
    {
        if (string.IsNullOrEmpty(selectedSpacecraft))
            return;

        LunarLaunchSequence.RequestedSpacecraft = selectedSpacecraft;
        SceneManager.LoadScene("LunarTransferScene");
    }

    private void CreateTab(Transform parent, string tabName, Vector2 position)
    {
        var button = CreateButton(
            tabName,
            parent,
            tabName,
            position,
            new Vector2(230f, 64f),
            () => TogglePanel(tabName),
            26f);
        tabButtons.Add(tabName, button);
    }

    private void BuildPanel(Transform parent)
    {
        var panelRect = CreateRect(
            "Loadout Panel",
            parent,
            new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f),
            new Vector2(28f, 0f),
            new Vector2(560f, 700f));
        panel = panelRect.gameObject;
        panel.AddComponent<Image>().color = PanelColor;
        panel.SetActive(false);

        panelTitle = CreateText(
            "Panel Title",
            panel.transform,
            string.Empty,
            34f,
            FontStyles.Bold,
            AccentColor,
            TextAlignmentOptions.Center,
            new Vector2(0f, 320f),
            new Vector2(440f, 52f));

        CreateButton(
            "Close Panel",
            panel.transform,
            "X",
            new Vector2(244f, 320f),
            new Vector2(42f, 42f),
            ClosePanel,
            24f);

        panelContent = CreateRect(
            "Panel Content",
            panel.transform,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            new Vector2(520f, 580f));

        confirmationButton = CreateButton(
            "Confirm Selection",
            panel.transform,
            "Confirm Selection",
            new Vector2(150f, -320f),
            new Vector2(210f, 54f),
            ConfirmSelection,
            21f);
        confirmationButton.gameObject.SetActive(false);

        var detailsRect = CreateRect(
            "Object Data Panel",
            parent,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            new Vector2(604f, 0f),
            new Vector2(470f, 700f));
        detailsPanel = detailsRect.gameObject;
        detailsPanel.AddComponent<Image>().color = PanelColor;
        detailsPanel.SetActive(false);

        CreateText(
            "Object Data Heading",
            detailsPanel.transform,
            "OBJECT DATA",
            28f,
            FontStyles.Bold,
            AccentColor,
            TextAlignmentOptions.Center,
            new Vector2(0f, 310f),
            new Vector2(420f, 46f));

        detailsTitle = CreateText(
            "Object Data Title",
            detailsPanel.transform,
            "Select an item",
            26f,
            FontStyles.Bold,
            Color.white,
            TextAlignmentOptions.Center,
            new Vector2(0f, 245f),
            new Vector2(420f, 52f));
        detailsTitle.enableAutoSizing = true;
        detailsTitle.fontSizeMin = 20f;
        detailsTitle.fontSizeMax = 26f;

        detailsText = CreateText(
            "Object Data Details",
            detailsPanel.transform,
            "Hover over or select an item to view its specifications.",
            23f,
            FontStyles.Normal,
            Color.white,
            TextAlignmentOptions.TopLeft,
            new Vector2(0f, 20f),
            new Vector2(420f, 360f));

        var totalsRect = CreateRect(
            "Loadout Totals Panel",
            parent,
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(-32f, 160f),
            new Vector2(470f, 220f));
        totalsPanel = totalsRect.gameObject;
        totalsPanel.AddComponent<Image>().color = PanelColor;

        CreateText(
            "Current Loadout Totals Heading",
            totalsPanel.transform,
            "CURRENT LOADOUT TOTALS",
            20f,
            FontStyles.Bold,
            AccentColor,
            TextAlignmentOptions.Center,
            new Vector2(0f, 55f),
            new Vector2(420f, 34f));

        var spendingCounterRect = CreateRect(
            "Spending Counter Field",
            totalsPanel.transform,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, 12f),
            new Vector2(420f, 36f));
        spendingCounterBackground = spendingCounterRect.gameObject.AddComponent<Image>();
        spendingCounterBackground.color = ButtonColor;
        spendingCounterText = CreateText(
            "Spending Counter",
            spendingCounterRect,
            string.Empty,
            20f,
            FontStyles.Bold,
            Color.white,
            TextAlignmentOptions.Center,
            Vector2.zero,
            new Vector2(400f, 32f));
        spendingCounterText.enableAutoSizing = true;
        spendingCounterText.fontSizeMin = 16f;
        spendingCounterText.fontSizeMax = 20f;

        var payloadMassCounterRect = CreateRect(
            "Payload Mass Counter Field",
            totalsPanel.transform,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, -32f),
            new Vector2(420f, 36f));
        payloadMassCounterBackground = payloadMassCounterRect.gameObject.AddComponent<Image>();
        payloadMassCounterBackground.color = ButtonColor;
        payloadMassCounterText = CreateText(
            "Payload Mass Counter",
            payloadMassCounterRect,
            string.Empty,
            20f,
            FontStyles.Bold,
            Color.white,
            TextAlignmentOptions.Center,
            Vector2.zero,
            new Vector2(400f, 32f));
        payloadMassCounterText.enableAutoSizing = true;
        payloadMassCounterText.fontSizeMin = 16f;
        payloadMassCounterText.fontSizeMax = 20f;

        var spacecraftFitRect = CreateRect(
            "Spacecraft Fit Status Field",
            totalsPanel.transform,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, -76f),
            new Vector2(420f, 36f));
        spacecraftFitBackground = spacecraftFitRect.gameObject.AddComponent<Image>();
        spacecraftFitBackground.color = ButtonColor;
        spacecraftFitText = CreateText(
            "Spacecraft Fit Status",
            spacecraftFitRect,
            string.Empty,
            18f,
            FontStyles.Bold,
            Color.white,
            TextAlignmentOptions.Center,
            Vector2.zero,
            new Vector2(400f, 32f));
        spacecraftFitText.enableAutoSizing = true;
        spacecraftFitText.fontSizeMin = 14f;
        spacecraftFitText.fontSizeMax = 18f;

        UpdateLoadoutCounters();
    }

    private void TogglePanel(string tabName)
    {
        if (openTab == tabName && panel.activeSelf)
        {
            ClosePanel();
            return;
        }

        openTab = tabName;
        ClearPanelContent();
        panelTitle.text = tabName.ToUpperInvariant();
        panel.SetActive(true);
        detailsPanel.SetActive(true);
        confirmationButton.gameObject.SetActive(tabName != "Rover");
        ResetConfirmation();

        if (tabName == "Power Source")
            BuildPowerSourcePanel();
        else if (tabName == "Instruments")
            BuildInstrumentsPanel();
        else if (tabName == "Spacecraft")
            BuildSpacecraftPanel();
        else if (tabName == "Rover")
            BuildRoverPanel();

        UpdateLoadoutCounters();
        UpdateTabHighlights();
    }

    private void ClosePanel()
    {
        openTab = null;
        panel.SetActive(false);
        detailsPanel.SetActive(false);
        UpdateTabHighlights();
    }

    private void BuildPowerSourcePanel()
    {
        CreateText(
            "Power Source Prompt",
            panelContent,
            "Select any number of power sources\nApproximate in-game estimates | kg, m, credits",
            20f,
            FontStyles.Normal,
            Color.white,
            TextAlignmentOptions.Center,
            new Vector2(0f, 230f),
            new Vector2(480f, 62f));

        powerSourceButtons.Clear();
        for (int i = 0; i < PowerSourceOptions.Length; i++)
        {
            OptionStats option = PowerSourceOptions[i];
            var button = CreateButton(
                option.Name,
                panelContent,
                option.Name,
                new Vector2(0f, 145f - i * 72f),
                new Vector2(460f, 58f),
                () => SelectPowerSource(option.Name),
                25f);
            AddHoverEvents(button.gameObject, option.Name, FormatOptionStats(option));
            powerSourceButtons.Add(option.Name, button);
        }

        powerSelectionLabel = CreateText(
            "Selected Power Source",
            panelContent,
            string.Empty,
            20f,
            FontStyles.Normal,
            Color.white,
            TextAlignmentOptions.Center,
            new Vector2(0f, -198f),
            new Vector2(480f, 30f));
        UpdatePowerSourceHighlights();

        OptionStats detailsOption = PowerSourceOptions[0];
        foreach (OptionStats option in PowerSourceOptions)
        {
            if (!selectedPowerSources.Contains(option.Name))
                continue;

            detailsOption = option;
            break;
        }
        ShowDetails(detailsOption.Name, FormatOptionStats(detailsOption));
    }

    private void BuildSpacecraftPanel()
    {
        CreateText(
            "Spacecraft Prompt",
            panelContent,
            "Select one spacecraft\nHover for stats and lunar payload capacity",
            20f,
            FontStyles.Normal,
            Color.white,
            TextAlignmentOptions.Center,
            new Vector2(0f, 230f),
            new Vector2(480f, 62f));

        spacecraftButtons.Clear();
        for (int i = 0; i < SpacecraftOptions.Length; i++)
        {
            OptionStats option = SpacecraftOptions[i];
            var button = CreateButton(
                option.Name,
                panelContent,
                option.Name,
                new Vector2(0f, 135f - i * 82f),
                new Vector2(460f, 70f),
                () => SelectSpacecraft(option.Name),
                26f);
            AddHoverEvents(button.gameObject, option.Name, FormatOptionStats(option));
            spacecraftButtons.Add(option.Name, button);
        }

        spacecraftSelectionLabel = CreateText(
            "Selected Spacecraft",
            panelContent,
            string.Empty,
            20f,
            FontStyles.Normal,
            Color.white,
            TextAlignmentOptions.Center,
            new Vector2(0f, -198f),
            new Vector2(480f, 30f));
        UpdateSpacecraftHighlights();

        OptionStats detailsOption = FindOption(SpacecraftOptions, selectedSpacecraft);
        if (detailsOption == null)
            detailsOption = SpacecraftOptions[0];
        ShowDetails(detailsOption.Name, FormatOptionStats(detailsOption));
    }

    private void BuildInstrumentsPanel()
    {
        string objective = MissionObjectiveSelection.GetSelectedObjective();
        string[] instruments = MissionInstrumentFilter.GetRecommendedInstruments(objective);
        EnsureInstrumentSelection(objective);

        CreateText(
            "Mission Objective",
            panelContent,
            "Objective: " + objective + "\nOptional: select none, one, or several instruments.",
            20f,
            FontStyles.Normal,
            Color.white,
            TextAlignmentOptions.Center,
            new Vector2(0f, 230f),
            new Vector2(480f, 62f));

        instrumentButtons.Clear();
        for (int i = 0; i < instruments.Length; i++)
        {
            OptionStats option = FindOption(InstrumentOptions, instruments[i]);
            if (option == null)
            {
                Debug.LogError($"No rover statistics are defined for instrument '{instruments[i]}'.", this);
                continue;
            }

            var button = CreateButton(
                option.Name,
                panelContent,
                option.Name,
                new Vector2(0f, 135f - i * 82f),
                new Vector2(460f, 70f),
                () => ToggleInstrument(option.Name),
                24f);
            AddHoverEvents(button.gameObject, option.Name, FormatOptionStats(option));
            instrumentButtons.Add(option.Name, button);
        }

        instrumentSelectionLabel = CreateText(
            "Instrument Selection Summary",
            panelContent,
            string.Empty,
            20f,
            FontStyles.Normal,
            Color.white,
            TextAlignmentOptions.Center,
            new Vector2(0f, -198f),
            new Vector2(480f, 30f));
        UpdateInstrumentHighlights();

        OptionStats detailsOption = FindSelectedInstrument(instruments);
        if (detailsOption == null && instruments.Length > 0)
            detailsOption = FindOption(InstrumentOptions, instruments[0]);
        if (detailsOption != null)
            ShowDetails(detailsOption.Name, FormatOptionStats(detailsOption));
    }

    private void EnsureInstrumentSelection(string objective)
    {
        if (instrumentObjective == objective)
            return;

        instrumentObjective = objective;
        selectedInstruments.Clear();
    }

    private void BuildRoverPanel()
    {
        CreateCard(
            "Rover Base Stats",
            "LUNAR ROVER BASE",
            FormatOptionStats(RoverStats),
            panelContent,
            new Vector2(0f, 160f),
            new Vector2(460f, 110f));
        CreateCard(
            "Assembled Rover Stats",
            "ASSEMBLED ROVER TOTAL",
            FormatRoverTotal(),
            panelContent,
            new Vector2(0f, 10f),
            new Vector2(460f, 130f));
        CreateText(
            "Rover Stats Note",
            panelContent,
            "Rover and assembled loadout details are shown in the panel to the right.",
            18f,
            FontStyles.Normal,
            Color.white,
            TextAlignmentOptions.Center,
            new Vector2(0f, -115f),
            new Vector2(480f, 42f));
        ShowDetails(
            "Rover Loadout",
            "LUNAR ROVER BASE\n" + FormatOptionStats(RoverStats) +
            "\n\nASSEMBLED ROVER TOTAL\n" + FormatRoverTotal());
    }

    private OptionStats FindSelectedInstrument(string[] recommendedInstruments)
    {
        foreach (string instrumentName in recommendedInstruments)
        {
            if (selectedInstruments.Contains(instrumentName))
                return FindOption(InstrumentOptions, instrumentName);
        }

        return null;
    }

    private void SelectPowerSource(string powerSource)
    {
        if (!selectedPowerSources.Add(powerSource))
            selectedPowerSources.Remove(powerSource);

        UpdatePowerSourceHighlights();
        OptionStats option = FindOption(PowerSourceOptions, powerSource);
        if (option != null)
            ShowDetails(option.Name, FormatOptionStats(option));
        UpdateLoadoutCounters();
        ResetConfirmation();
    }

    private void SelectSpacecraft(string spacecraft)
    {
        selectedSpacecraft = selectedSpacecraft == spacecraft ? null : spacecraft;
        UpdateSpacecraftHighlights();
        OptionStats option = FindOption(SpacecraftOptions, spacecraft);
        if (option != null)
            ShowDetails(option.Name, FormatOptionStats(option));
        UpdateLoadoutCounters();
        UpdateLaunchButton();
        ResetConfirmation();
    }

    private void ToggleInstrument(string instrument)
    {
        if (!selectedInstruments.Add(instrument))
            selectedInstruments.Remove(instrument);

        UpdateInstrumentHighlights();
        OptionStats option = FindOption(InstrumentOptions, instrument);
        if (option != null)
            ShowDetails(option.Name, FormatOptionStats(option));
        UpdateLoadoutCounters();
        ResetConfirmation();
    }

    private void UpdatePowerSourceHighlights()
    {
        foreach (var item in powerSourceButtons)
        {
            var image = item.Value.GetComponent<Image>();
            image.color = selectedPowerSources.Contains(item.Key) ? SelectedColor : ButtonColor;
        }

        if (powerSelectionLabel != null)
            powerSelectionLabel.text = "Selected power sources: " + selectedPowerSources.Count;
    }

    private void UpdateSpacecraftHighlights()
    {
        foreach (var item in spacecraftButtons)
        {
            var image = item.Value.GetComponent<Image>();
            image.color = item.Key == selectedSpacecraft ? SelectedColor : ButtonColor;
        }

        if (spacecraftSelectionLabel != null)
            spacecraftSelectionLabel.text = selectedSpacecraft == null
                ? "No spacecraft selected"
                : "Selected spacecraft: " + selectedSpacecraft;
    }

    private void UpdateInstrumentHighlights()
    {
        foreach (var item in instrumentButtons)
        {
            bool included = selectedInstruments.Contains(item.Key);
            item.Value.GetComponent<Image>().color = included ? SelectedColor : ButtonColor;
        }

        if (instrumentSelectionLabel != null)
            instrumentSelectionLabel.text = "Selected instruments: " + selectedInstruments.Count;
    }

    private void UpdateTabHighlights()
    {
        foreach (var item in tabButtons)
        {
            var image = item.Value.GetComponent<Image>();
            image.color = item.Key == openTab ? SelectedColor : ButtonColor;
        }
    }

    private void AddHoverEvents(GameObject target, string title, string details)
    {
        var trigger = target.AddComponent<EventTrigger>();
        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ => ShowDetails(title, details));
        trigger.triggers.Add(enter);
    }

    private void ShowDetails(string title, string details)
    {
        detailsTitle.text = title;
        detailsText.text = details;
        detailsPanel.SetActive(true);
    }

    private void ConfirmSelection()
    {
        var label = confirmationButton.GetComponentInChildren<TextMeshProUGUI>();
        label.text = "Selection Confirmed";
        confirmationButton.interactable = false;
        confirmationButton.GetComponent<Image>().color = SelectedColor;
    }

    private void ResetConfirmation()
    {
        if (confirmationButton == null)
            return;

        confirmationButton.interactable = true;
        confirmationButton.GetComponentInChildren<TextMeshProUGUI>().text = "Confirm Selection";
        confirmationButton.GetComponent<Image>().color = ButtonColor;
    }

    private void ClearPanelContent()
    {
        powerSelectionLabel = null;
        spacecraftSelectionLabel = null;
        instrumentSelectionLabel = null;
        powerSourceButtons.Clear();
        spacecraftButtons.Clear();
        instrumentButtons.Clear();
        for (int i = panelContent.childCount - 1; i >= 0; i--)
            Destroy(panelContent.GetChild(i).gameObject);
    }

    private void CreateCard(
        string objectName,
        string label,
        string hoverDetails,
        Transform parent,
        Vector2 position,
        Vector2 size)
    {
        var rect = CreateRect(
            objectName,
            parent,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            position,
            size);
        rect.gameObject.AddComponent<Image>().color = ButtonColor;
        AddHoverEvents(rect.gameObject, label, hoverDetails);
        CreateText(
            objectName + " Label",
            rect,
            label,
            26f,
            FontStyles.Bold,
            Color.white,
            TextAlignmentOptions.Center,
            Vector2.zero,
            size - new Vector2(24f, 12f));
    }

    private void CalculateRoverLoadoutTotals(out float massKg, out int costCredits)
    {
        massKg = RoverStats.MassKg;
        costCredits = RoverStats.CostCredits;
        foreach (string powerSourceName in selectedPowerSources)
        {
            OptionStats powerSource = FindOption(PowerSourceOptions, powerSourceName);
            if (powerSource == null)
                continue;

            massKg += powerSource.MassKg;
            costCredits += powerSource.CostCredits;
        }

        foreach (string instrumentName in selectedInstruments)
        {
            OptionStats instrument = FindOption(InstrumentOptions, instrumentName);
            if (instrument == null)
                continue;

            massKg += instrument.MassKg;
            costCredits += instrument.CostCredits;
        }
    }

    private void UpdateLoadoutCounters()
    {
        if (spendingCounterText == null || payloadMassCounterText == null)
            return;

        CalculateRoverLoadoutTotals(out float massKg, out int roverCostCredits);
        OptionStats spacecraft = FindOption(SpacecraftOptions, selectedSpacecraft);
        int spendingCredits = roverCostCredits + (spacecraft == null ? 0 : spacecraft.CostCredits);

        if (MissionBudgetSelection.TryGetBudget(out int budgetCredits))
        {
            spendingCounterText.text = "Spending: " + FormatCredits(spendingCredits) +
                                       " / " + FormatCredits(budgetCredits) + " credits";
            spendingCounterBackground.color = spendingCredits > budgetCredits
                ? WarningColor
                : ButtonColor;
        }
        else
        {
            spendingCounterText.text = "Spending: " + FormatCredits(spendingCredits) +
                                       " credits (set budget)";
            spendingCounterBackground.color = ButtonColor;
        }

        if (spacecraft != null && spacecraft.LunarPayloadCapacityKg.HasValue)
        {
            float payloadCapacityKg = spacecraft.LunarPayloadCapacityKg.Value;
            payloadMassCounterText.text = "Payload mass: " + FormatNumber(massKg) +
                                          " / " + FormatNumber(payloadCapacityKg) + " kg";
            payloadMassCounterBackground.color = massKg > payloadCapacityKg
                ? WarningColor
                : ButtonColor;
        }
        else
        {
            payloadMassCounterText.text = "Payload mass: " + FormatNumber(massKg) +
                                          " kg (select spacecraft)";
            payloadMassCounterBackground.color = ButtonColor;
        }

        UpdateSpacecraftFitStatus(spacecraft);
    }

    private void UpdateSpacecraftFitStatus(OptionStats spacecraft)
    {
        if (spacecraftFitText == null || spacecraftFitBackground == null)
            return;

        if (spacecraft == null)
        {
            spacecraftFitText.text = "Select a spacecraft to check fit";
            spacecraftFitBackground.color = ButtonColor;
            return;
        }

        bool roverFits = FitsInSpacecraft(RoverStats, spacecraft);
        bool instrumentsFit = true;
        foreach (string instrumentName in selectedInstruments)
        {
            OptionStats instrument = FindOption(InstrumentOptions, instrumentName);
            if (instrument != null && !FitsInSpacecraft(instrument, spacecraft))
            {
                instrumentsFit = false;
                break;
            }
        }

        if (!roverFits || !instrumentsFit)
        {
            spacecraftFitText.text = !roverFits
                ? "SPACECRAFT TOO SMALL FOR ROVER"
                : "SPACECRAFT TOO SMALL FOR INSTRUMENT";
            spacecraftFitBackground.color = WarningColor;
            return;
        }

        spacecraftFitText.text = "FIT CHECK: OK";
        spacecraftFitBackground.color = ButtonColor;
    }

    private static bool FitsInSpacecraft(OptionStats payload, OptionStats spacecraft)
    {
        return payload.HeightMeters <= spacecraft.HeightMeters &&
               payload.WidthMeters <= spacecraft.WidthMeters;
    }

    private string FormatRoverTotal()
    {
        CalculateRoverLoadoutTotals(out float massKg, out int costCredits);

        return "Mass: " + FormatNumber(massKg) + " kg\n" +
               "Cost: " + FormatCredits(costCredits) + " credits\n" +
               "Envelope: H " + FormatNumber(RoverStats.HeightMeters) + " m | W " +
               FormatNumber(RoverStats.WidthMeters) + " m\n" +
               "Power sources: " + selectedPowerSources.Count + " | Instruments: " + selectedInstruments.Count;
    }

    private static string FormatOptionStats(OptionStats option)
    {
        string details = "Approximate in-game estimate\n" +
               "Mass: " + FormatNumber(option.MassKg) + " kg\n" +
               "Cost: " + FormatCredits(option.CostCredits) + " credits\n" +
               "Height: " + FormatNumber(option.HeightMeters) + " m\n" +
               "Width: " + FormatNumber(option.WidthMeters) + " m";
        if (option.LunarPayloadCapacityKg.HasValue)
            details += "\nLunar payload (estimate): " +
                       FormatNumber(option.LunarPayloadCapacityKg.Value) + " kg";

        return details;
    }

    private static string FormatNumber(float value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string FormatCredits(int value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture);
    }

    private static OptionStats FindOption(OptionStats[] options, string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        for (int i = 0; i < options.Length; i++)
        {
            if (options[i].Name == name)
                return options[i];
        }

        return null;
    }

    private Button CreateButton(
        string objectName,
        Transform parent,
        string label,
        Vector2 position,
        Vector2 size,
        Action onClick,
        float fontSize = 23f)
    {
        var rect = CreateRect(
            objectName,
            parent,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            position,
            size);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = ButtonColor;
        var button = rect.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(() => onClick());

        var labelRect = CreateRect(
            objectName + " Label",
            rect,
            Vector2.zero,
            Vector2.one,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero);
        labelRect.offsetMin = new Vector2(12f, 8f);
        labelRect.offsetMax = new Vector2(-12f, -8f);
        var text = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.text = label;
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        return button;
    }

    private TextMeshProUGUI CreateText(
        string objectName,
        Transform parent,
        string value,
        float size,
        FontStyles style,
        Color color,
        TextAlignmentOptions alignment,
        Vector2 position,
        Vector2 rectSize)
    {
        var rect = CreateRect(
            objectName,
            parent,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            position,
            rectSize);
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        return text;
    }

    private RectTransform CreateRect(
        string objectName,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 position,
        Vector2 size)
    {
        var gameObject = new GameObject(objectName, typeof(RectTransform));
        var rect = gameObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }
}
