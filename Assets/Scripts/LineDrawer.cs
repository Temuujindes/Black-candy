using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    private class LineData
    {
        public LineRenderer renderer;
        public readonly List<Dot> dots = new List<Dot>();
        public int colorId;
        public Color color;
        public bool isComplete;
    }

    [Header("References")]
    public LevelManager levelManager;
    public GridManager gridManager;

    [Header("Pipe Look")]
    public float cellSize = 1.5f;
    [Range(0.1f, 1f)] public float pipeWidthRatio = 0.78f;

    private readonly List<LineData> allLines = new List<LineData>();
    private LineData currentLine;
    private bool isDrawing;
    private int totalDots;
    private int requiredLines;
    private bool inputEnabled = true;

    public int CompletedLineCount => allLines.FindAll(line => line.isComplete).Count;
    public int RequiredLineCount => requiredLines;

    private void Awake()
    {
        if (levelManager == null)
            levelManager = FindFirstObjectByType<LevelManager>();
        if (gridManager == null)
            gridManager = FindFirstObjectByType<GridManager>();
        if (gridManager != null) cellSize = gridManager.spacing;
    }

    private void Update()
    {
        if (!inputEnabled) return;
        if (Input.GetMouseButtonDown(0)) TryStartLine();
        if (Input.GetMouseButton(0) && isDrawing) TryAddDot();
        if (Input.GetMouseButtonUp(0)) FinishCurrentStroke();
    }

    private void TryStartLine()
    {
        Dot hitDot = GetDotUnderMouse(true);
        if (hitDot == null) return;

        LineData existingLine = FindLineContainingDot(hitDot);
        // An endpoint starts its colour. An occupied non-endpoint cell starts the
        // colour whose path owns it; in both cases the old path is fully erased.
        if (hitDot.colorId == 0 && existingLine == null) return;

        int colorId = existingLine != null ? existingLine.colorId : hitDot.colorId;
        Color color = existingLine != null ? existingLine.color : hitDot.GetComponent<SpriteRenderer>().color;
        RemoveLine(existingLine);

        currentLine = new LineData { colorId = colorId, color = color };
        currentLine.dots.Add(hitDot);
        currentLine.renderer = CreateRenderer(color);
        currentLine.renderer.positionCount = 1;
        currentLine.renderer.SetPosition(0, hitDot.transform.position);
        allLines.Add(currentLine);
        isDrawing = true;
    }

    private void TryAddDot()
    {
        if (currentLine == null || currentLine.isComplete) return;

        Dot hitDot = GetDotUnderMouse(false);
        if (hitDot == null) return;

        int existingIndex = currentLine.dots.IndexOf(hitDot);
        if (existingIndex >= 0)
        {
            // Dragging backward trims the active flow, which is much less
            // frustrating than forcing the player to start the whole path again.
            if (existingIndex < currentLine.dots.Count - 1)
            {
                currentLine.dots.RemoveRange(existingIndex + 1, currentLine.dots.Count - existingIndex - 1);
                currentLine.renderer.positionCount = currentLine.dots.Count;
            }
            return;
        }

        Dot lastDot = currentLine.dots[currentLine.dots.Count - 1];
        int dx = Mathf.Abs(lastDot.gridX - hitDot.gridX);
        int dy = Mathf.Abs(lastDot.gridY - hitDot.gridY);
        if (dx + dy != 1)
        {
            return;
        }

        // Other colours' endpoints are fixed obstacles. Entering a path's empty
        // cell erases that path before this one claims the cell.
        if (hitDot.colorId != 0 && hitDot.colorId != currentLine.colorId)
        {
            return;
        }

        LineData crossedLine = FindLineContainingDot(hitDot);
        if (crossedLine != null && crossedLine != currentLine) RemoveLine(crossedLine);

        currentLine.dots.Add(hitDot);
        currentLine.renderer.positionCount = currentLine.dots.Count;
        currentLine.renderer.SetPosition(currentLine.dots.Count - 1, hitDot.transform.position);

        if (hitDot.colorId == currentLine.colorId && currentLine.dots.Count > 1)
        {
            currentLine.isComplete = true;
            isDrawing = false;
            levelManager?.UpdateProgress(CompletedLineCount, requiredLines);
            CheckWinCondition();
        }
    }

    private void FinishCurrentStroke()
    {
        isDrawing = false;
        if (currentLine != null) currentLine.renderer.positionCount = currentLine.dots.Count;
        currentLine = null;
    }

    private LineRenderer CreateRenderer(Color color)
    {
        GameObject lineObject = new GameObject("Flow Line");
        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        float width = cellSize * pipeWidthRatio;
        line.startWidth = width;
        line.endWidth = width;
        line.numCapVertices = 12;
        line.numCornerVertices = 12;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.sortingLayerName = "Default";
        line.sortingOrder = 1;
        line.startColor = color;
        line.endColor = color;
        return line;
    }

    private Dot GetDotUnderMouse(bool reportFailure)
    {
        if (Camera.main == null)
        {
            Debug.LogError("Flow Free needs a camera tagged MainCamera to read mouse input.");
            return null;
        }

        Vector3 mouse = Input.mousePosition;
        mouse.z = -Camera.main.transform.position.z;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(mouse);

       
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        if (gridManager != null && gridManager.TryGetDotAtWorldPosition(mousePosition, out Dot dot)) return dot;
        return null;
    }

    private LineData FindLineContainingDot(Dot dot)
    {
        foreach (LineData line in allLines)
            if (line.dots.Contains(dot)) return line;
        return null;
    }

    private void RemoveLine(LineData line)
    {
        if (line == null) return;
        DestroyLineRenderer(line.renderer);
        allLines.Remove(line);
        if (currentLine == line) currentLine = null;
        levelManager?.UpdateProgress(CompletedLineCount, requiredLines);
    }

    private static void DestroyLineRenderer(LineRenderer renderer)
    {
        if (renderer == null) return;
        Material material = renderer.sharedMaterial;
        if (material != null) UnityEngine.Object.Destroy(material);
        UnityEngine.Object.Destroy(renderer.gameObject);
    }

    public void ResetForNewLevel(int newTotalDots, int newRequiredLines)
    {
        foreach (LineData line in allLines)
            DestroyLineRenderer(line.renderer);
        allLines.Clear();
        currentLine = null;
        isDrawing = false;
        totalDots = newTotalDots;
        requiredLines = newRequiredLines;
        if (gridManager != null) cellSize = gridManager.spacing;
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
        if (!enabled) FinishCurrentStroke();
    }

    private void OnDestroy()
    {
        foreach (LineData line in allLines)
            DestroyLineRenderer(line.renderer);
        allLines.Clear();
    }

    private void CheckWinCondition()
    {
        if (allLines.Count != requiredLines || requiredLines == 0) return;

        HashSet<Dot> coveredDots = new HashSet<Dot>();
        foreach (LineData line in allLines)
        {
            if (!line.isComplete) return;
            foreach (Dot dot in line.dots) coveredDots.Add(dot);
        }

        if (coveredDots.Count == totalDots && levelManager != null)
            levelManager.ShowNextButton();
    }
}
