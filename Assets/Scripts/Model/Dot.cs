using UnityEngine;

public class Dot : MonoBehaviour
{
    public int gridX;
    public int gridY;
    public int colorId = 0;

    private SpriteRenderer sr;
    private Vector3 baseScale;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetColor(Color color, bool isEndpoint)
    {
        sr.color = color;
        // Endpoints should read as part of the pipe, rather than tiny dots below it.
        baseScale = isEndpoint ? new Vector3(0.78f, 0.78f, 1f) : new Vector3(0.12f, 0.12f, 1f);
        transform.localScale = baseScale;
        sr.sortingOrder = isEndpoint ? 3 : 0;
    }

}
