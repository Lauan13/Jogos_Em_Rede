using UnityEngine;

public class Tile : MonoBehaviour
{
    public Renderer rend;

    public void Highlight(Color color)
    {
        rend.material.color = color;
    }

    public void ResetColor(Color original)
    {
        rend.material.color = original;
    }
}