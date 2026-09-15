using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public GameObject dotPrefab;
    public GameObject backgroundTilePrefab;
    public float spacing = 1.5f;
    [Header("Board Look")]
    public Color lightCellColor = new Color(0.17f, 0.25f, 0.37f, 1f);
    public Color darkCellColor = new Color(0.13f, 0.20f, 0.30f, 1f);
    public Color frameColor = new Color(0.06f, 0.10f, 0.16f, 1f);

    private Dot[,] currentGrid;
    private List<GameObject> currentDots = new List<GameObject>();
    private List<GameObject> currentTiles = new List<GameObject>();
    private GameObject currentFrame;
    private int currentWidth;
    private int currentHeight;
    private float currentOffsetX;
    private float currentOffsetY;
    private static Sprite fallbackTileSprite;

    public Dot[,] GenerateGrid(int width, int height)
    {
        ClearGrid();
        currentGrid = new Dot[width, height];
        currentWidth = width;
        currentHeight = height;

        currentOffsetX = (width - 1) * spacing / 2f;
        currentOffsetY = (height - 1) * spacing / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = new Vector2(x * spacing - currentOffsetX, y * spacing - currentOffsetY);

                if (backgroundTilePrefab != null)
                {
                    GameObject tile = Instantiate(backgroundTilePrefab, position, Quaternion.identity);
                    // A tile is visual only. Its collider must never take a board click.
                    foreach (Collider2D tileCollider in tile.GetComponentsInChildren<Collider2D>())
                    {
                        Destroy(tileCollider);
                    }
                    currentTiles.Add(tile);
                }
                else
                {
                    currentTiles.Add(CreateFallbackTile(position, (x + y) % 2 == 0 ? lightCellColor : darkCellColor));
                }

                GameObject dotObj = Instantiate(dotPrefab, position, Quaternion.identity);
                Dot dot = dotObj.GetComponent<Dot>();
                dot.gridX = x;
                dot.gridY = y;
                dot.SetColor(Color.gray, false);

                currentGrid[x, y] = dot;
                currentDots.Add(dotObj);
            }
        }

        CreateFrame(width, height);

        return currentGrid;
    }

    public bool TryGetDotAtWorldPosition(Vector2 worldPosition, out Dot dot)
    {
        dot = null;
        if (currentGrid == null || spacing <= 0f) return false;

        int x = Mathf.RoundToInt((worldPosition.x + currentOffsetX) / spacing);
        int y = Mathf.RoundToInt((worldPosition.y + currentOffsetY) / spacing);
        if (x < 0 || x >= currentWidth || y < 0 || y >= currentHeight) return false;

        Vector2 centre = new Vector2(x * spacing - currentOffsetX, y * spacing - currentOffsetY);
        // Do not snap clicks outside the visible board into an edge cell.
        if (Mathf.Abs(worldPosition.x - centre.x) > spacing * 0.48f ||
            Mathf.Abs(worldPosition.y - centre.y) > spacing * 0.48f)
            return false;

        dot = currentGrid[x, y];
        return dot != null;
    }

    private GameObject CreateFallbackTile(Vector2 position, Color color)
    {
        if (fallbackTileSprite == null)
            fallbackTileSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        GameObject tile = new GameObject("Board Cell");
        tile.transform.position = new Vector3(position.x, position.y, 0.2f);
        tile.transform.localScale = Vector3.one * spacing * 0.94f;
        SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
        renderer.sprite = fallbackTileSprite;
        renderer.color = color;
        renderer.sortingOrder = -2;
        return tile;
    }

    private void CreateFrame(int width, int height)
    {
        currentFrame = new GameObject("Board Frame");
        LineRenderer frame = currentFrame.AddComponent<LineRenderer>();
        frame.material = new Material(Shader.Find("Sprites/Default"));
        frame.startColor = frameColor;
        frame.endColor = frameColor;
        frame.startWidth = spacing * 0.12f;
        frame.endWidth = spacing * 0.12f;
        frame.numCornerVertices = 6;
        frame.sortingOrder = -1;
        float halfWidth = width * spacing * 0.5f;
        float halfHeight = height * spacing * 0.5f;
        frame.positionCount = 5;
        frame.SetPositions(new[]
        {
            new Vector3(-halfWidth, -halfHeight, 0.15f), new Vector3(-halfWidth, halfHeight, 0.15f),
            new Vector3(halfWidth, halfHeight, 0.15f), new Vector3(halfWidth, -halfHeight, 0.15f),
            new Vector3(-halfWidth, -halfHeight, 0.15f)
        });
    }

    public void ClearGrid()
    {
        foreach (GameObject dot in currentDots)
        {
            Destroy(dot);
        }
        currentDots.Clear();

        foreach (GameObject tile in currentTiles)
        {
            Destroy(tile);
        }
        currentTiles.Clear();

        if (currentFrame != null)
        {
            LineRenderer frame = currentFrame.GetComponent<LineRenderer>();
            if (frame != null && frame.sharedMaterial != null) Destroy(frame.sharedMaterial);
            Destroy(currentFrame);
            currentFrame = null;
        }
    }
}
