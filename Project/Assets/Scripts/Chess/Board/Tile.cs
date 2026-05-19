using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int boardPosition;

    private SpriteRenderer sr;

    public Color normalColor;
    public Color highlightColor;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetColor(Color color)
    {
        sr.color = color;
    }

    public void Highlight()
    {
        sr.color = highlightColor;
    }

    public void ResetColor()
    {
        sr.color = normalColor;
    }
}