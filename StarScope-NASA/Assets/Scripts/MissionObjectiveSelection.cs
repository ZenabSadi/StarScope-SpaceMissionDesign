using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Dropdown))]
public class MissionObjectiveSelection : MonoBehaviour
{
    private const string PlayerPrefsKey = "StarScope.MissionObjective";
    private const string DefaultObjective = "Map lunar surface";

    public static string SelectedObjective { get; private set; }

    private TMP_Dropdown objectiveDropdown;

    public static string GetSelectedObjective()
    {
        if (string.IsNullOrEmpty(SelectedObjective))
            SelectedObjective = PlayerPrefs.GetString(PlayerPrefsKey, DefaultObjective);

        return SelectedObjective;
    }

    private void Awake()
    {
        objectiveDropdown = GetComponent<TMP_Dropdown>();
        if (objectiveDropdown.options == null || objectiveDropdown.options.Count == 0)
        {
            Debug.LogError("Mission objective dropdown has no options.", this);
            enabled = false;
            return;
        }

        string savedObjective = PlayerPrefs.GetString(PlayerPrefsKey, GetSelectedObjective());
        int savedIndex = objectiveDropdown.options.FindIndex(option => option.text == savedObjective);
        if (savedIndex >= 0)
            objectiveDropdown.SetValueWithoutNotify(savedIndex);

        SetSelectedObjective(objectiveDropdown.value);
        objectiveDropdown.onValueChanged.AddListener(OnObjectiveChanged);
    }

    private void OnDestroy()
    {
        if (objectiveDropdown != null)
            objectiveDropdown.onValueChanged.RemoveListener(OnObjectiveChanged);
    }

    private void OnObjectiveChanged(int index)
    {
        SetSelectedObjective(index);
    }

    private void SetSelectedObjective(int index)
    {
        if (index < 0 || index >= objectiveDropdown.options.Count)
        {
            Debug.LogError($"Mission objective index {index} is out of range.", this);
            return;
        }

        SelectedObjective = objectiveDropdown.options[index].text;
        PlayerPrefs.SetString(PlayerPrefsKey, SelectedObjective);
        PlayerPrefs.Save();
    }
}
