using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class LunarLaunchSequence : MonoBehaviour
{
    private static readonly Color32 AccentColor = new Color32(98, 207, 240, 255);
    private static readonly Color32 ButtonColor = new Color32(18, 39, 61, 255);

    private GameObject launchCanvasObject;
    private Transform rocketTransform;
    private Transform exhaustTransform;
    private RectTransform progressFillRect;
    private TMP_Text statusText;
    private TMP_FontAsset font;
    private Button returnButton;
    private Vector3 launchStartPosition;
    private Vector3 lunarArrivalPosition;

    public static string RequestedSpacecraft { get; set; }

    private void Start()
    {
        TMP_FontAsset textFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (textFont == null)
            textFont = TMP_Settings.defaultFontAsset;

        if (textFont == null)
        {
            Debug.LogError("The lunar transfer scene could not find a TextMesh Pro font asset.", this);
            return;
        }

        if (string.IsNullOrWhiteSpace(RequestedSpacecraft))
        {
            Debug.LogError("The lunar transfer scene did not receive a spacecraft selection.", this);
            return;
        }

        Play(textFont, RequestedSpacecraft);
    }

    public void Play(TMP_FontAsset textFont, string spacecraftName)
    {
        if (textFont == null)
        {
            Debug.LogError("The lunar transfer screen requires a TextMesh Pro font.", this);
            return;
        }

        if (string.IsNullOrWhiteSpace(spacecraftName))
        {
            Debug.LogError("Select a spacecraft before starting the lunar transfer.", this);
            return;
        }

        if (launchCanvasObject != null)
            return;

        font = textFont;
        if (!TryInitializeSceneObjects())
            return;

        BuildSequenceScreen(spacecraftName);
        StartCoroutine(AnimateTransfer(spacecraftName));
    }

    private bool TryInitializeSceneObjects()
    {
        var rocket = GameObject.Find("Mission Rocket");
        var moon = GameObject.Find("Moon");
        var exhaust = GameObject.Find("Rocket Exhaust");
        if (rocket == null || moon == null || exhaust == null)
        {
            Debug.LogError("The lunar transfer scene is missing its rocket, Moon, or rocket exhaust object.", this);
            return false;
        }

        rocketTransform = rocket.transform;
        exhaustTransform = exhaust.transform;
        launchStartPosition = rocketTransform.position;
        lunarArrivalPosition = moon.transform.position + Vector3.left * 4.8f;
        return true;
    }

    private void BuildSequenceScreen(string spacecraftName)
    {
        launchCanvasObject = new GameObject(
            "Lunar Transfer Sequence",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        var canvas = launchCanvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = launchCanvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        CreateText(
            "Transfer Title",
            canvas.transform,
            "LUNAR TRANSFER",
            42f,
            FontStyles.Bold,
            AccentColor,
            TextAlignmentOptions.Center,
            new Vector2(0f, 420f),
            new Vector2(820f, 60f));
        CreateText(
            "Transfer Spacecraft",
            canvas.transform,
            "SPACECRAFT: " + spacecraftName,
            24f,
            FontStyles.Bold,
            Color.white,
            TextAlignmentOptions.Center,
            new Vector2(0f, 365f),
            new Vector2(900f, 40f));

        CreateText(
            "Earth Label",
            canvas.transform,
            "EARTH",
            24f,
            FontStyles.Bold,
            Color.white,
            TextAlignmentOptions.Center,
            new Vector2(-460f, 205f),
            new Vector2(240f, 38f));
        CreateText(
            "Moon Label",
            canvas.transform,
            "MOON",
            24f,
            FontStyles.Bold,
            Color.white,
            TextAlignmentOptions.Center,
            new Vector2(460f, 145f),
            new Vector2(240f, 38f));

        var progressRect = CreateRect(
            "Transfer Progress",
            canvas.transform,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, 0.5f),
            new Vector2(-510f, -205f),
            new Vector2(0f, 10f));
        progressFillRect = progressRect;
        progressRect.gameObject.AddComponent<Image>().color = AccentColor;
        var progressBackground = CreateRect(
            "Transfer Progress Background",
            canvas.transform,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, -205f),
            new Vector2(1020f, 10f));
        progressBackground.gameObject.AddComponent<Image>().color = new Color32(28, 47, 69, 255);
        progressRect.SetAsLastSibling();

        CreateText(
            "Transfer Progress Label",
            canvas.transform,
            "MISSION PROGRESS",
            19f,
            FontStyles.Bold,
            Color.white,
            TextAlignmentOptions.Center,
            new Vector2(0f, -174f),
            new Vector2(320f, 30f));
        statusText = CreateText(
            "Transfer Status",
            canvas.transform,
            "Preparing lunar transfer...",
            23f,
            FontStyles.Bold,
            Color.white,
            TextAlignmentOptions.Center,
            new Vector2(0f, -278f),
            new Vector2(900f, 42f));
        returnButton = CreateButton(
            "Return To Rover",
            canvas.transform,
            "RETURN TO ROVER LOADOUT",
            new Vector2(0f, -425f),
            new Vector2(360f, 64f),
            CloseSequence);
        returnButton.gameObject.SetActive(false);
    }

    private IEnumerator AnimateTransfer(string spacecraftName)
    {
        const float durationSeconds = 7f;
        float elapsed = 0f;

        while (elapsed < durationSeconds)
        {
            float progress = Mathf.Clamp01(elapsed / durationSeconds);
            UpdateTransfer(progress);
            statusText.text = progress < 0.18f
                ? "Liftoff from Earth"
                : progress < 0.82f
                    ? "Cruising through space toward the Moon"
                    : "Approaching lunar orbit";

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        UpdateTransfer(1f);
        statusText.text = spacecraftName + " has reached the Moon.";
        returnButton.gameObject.SetActive(true);
    }

    private void UpdateTransfer(float progress)
    {
        float arcHeight = Mathf.Sin(progress * Mathf.PI) * 2.4f;
        rocketTransform.position = Vector3.Lerp(launchStartPosition, lunarArrivalPosition, progress)
            + Vector3.up * arcHeight;
        float exhaustPulse = 0.75f + Mathf.PingPong(Time.unscaledTime * 1.8f, 0.45f);
        exhaustTransform.localScale = new Vector3(0.28f, exhaustPulse, 0.28f);
        progressFillRect.sizeDelta = new Vector2(1020f * progress, 10f);
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
        text.enableAutoSizing = true;
        text.fontSizeMin = size * 0.8f;
        text.fontSizeMax = size;
        return text;
    }

    private Button CreateButton(
        string objectName,
        Transform parent,
        string label,
        Vector2 position,
        Vector2 size,
        UnityEngine.Events.UnityAction onClick)
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
        button.onClick.AddListener(onClick);
        var labelText = CreateText(
            objectName + " Label",
            rect,
            label,
            23f,
            FontStyles.Bold,
            Color.white,
            TextAlignmentOptions.Center,
            Vector2.zero,
            size - new Vector2(20f, 12f));
        labelText.raycastTarget = false;
        return button;
    }

    private void CloseSequence()
    {
        SceneManager.LoadScene("LunarRoverScene");
    }

}
