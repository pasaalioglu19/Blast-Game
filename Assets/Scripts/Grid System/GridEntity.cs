using UnityEngine;

public abstract class GridEntity : MonoBehaviour
{
    [SerializeField] private int gridX;
    [SerializeField] private int gridY;

    public int GetGridX() => gridX;
    public int GetGridY() => gridY;

    public void SetGridX(int x) => gridX = x;
    public void SetGridY(int y) => gridY = y;
}
