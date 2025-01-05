using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using static GridObject;

public class GridManager : MonoBehaviour
{
    private IBlastHandler blastHandler;
    private CubeSpriteOrganizer cubeSpriteOrganizer;

    private int gridRows = 6;
    private int gridColumns = 6;
    private List<int> colorsSelected = new ();

    private int lastDefaultIconIndex = 6;
    private int lastFirstIconIndex = 6;
    private int lastSecondIconIndex = 6;

    private bool isGridUpdating = false;
    private bool gameOver = false;
    private List<GridObject> tntHintObjects = new();

    public GridObjectData GridObjectData;

    private const int backgroundWidthMultiplier = 102;
    private const int backgroundHeigthMultiplier = 104;
    private const float xyoffSet = 0.5f;
    public GameObject GridBackground;

    private GameObject[,] gridArray;

    private void Start()
    {
        blastHandler = new BlastHandler();
    }

    public void InitializeGridWithLevelData(int gridRows, int gridColumns, int colorsCount, int lastDefaultIconIndex, int lastFirstIconIndex, int lastSecondIconIndex)
    {
        this.gridRows = gridRows;
        this.gridColumns = gridColumns;

        this.lastDefaultIconIndex = lastDefaultIconIndex;
        this.lastFirstIconIndex = lastFirstIconIndex;
        this.lastSecondIconIndex = lastSecondIconIndex;

        gridArray = new GameObject[gridColumns, gridRows];
        SelectColor(colorsCount);

        int totalObjectCount = GridObjectData.ObjectPrefabs.Length;
        for (int y = 0; y < gridRows; y++)
        {
            for (int x = 0; x < gridColumns; x++)
            {
                GameObject newObject = null;
                int selectedColorIndex = colorsSelected[Random.Range(0, colorsSelected.Count)];
                newObject = Instantiate(GridObjectData.ObjectPrefabs[selectedColorIndex], new Vector3(x * xyoffSet, y * xyoffSet, 0), Quaternion.identity);
                SetupGridEntity(newObject, x, y, selectedColorIndex);
            }
        }

        cubeSpriteOrganizer = new CubeSpriteOrganizer(GridObjectData, lastDefaultIconIndex, lastFirstIconIndex, lastSecondIconIndex);
        AdjustCamera();
        AdjustBackground();
        StartCoroutine(HintCoroutine());
    }

    private IEnumerator HintCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        CheckHint();
    }

    private void CheckHint()
    {
        Dictionary<int, List<Vector2Int>> cubeGroups = new Dictionary<int, List<Vector2Int>>();
        blastHandler.FindGroups(gridArray, gridColumns, gridRows, cubeGroups);
        cubeSpriteOrganizer.OrganizeCubeSprites(cubeGroups, gridArray);
    }

    private void SelectColor(int colorsCount)
    {
        int totalObjectCount = GridObjectData.ObjectPrefabs.Length;
        List<int> numbers = new();

        for (int i = 0; i < totalObjectCount; i++)
        {
            numbers.Add(i);
        }

        for (int i = 0; i < colorsCount; i++)
        {
            int candidate = numbers[Random.Range(0, numbers.Count)];
            colorsSelected.Add(candidate);
            numbers.Remove(candidate);
        }
    }

    private void SetupGridEntity(GameObject newEntity, int x, int y, int selectedColorIndex)
    {
        GridEntity gridEntity = newEntity.GetComponent<GridEntity>();
        gridEntity.SetGridX(x);
        gridEntity.SetGridY(y);
        gridEntity.SetIsCube(true);
        newEntity.GetComponent<GridObject>().SetColor(selectedColorIndex);
        newEntity.GetComponent<SpriteRenderer>().sortingOrder = y + 2; //Render order layout was provided according to y coordinates
        gridArray[x, y] = newEntity;
    }

    private void AdjustBackground()
    {
        GridBackground.SetActive(true);
        RectTransform gridRectTransform = GridBackground.GetComponent<RectTransform>();
        gridRectTransform.sizeDelta = new Vector2(backgroundWidthMultiplier * gridColumns, backgroundHeigthMultiplier * gridRows);
    }
    private void AdjustCamera()
    {
        Camera camera = Camera.main;

        camera.transform.position = new Vector3((gridColumns - 1) * xyoffSet / 2, (gridRows + 4.6F) * xyoffSet / 2, -10);
    }

}
