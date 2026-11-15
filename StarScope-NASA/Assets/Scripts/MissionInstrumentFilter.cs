using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionInstrumentFilter : MonoBehaviour
{
    private const string NavigationCameras = "Navigation and hazard-detection cameras";
    private const string WaterIceObjective = "Search for water ice";
    private const string SamplesObjective = "Analyze lunar samples";
    private const string RadiationObjective = "Measure radiation levels";
    private const string EnvironmentObjective = "Monitor dust and temperature";
    private const string CommunicationsObjective = "Test communications";
    private const string LavaTubeObjective = "Find lava tubes for radiation shelter";

    private static readonly HashSet<string> KnownInstruments = new HashSet<string>
    {
        "LROC",
        NavigationCameras,
        "Seismometers",
        "Mass spectrometers",
        "TRIDENT",
        "Sample collection tools",
        "Lunar Laser Ranging Retroreflectors",
        "crewTransportation Starship",
        "CRaTER",
        "crewTransportation Orion",
        "Radiation monitors and dosimeters",
        "Dust, temperature and plasma sensors",
        "LEND",
        "Diviner Lunar Radiometer",
        "Mini-RF"
    };

    private void Awake()
    {
        ApplyObjective(MissionObjectiveSelection.GetSelectedObjective());
    }

    private void ApplyObjective(string objective)
    {
        var recommendedInstruments = new HashSet<string>(GetRecommendedInstruments(objective));
        var buttons = GetComponentsInChildren<Button>(true)
            .Where(button => !button.gameObject.name.Contains("NextStageButton") &&
                             !button.gameObject.name.Contains("PreviousStageButton"))
            .ToArray();

        if (buttons.Length == 0)
        {
            Debug.LogError("Mission instrument menu contains no instrument buttons.", this);
            enabled = false;
            return;
        }

        var buttonsByCategory = new Dictionary<GameObject, List<Button>>();
        foreach (var button in buttons)
        {
            string instrument = GetInstrumentName(button.gameObject.name);
            if (!KnownInstruments.Contains(instrument))
            {
                Debug.LogWarning($"Unrecognized rover instrument option '{instrument}' was hidden.", button);
                button.gameObject.SetActive(false);
                continue;
            }

            button.gameObject.SetActive(recommendedInstruments.Contains(instrument));
            var category = button.transform.parent.gameObject;
            if (!buttonsByCategory.TryGetValue(category, out var categoryButtons))
            {
                categoryButtons = new List<Button>();
                buttonsByCategory.Add(category, categoryButtons);
            }

            categoryButtons.Add(button);
        }

        var visibleCategories = buttonsByCategory
            .Where(pair => pair.Value.Any(button => button.gameObject.activeSelf))
            .Select(pair => pair.Key)
            .OrderBy(GetCategoryOrder)
            .ToArray();

        foreach (var category in buttonsByCategory.Keys)
            category.SetActive(visibleCategories.Contains(category));

        for (int i = 0; i < visibleCategories.Length; i++)
        {
            var rect = visibleCategories[i].GetComponent<RectTransform>();
            if (rect == null)
            {
                Debug.LogError($"Instrument category '{visibleCategories[i].name}' has no RectTransform.", visibleCategories[i]);
                continue;
            }

            float x = visibleCategories.Length == 1 ? 0f : (i % 2 == 0 ? -320f : 320f);
            float y = 220f - (i / 2) * 190f;
            rect.localPosition = new Vector3(x, y, rect.localPosition.z);
        }

        var heading = GetComponentsInChildren<TMP_Text>(true)
            .FirstOrDefault(text => text.gameObject.name == "ConceptDesign");
        if (heading == null)
        {
            Debug.LogError("Mission instrument menu is missing its heading.", this);
            return;
        }

        heading.fontSize = 34f;
        heading.text = objective == CommunicationsObjective
            ? $"Lunar Rover: {objective}\nCommunications use a built-in rover system."
            : $"Recommended rover instruments for:\n{objective}";
    }

    private static string GetInstrumentName(string buttonName)
    {
        string name = buttonName.Trim();
        const string buttonSuffix = "Button";
        if (name.EndsWith(buttonSuffix, StringComparison.OrdinalIgnoreCase))
            name = name.Substring(0, name.Length - buttonSuffix.Length).Trim();

        return name;
    }

    public static string[] GetRecommendedInstruments(string objective)
    {
        var instruments = new List<string> { NavigationCameras };

        if (objective == WaterIceObjective)
        {
            instruments.Add("Mass spectrometers");
            instruments.Add("TRIDENT");
            instruments.Add("Sample collection tools");
        }
        else if (objective == SamplesObjective)
        {
            instruments.Add("Mass spectrometers");
            instruments.Add("Sample collection tools");
        }
        else if (objective == RadiationObjective)
        {
            instruments.Add("Radiation monitors and dosimeters");
        }
        else if (objective == EnvironmentObjective)
        {
            instruments.Add("Dust, temperature and plasma sensors");
        }
        else if (objective == LavaTubeObjective)
        {
            instruments.Add("Radiation monitors and dosimeters");
            instruments.Add("Dust, temperature and plasma sensors");
        }

        return instruments.ToArray();
    }

    private static int GetCategoryOrder(GameObject category)
    {
        switch (category.name)
        {
            case "Camera":
                return 0;
            case "Spectrometers":
                return 1;
            case "Drilling":
                return 2;
            case "Sample collection":
                return 3;
            case "Radiation sensors":
                return 4;
            default:
                return 5;
        }
    }
}
