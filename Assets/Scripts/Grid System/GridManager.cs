using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GridObject;

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

    private bool isGridUpdating = false;
    private bool gameOver = false;

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
        if (cubeGroups.Count == 0) 
        {
            StartCoroutine(WaitAndShuffle());
            return;
        }

        cubeSpriteOrganizer.OrganizeCubeSprites(cubeGroups, gridArray);
    }

    private IEnumerator WaitAndShuffle()
    {
        yield return new WaitForSeconds(1.5f);
        ShuffleBoard(); 
    }

    private void ShuffleBoard(int retryCount = 0)
    {
        if (retryCount > 10) return;

        int randomX = Random.Range(0, gridColumns);
        int randomY = Random.Range(0, gridRows);
        ObjectColor randomXYColor = gridArray[randomX, randomY].GetComponent<GridObject>().GetObjectColor();

        for (int y = 0; y < gridRows; y++)
        {
            for (int x = 0; x < gridColumns; x++)
            {
                if(x != randomX || y != randomY)
                {
                    if(gridArray[x, y].GetComponent<GridObject>().GetObjectColor() == randomXYColor)
                    {
                        Vector2Int[] directions = new Vector2Int[]
{
                        new(0, 1),  
                        new(0, -1), 
                        new(1, 0),  
                        new(-1, 0)   
};

                        // Select random direction
                        Vector2Int randomDirection = directions[Random.Range(0, directions.Length)];

                        int newX = randomX + randomDirection.x;
                        int newY = randomY + randomDirection.y;

                        if(newX < 0 || newX == gridColumns || newY < 0 || newY == gridRows)
                        {
                            newX = randomX - randomDirection.x;
                            newY = randomY - randomDirection.y;
                        }

                        ReplaceTwoPiece(x, y, newX, newY);
                        RandomShuffle(x, y, newX, newY);
                        CheckHint();
                        return;
                    }
                }
            }
        }
        ShuffleBoard(retryCount + 1);
    }

    public void RandomShuffle(int swappedFirstX, int swappedFirstY, int swappedSecondX, int swappedSecondY)
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        for (int y = 0; y < gridRows; y++)
        {
            for (int x = 0; x < gridColumns; x++)
            {
                if ((x == swappedFirstX && y == swappedFirstY) || (x == swappedSecondX && y == swappedSecondY))
                    continue; 

                positions.Add(new Vector2Int(x, y));
            }
        }

        int halfCount = positions.Count / 2;
        for (int i = 0; i < halfCount; i++)
        {
            Vector2Int pos1 = positions[i];
            Vector2Int pos2 = positions[positions.Count - 1 - i];

            ReplaceTwoPiece(pos1.x, pos1.y, pos2.x, pos2.y);
        }
    }


    private void ReplaceTwoPiece(int x, int y, int newX, int newY)
    {
        GameObject movingItem = gridArray[x, y];
        GameObject movingItem2 = gridArray[newX, newY];
        gridArray[x, y] = movingItem2;
        gridArray[newX, newY] = movingItem;

        Vector3 targetPosition1 = new(newX * xyoffSet, newY * xyoffSet, 0);
        Vector3 targetPosition2 = new(x * xyoffSet, y * xyoffSet, 0);

        animationManager.SwapObject(movingItem, movingItem2, targetPosition1, targetPosition2);

        gridArray[newX, newY].GetComponent<GridEntity>().SetGridX(newX);
        gridArray[newX, newY].GetComponent<GridEntity>().SetGridY(newY);
        gridArray[newX, newY].GetComponent<SpriteRenderer>().sortingOrder = newY + 2;

        gridArray[x, y].GetComponent<GridEntity>().SetGridX(x);
        gridArray[x, y].GetComponent<GridEntity>().SetGridY(y);
        gridArray[x, y].GetComponent<SpriteRenderer>().sortingOrder = y + 2;
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
                BringDefaultObjectSprites();
                UpdateGridAfterBlast();
                break;
            }
        }
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
