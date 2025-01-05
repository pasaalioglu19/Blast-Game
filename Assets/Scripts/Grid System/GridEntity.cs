using UnityEngine;
using UnityEngine.SocialPlatforms;

public abstract class GridEntity : MonoBehaviour
{
    [SerializeField] private int gridX;
    [SerializeField] private int gridY;
    [SerializeField] private bool isCube;

    public int GetGridX() => gridX;
    public int GetGridY() => gridY;
    public bool IsCube() => isCube;

    public void SetGridX(int x) => gridX = x;
    public void SetGridY(int y) => gridY = y;
    public void SetIsCube(bool x) => isCube = x;
}
