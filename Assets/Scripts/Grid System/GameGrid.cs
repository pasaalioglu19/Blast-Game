using UnityEngine;

public class GameGrid : MonoBehaviour
{
    private static GameGrid _instance;
    public static GameGrid Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("Grid");
                _instance = obj.AddComponent<GameGrid>();
            }
            return _instance;
        }
    }

    private float gridPositionXYOffset;
    private int gridWidth;
    private int gridHeight;
    private GameObject[,] gridArray;

    public float GridPositionXYOffset => gridPositionXYOffset;
    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public GameObject[,] GridArray => gridArray;

    public void InitializeGrid(GameObject[,] gridArray, int gridWidth, int gridHeight, float gridPositionXYOffset)
    {
        this.gridPositionXYOffset = gridPositionXYOffset;
        this.gridWidth = gridWidth;
        this.gridHeight = gridHeight;
        this.gridArray = gridArray;
    }
}
