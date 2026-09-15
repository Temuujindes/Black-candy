using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public GridManager gridManager;
    public LineDrawer lineDrawer;
    public GameObject nextButton;
    public TextMeshProUGUI levelText;

    [System.Serializable]
    public class ColorPair { public int colorId; public Vector2Int start; public Vector2Int end; public Color color; }
    private class LevelData { public int width; public int height; public ColorPair[] pairs; }

    private LevelData[] levels;
    private int currentLevelIndex;
    private TextMeshProUGUI progressText;
    private Button nextLevelButton;
    private Button restartButton;

    private void Awake()
    {
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        if (lineDrawer == null) lineDrawer = FindFirstObjectByType<LineDrawer>();
    }

    private void Start()
    {
        SetupLevels();
        EnsureHud();
        LoadLevel(currentLevelIndex);
    }

    private void SetupLevels()
    {
       
        levels = new[]
        {
            MakeLevel(3, 3, Pair(1, 0, 0, 1, 1, Color.red), Pair(2, 0, 1, 2, 2, Color.blue)),
            MakeLevel(4, 4, Pair(1, 0, 0, 3, 0, Color.red), Pair(2, 0, 1, 3, 3, Color.blue)),
            MakeLevel(4, 4, Pair(1, 0, 0, 3, 0, Color.red), Pair(2, 0, 1, 3, 1, Color.blue), Pair(3, 0, 2, 0, 3, Color.green)),
            MakeLevel(5, 5, Pair(1, 0, 0, 4, 1, Color.red), Pair(2, 3, 1, 4, 2, Color.blue), Pair(3, 4, 3, 4, 4, Color.green)),
            MakeLevel(5, 5, Pair(1, 0, 0, 4, 0, Color.red), Pair(2, 4, 1, 0, 2, Color.blue), Pair(3, 1, 2, 0, 3, Color.green), Pair(4, 4, 4, 0, 4, Color.yellow))
        };
    }

    private static LevelData MakeLevel(int width, int height, params ColorPair[] pairs) => new LevelData { width = width, height = height, pairs = pairs };
    private static ColorPair Pair(int id, int sx, int sy, int ex, int ey, Color color) => new ColorPair { colorId = id, start = new Vector2Int(sx, sy), end = new Vector2Int(ex, ey), color = color };

    public void LoadNextLevel()
    {
        lineDrawer.SetInputEnabled(false);
        nextLevelButton.gameObject.SetActive(false);
        currentLevelIndex = (currentLevelIndex + 1) % levels.Length;
        LoadLevel(currentLevelIndex);
        lineDrawer.SetInputEnabled(true);
    }

    public void ReplayLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    private void LoadLevel(int index)
    {
        LevelData level = levels[index];
        levelText.text = $"LEVEL {index + 1}";
        progressText.text = $"FLOWS 0 / {level.pairs.Length}";
        lineDrawer.ResetForNewLevel(level.width * level.height, level.pairs.Length);
        Dot[,] grid = gridManager.GenerateGrid(level.width, level.height);
        foreach (ColorPair pair in level.pairs)
        {
            Dot start = grid[pair.start.x, pair.start.y];
            Dot end = grid[pair.end.x, pair.end.y];
            start.colorId = pair.colorId;
            end.colorId = pair.colorId;
            start.SetColor(pair.color, true);
            end.SetColor(pair.color, true);
        }
        if (nextButton != null) nextButton.SetActive(false);
        nextLevelButton.gameObject.SetActive(false);
    }

    public void ShowNextButton()
    {
        if (nextButton != null) nextButton.SetActive(false);
        nextLevelButton.gameObject.SetActive(true);
    }

    public void UpdateProgress(int completed, int required)
    {
        if (progressText != null) progressText.text = $"FLOWS {completed} / {required}";
    }

    private void EnsureHud()
    {
        KeepEventSystemActive();
        if (nextButton != null) nextButton.SetActive(false);
        GameObject legacyReplay = GameObject.Find("ReplayButton");
        if (legacyReplay != null) legacyReplay.SetActive(false);

        GameObject canvasObject = new GameObject("Flow HUD");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObject.AddComponent<GraphicRaycaster>();

        levelText = CreateText(canvas.transform, "Level", new Vector2(0.5f, 0.94f), new Vector2(480, 70), 42, FontStyles.Bold, new Color(0.92f, 0.97f, 1f));
        progressText = CreateText(canvas.transform, "Flows", new Vector2(0.5f, 0.895f), new Vector2(480, 46), 24, FontStyles.UpperCase, new Color(0.53f, 0.76f, 0.92f));
        restartButton = CreateButton(canvas.transform, "Restart", new Vector2(0.31f, 0.085f), new Color(0.14f, 0.30f, 0.47f));
        restartButton.onClick.AddListener(ReplayLevel);
        nextLevelButton = CreateButton(canvas.transform, "Candy", new Vector2(0.69f, 0.085f), new Color(0.16f, 0.56f, 0.40f));
        nextLevelButton.onClick.AddListener(LoadNextLevel);
        nextLevelButton.gameObject.SetActive(false);
    }

    private void KeepEventSystemActive()
    {
        // The original scene accidentally stores the EventSystem below the
        // NextButton. Disabling that button also disabled every UI click.
        if (EventSystem.current != null) return;
        if (nextButton == null) return;

        EventSystem eventSystem = nextButton.GetComponentInChildren<EventSystem>(true);
        if (eventSystem == null) return;
        eventSystem.transform.SetParent(null, true);
        eventSystem.gameObject.SetActive(true);
    }

    private TextMeshProUGUI CreateText(Transform parent, string objectName, Vector2 anchor, Vector2 size, float fontSize, FontStyles style, Color color)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.text = objectName.ToUpperInvariant();
        return text;
    }

    private Button CreateButton(Transform parent, string label, Vector2 anchor, Color color)
    {
        GameObject buttonObject = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(210, 68);
        buttonObject.GetComponent<Image>().color = color;
        Button button = buttonObject.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        TextMeshProUGUI text = CreateText(buttonObject.transform, label, new Vector2(0.5f, 0.5f), new Vector2(210, 68), 26, FontStyles.Bold, Color.white);
        text.raycastTarget = false;
        return button;
    }

}
