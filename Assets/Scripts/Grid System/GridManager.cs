using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UIElements;

public class GridManager : MonoBehaviour
{
    private IBlastHandler blastHandler;
    private CubeSpriteOrganizer cubeSpriteOrganizer;
    private AnimationManager animationManager;

    private int gridRows = 6;
    private int gridColumns = 6;
    private int scannedColumnCount = 0;

    private List<int> colorsSelected = new ();
    private Dictionary<int, List<Vector2Int>> cubeGroups = new Dictionary<int, List<Vector2Int>>();

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
        animationManager = FindFirstObjectByType<AnimationManager>();
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
                int selectedColorIndex = colorsSelected[Random.Range(0, colorsSelected.Count)];
                GameObject newObject = Instantiate(GridObjectData.ObjectPrefabs[selectedColorIndex], new Vector3(x * xyoffSet, y * xyoffSet, 0), Quaternion.identity);
                SetupGridEntity(newObject, x, y, selectedColorIndex);
            }
        }

        cubeSpriteOrganizer = new CubeSpriteOrganizer(GridObjectData, lastDefaultIconIndex, lastFirstIconIndex, lastSecondIconIndex);
        AdjustCamera();
        AdjustBackground();
        StartCoroutine(HintCoroutine());
    }


    //When trying to keep the default sprites, I encountered an issue where the sprites changed after the hint due to concurrency, so I added a delay to prevent this.
    private IEnumerator HintCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        CheckHint();
    }

    private void CheckHint()
    {
        cubeGroups.Clear();
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

    //_________________________________________________________________________________________________________________________

    public void CheckBlast(GridObject touchedObject)
    {
        if (isGridUpdating || gameOver)
        {
            return;
        }

        foreach (var group in cubeGroups)
        {
            if (group.Value.Contains(new Vector2Int(touchedObject.GetGridX(), touchedObject.GetGridY())))
            {
                foreach (var position in group.Value)
                {
                    int x = position.x;
                    int y = position.y;
                    GameObject objectInGroup = gridArray[x, y];
                    Destroy(objectInGroup);
                    gridArray[x, y] = null;
                }

                group.Value.Clear();
                break;
            }
        }

        BringDefaultObjectSprites();
        UpdateGridAfterBlast();
    }

    private void BringDefaultObjectSprites()
    {
        foreach (var group in cubeGroups)
        {
            foreach (var position in group.Value)
            {
                int x = position.x;
                int y = position.y;
                GameObject objectInGroup = gridArray[x, y];
                objectInGroup.GetComponent<GridObject>().ResetObjectSprites();
            }
        }
    }

    //_________________________________________________________________________________________________________________________

    private void InstantiateRandomObject(int x, int y)
    {
        int selectedColorIndex = colorsSelected[Random.Range(0, colorsSelected.Count)];
        GameObject newObject = Instantiate(GridObjectData.ObjectPrefabs[selectedColorIndex], new Vector3(x * xyoffSet, gridRows * xyoffSet, 0), Quaternion.identity);
        SetupGridEntity(newObject, x, y, selectedColorIndex);
        animationManager.DropObject(newObject, new Vector3(x * xyoffSet, y * xyoffSet, 0), gridRows - y);
    }

    private void UpdateGridAfterBlast()
    {
        scannedColumnCount = 0; // To calculate the gridUpdating process when it is exactly completed
        isGridUpdating = true;
        for (int x = 0; x < gridColumns; x++)
        {
            StartCoroutine(UpdateHelper(x));
        }
        StartCoroutine(WaitForGridUpdateToFinish());
    }

    private IEnumerator UpdateHelper(int x)
    {
        for (int y = 0; y < gridRows; y++)
        {
            Debug.Log("[x,y] is : [" + x + "," + y + "] and gridArray[x,y] is : " + gridArray[x, y]);

            if (gridArray[x, y] == null)
            {
                for (int aboveY = y + 1; aboveY < gridRows; aboveY++)
                {
                    if (gridArray[x, aboveY] == null) continue;
                    MoveGridItem(x, y, aboveY);
                    yield return new WaitForSeconds(0.1f);
                    break;
                }
            }
        }

        int lowestY = gridRows;
        for (int y = gridRows - 1; y >= 0; y--)
        {
            if (gridArray[x, y] == null)
            {
                lowestY = y;
            }
            else
            {
                break;
            }
        }

        for (int y = lowestY; y < gridRows; y++)
        {
            InstantiateRandomObject(x, y);
            yield return new WaitForSeconds(0.1f);
        }
        scannedColumnCount++;
    }

    private IEnumerator WaitForGridUpdateToFinish()
    {
        while (scannedColumnCount < gridColumns)
        {
            yield return new WaitForSeconds(0.1f);
        }
        isGridUpdating = false;
        CheckHint();
    }

    private void MoveGridItem(int x, int y, int aboveY)
    {
        GameObject movingItem = gridArray[x, aboveY];

        gridArray[x, y] = movingItem;
        gridArray[x, aboveY] = null;
        Vector3 targetPosition = new(x * xyoffSet, y * xyoffSet, 0);
        animationManager.DropObject(movingItem, targetPosition, aboveY - y);
        movingItem.GetComponent<GridEntity>().SetGridY(y);
        movingItem.GetComponent<SpriteRenderer>().sortingOrder = y + 2;
    }

    //_________________________________________________________________________________________________________________________

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
