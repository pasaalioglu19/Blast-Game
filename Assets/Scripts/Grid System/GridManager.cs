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


    private readonly int SHUFFLERETRYCOUNT = 100;
    private int gridRows = 6;
    private int gridColumns = 6;
    private int scannedColumnCount = 0;

    private List<int> colorsSelected = new ();
    private Dictionary<int, List<Vector2Int>> cubeGroups = new Dictionary<int, List<Vector2Int>>();

    private bool isGridUpdating = false;
    private readonly bool gameOver = false;

    public GridObjectData GridObjectData;

    private const int backgroundWidthMultiplier = 108;
    private const int backgroundHeigthMultiplier = 110;
    private const float xyoffSet = 0.5f;
    private float objectDropDelay = 0.1f;
    public GameObject GridBackground;

    private GameObject[,] gridArray;

    private void Start()
    {
        blastHandler = new BlastHandler();
        animationManager = FindFirstObjectByType<AnimationManager>();
    }

    public void InitializeGridWithLevelData(int gridRows, int gridColumns, int colorsCount, int lastDefaultIconIndex, int lastFirstIconIndex, int lastSecondIconIndex)
    {
        if(gridRows > 10 || gridColumns > 10 || colorsCount > 6)
        {
            string warningMessage = "Invalid input: ";

            if (gridRows > 10)
                warningMessage += $"gridRows ({gridRows}) exceeds the limit. Maximum allowed is 10. ";

            if (gridColumns > 10)
                warningMessage += $"gridColumns ({gridColumns}) exceeds the limit. Maximum allowed is 10. ";

            if (colorsCount > 6)
                warningMessage += $"colorsCount ({colorsCount}) exceeds the limit. Maximum allowed is 6. ";

            Debug.LogWarning(warningMessage);
            return;
        }

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


    // When trying to keep the default sprites, I encountered an issue where the sprites changed after the hint due to concurrency, so I added a delay to prevent this.
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
        newEntity.GetComponent<SpriteRenderer>().sortingOrder = y + 2; // Render order layout was provided according to y coordinates
        gridArray[x, y] = newEntity;
    }

    //_________________________________________________________________________________________________________________________

    private IEnumerator WaitAndShuffle()
    {
        yield return new WaitForSeconds(1.5f);
        ShuffleBoard(0);
    }

    private void ShuffleBoard(int retryCount = 0)
    {
        if (retryCount > SHUFFLERETRYCOUNT) return;

        int randomX = Random.Range(0, gridColumns);
        int randomY = Random.Range(0, gridRows);
        ObjectColor randomXYColor = gridArray[randomX, randomY].GetComponent<GridObject>().GetObjectColor();

        for (int y = 0; y < gridRows; y++)
        {
            for (int x = 0; x < gridColumns; x++)
            {
                if (x != randomX || y != randomY)
                {
                    if (gridArray[x, y].GetComponent<GridObject>().GetObjectColor() == randomXYColor)
                    {
                        Vector2Int[] directions = new Vector2Int[]
{
                        new(0, 1),
                        new(0, -1),
                        new(1, 0),
                        new(-1, 0)
};

                        Vector2Int randomDirection = directions[Random.Range(0, directions.Length)];

                        int newX = randomX + randomDirection.x;
                        int newY = randomY + randomDirection.y;

                        if (newX < 0 || newX == gridColumns || newY < 0 || newY == gridRows)
                        {
                            newX = randomX - randomDirection.x;
                            newY = randomY - randomDirection.y;
                        }

                        ReplacePieces(new List<Vector2Int> { new(randomX, randomY), new(x, y), new(newX, newY) });
                        RandomShuffle(randomX, randomY, x, y, newX, newY);
                        CheckHint();
                        return;
                    }
                }
            }
        }
        ShuffleBoard(retryCount + 1);
    }

    // Except for 3 cells in the guaranteed match process, all other cells are shuffled.
    public void RandomShuffle(int matchedX, int matchedY, int swappedFirstX, int swappedFirstY, int swappedSecondX, int swappedSecondY)
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        for (int y = 0; y < gridRows; y++)
        {
            for (int x = 0; x < gridColumns; x++)
            {
                if ((x == matchedX && y == matchedY) || (x == swappedFirstX && y == swappedFirstY) || (x == swappedSecondX && y == swappedSecondY))
                    continue;

                positions.Add(new Vector2Int(x, y));
            }
        }

        int halfCount = positions.Count / 2;
        for (int i = 0; i < halfCount; i++)
        {
            ReplacePieces(new List<Vector2Int> { positions[i], positions[positions.Count - 1 - i] });
        }
    }

    // Manages the swapping operations between three cells that resolve the deadlock issue and other cells.
    private void ReplacePieces(List<Vector2Int> positions)
    {
        int count = positions.Count;
        if (count < 2) return;

        // Assign Game Objects
        GameObject[] movingItems = positions.Select(pos => gridArray[pos.x, pos.y]).ToArray();

        for (int i = 0; i < count - 1; i++)
        {
            gridArray[positions[i].x, positions[i].y] = movingItems[i + 1];
        }
        gridArray[positions[count - 1].x, positions[count - 1].y] = movingItems[0];

        Vector3[] targetPositions = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            targetPositions[i] = new Vector3(positions[(i + (count - 1)) % count].x * xyoffSet, positions[(i + (count - 1)) % count].y * xyoffSet, 0);
        }

        animationManager.SwapObjects(movingItems, targetPositions);

        foreach (var pos in positions)
        {
            var entity = gridArray[pos.x, pos.y].GetComponent<GridEntity>();
            entity.SetGridX(pos.x);
            entity.SetGridY(pos.y);
            gridArray[pos.x, pos.y].GetComponent<SpriteRenderer>().sortingOrder = pos.y + 2;
        }
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
                    gridArray[x, y] = null;
                    float touchedObjectX = touchedObject.transform.position.x;
                    float touchedObjectY = touchedObject.transform.position.y;
                    animationManager.DestroyObject(objectInGroup, objectInGroup.GetComponent<GridObject>().GetObjectColor(), objectInGroup.GetComponent<GridObject>().IsDefaultSprite(), touchedObjectX, touchedObjectY);
                }
                group.Value.Clear();
                StartCoroutine(WaitForDestroyAnimToFinish(animationManager.GetSpecialDestroyAnimDuration()));
                break;
            }
        }
    }

    private IEnumerator WaitForDestroyAnimToFinish(float delay)
    {
        isGridUpdating = true;
        yield return new WaitForSeconds(delay);
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
                if (objectInGroup)
                {
                    objectInGroup.GetComponent<GridObject>().ResetObjectSprites();
                }
            }
        }
    }

    //_________________________________________________________________________________________________________________________

    private void InstantiateRandomObject(int x, int y)
    {
        int selectedColorIndex = colorsSelected[Random.Range(0, colorsSelected.Count)];
        GameObject newObject = Instantiate(GridObjectData.ObjectPrefabs[selectedColorIndex], new Vector3(x * xyoffSet, (gridRows+1.5f) * xyoffSet, 0), Quaternion.identity);
        SetupGridEntity(newObject, x, y, selectedColorIndex);
        animationManager.DropObject(newObject, new Vector3(x * xyoffSet, y * xyoffSet, 0), gridRows+1.5f - y);
    }

    private void UpdateGridAfterBlast()
    {
        scannedColumnCount = 0; // To calculate the gridUpdating process when it is exactly completed
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
            float maxDropDelayCandidate = (gridRows + 1.5f - y) * animationManager.GetObjectDropDuration();
            if (maxDropDelayCandidate > objectDropDelay)
            {
                objectDropDelay = maxDropDelayCandidate;
            }

            yield return new WaitForSeconds(0.12f);
        }
        scannedColumnCount++;
    }

    private IEnumerator WaitForGridUpdateToFinish()
    {
        while (scannedColumnCount < gridColumns)
        {
            yield return new WaitForSeconds(0.01f);
        }
        yield return new WaitForSeconds(objectDropDelay);
        isGridUpdating = false;
        objectDropDelay = 0.1f;
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
        gridRectTransform.sizeDelta = new Vector2((backgroundWidthMultiplier - gridColumns) * gridColumns, (backgroundHeigthMultiplier - gridRows)* gridRows);
    }
    private void AdjustCamera()
    {
        Camera camera = Camera.main;

        camera.transform.position = new Vector3((gridColumns - 1) * xyoffSet / 2, (gridRows + 4.6F) * xyoffSet / 2, -10);
    }
}
