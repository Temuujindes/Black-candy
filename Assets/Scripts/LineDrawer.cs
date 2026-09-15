using UnityEngine;
using System.Collections.Generic;

public class LineDrawer : MonoBehaviour
{
    private class LineData
    {
        public LineRenderer renderer;
        public List<Transform> dots = new List<Transform>();
    }

    private List<LineData> allLines = new List<LineData>();
    private LineData currentLineData;
    private bool isDrawing = false;

    public GameObject nextButton;
    public int totalDots;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryStartLine();
        }

        if (Input.GetMouseButton(0) && isDrawing)
        {
            TryAddDot();
            UpdatePreviewLine();
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
        }

        CheckWinCondition();
    }

    void TryStartLine()
    {
        Dot hitDot = GetDotUnderMouse();
        if (hitDot == null) return;

        LineData existingLine = FindLineContainingDot(hitDot.transform);
        if (existingLine != null)
        {
            Destroy(existingLine.renderer.gameObject);
            allLines.Remove(existingLine);
        }

        isDrawing = true;

        currentLineData = new LineData();
        currentLineData.dots.Add(hitDot.transform);

        GameObject lineObj = new GameObject("Line");
        currentLineData.renderer = lineObj.AddComponent<LineRenderer>();
        currentLineData.renderer.startWidth = 0.1f;
        currentLineData.renderer.endWidth = 0.1f;
        currentLineData.renderer.positionCount = 1;
        currentLineData.renderer.SetPosition(0, hitDot.transform.position);

        allLines.Add(currentLineData);
    }

    void TryAddDot()
    {
        Dot hitDot = GetDotUnderMouse();

        if (hitDot != null && !currentLineData.dots.Contains(hitDot.transform))
        {
            Vector2 newPointA = currentLineData.dots[currentLineData.dots.Count - 1].position;
            Vector2 newPointB = hitDot.transform.position;

            CheckAndRemoveIntersectingLines(newPointA, newPointB);

            currentLineData.dots.Add(hitDot.transform);
            currentLineData.renderer.positionCount = currentLineData.dots.Count;
            currentLineData.renderer.SetPosition(currentLineData.dots.Count - 1, hitDot.transform.position);
        }
    }

    void UpdatePreviewLine()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int previewIndex = currentLineData.dots.Count;
        currentLineData.renderer.positionCount = previewIndex + 1;
        currentLineData.renderer.SetPosition(previewIndex, mousePos);
    }

    Dot GetDotUnderMouse()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            return hit.collider.GetComponent<Dot>();
        }
        return null;
    }

    LineData FindLineContainingDot(Transform dot)
    {
        foreach (LineData line in allLines)
        {
            if (line.dots.Contains(dot))
            {
                return line;
            }
        }
        return null;
    }

    void CheckAndRemoveIntersectingLines(Vector2 newA, Vector2 newB)
    {
        List<LineData> toRemove = new List<LineData>();

        foreach (LineData line in allLines)
        {
            if (line == currentLineData) continue;

            for (int i = 0; i < line.dots.Count - 1; i++)
            {
                Vector2 a = line.dots[i].position;
                Vector2 b = line.dots[i + 1].position;

                if (LinesIntersect(newA, newB, a, b))
                {
                    toRemove.Add(line);
                    break;
                }
            }
        }

        foreach (LineData line in toRemove)
        {
            Destroy(line.renderer.gameObject);
            allLines.Remove(line);
        }
    }

    bool LinesIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        float d1 = Cross(p4 - p3, p1 - p3);
        float d2 = Cross(p4 - p3, p2 - p3);
        float d3 = Cross(p2 - p1, p3 - p1);
        float d4 = Cross(p2 - p1, p4 - p1);

        if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
            ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
        {
            return true;
        }
        return false;
    }

    float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    void CheckWinCondition()
    {
        int connectedCount = 0;
        foreach (LineData line in allLines)
        {
            connectedCount += line.dots.Count;
        }

        if (connectedCount >= totalDots && nextButton != null)
        {
            nextButton.SetActive(true);
        }
    }
}