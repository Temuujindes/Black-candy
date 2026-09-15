using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject dotPrefab;
    public int gridWidth = 5;
    public int gridHeight = 5;
    public float spacing = 1.5f;

    void Start()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 position = new Vector2(x * spacing, y * spacing);
                Instantiate(dotPrefab, position, Quaternion.identity);
            }
        }
    }
}