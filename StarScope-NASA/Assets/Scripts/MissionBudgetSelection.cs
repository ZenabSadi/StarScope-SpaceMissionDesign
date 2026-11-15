using System.Globalization;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TMP_InputField))]
public class MissionBudgetSelection : MonoBehaviour
{
    private const string PlayerPrefsKey = "StarScope.MissionBudget";
    private static readonly NumberStyles BudgetNumberStyles =
        NumberStyles.Integer | NumberStyles.AllowThousands;

    private TMP_InputField budgetInput;

    public static bool TryGetBudget(out int budgetCredits)
    {
        string savedBudget = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
        return TryParseBudget(savedBudget, out budgetCredits);
    }

    private void Awake()
    {
        budgetInput = GetComponent<TMP_InputField>();
        string savedBudget = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);

        if (!string.IsNullOrEmpty(savedBudget))
        {
            if (TryParseBudget(savedBudget, out int budgetCredits))
            {
                budgetInput.SetTextWithoutNotify(budgetCredits.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                Debug.LogWarning("The saved mission budget was invalid and has been cleared.", this);
                PlayerPrefs.DeleteKey(PlayerPrefsKey);
                PlayerPrefs.Save();
            }
        }

        budgetInput.onEndEdit.AddListener(SaveBudget);
    }

    private void OnDestroy()
    {
        if (budgetInput != null)
            budgetInput.onEndEdit.RemoveListener(SaveBudget);
    }

    private void SaveBudget(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            PlayerPrefs.DeleteKey(PlayerPrefsKey);
            PlayerPrefs.Save();
            return;
        }

        if (!TryParseBudget(value, out int budgetCredits))
        {
            Debug.LogWarning("Mission budget must be a non-negative whole number of credits.", this);
            budgetInput.SetTextWithoutNotify(PlayerPrefs.GetString(PlayerPrefsKey, string.Empty));
            return;
        }

        PlayerPrefs.SetString(PlayerPrefsKey, budgetCredits.ToString(CultureInfo.InvariantCulture));
        PlayerPrefs.Save();
    }

    private static bool TryParseBudget(string value, out int budgetCredits)
    {
        return int.TryParse(value, BudgetNumberStyles, CultureInfo.InvariantCulture, out budgetCredits) &&
               budgetCredits >= 0;
    }
}
