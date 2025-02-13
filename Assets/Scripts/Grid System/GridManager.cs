using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening.Core.Easing;
using UnityEngine;
using static GridObject;
using static GridPowerUps;

public class GridManager : MonoBehaviour
{
    private IBlastHandler blastHandler;
    private IJrynothHandler jrynothHandler;

    private CubeSpriteOrganizer cubeSpriteOrganizer;
    private AnimationManager animationManager;


    private readonly int SHUFFLERETRYCOUNT = 100;
    private int gridHeight = 6;
    private int gridWidth = 6;
    private int scannedColumnCount = 0;

    private List<int> colorsSelected = new ();
    private Dictionary<int, List<Vector2Int>> cubeGroups = new Dictionary<int, List<Vector2Int>>();

    private bool isGridUpdating = false;
    private readonly bool gameOver = false;

    public GridObjectData GridObjectData;
    public GridPowerUpsData GridPowerUpsData;

    private const int backgroundWidthMultiplier = 108;
    private const int backgroundHeigthMultiplier = 110;
    private const float xyoffSet = 0.5f;
    private float objectDropDelay = 0.1f;
    private const float powerUpSequenceDelay = 0.15f;
    private const int leastGroupCount = 2;
    private int lastDefaultIconIndex = 0;

    public GameObject GridBackground;

    private GameObject[,] gridArray;

    private void Start()
    {
        blastHandler = new BlastHandler();
        jrynothHandler = new JrynothHandler();
        animationManager = FindFirstObjectByType<AnimationManager>();
    }

    public void InitializeGridWithLevelData(int gridHeight, int gridWidth, int colorsCount, int lastDefaultIconIndex, int lastFirstIconIndex, int lastSecondIconIndex)
    {
        if(gridHeight > 10 || gridWidth > 10 || colorsCount > 6)
        {
            string warningMessage = "Invalid input: ";

            if (gridHeight > 10)
                warningMessage += $"gridHeight ({gridHeight}) exceeds the limit. Maximum allowed is 10. ";

            if (gridWidth > 10)
                warningMessage += $"gridWidth ({gridWidth}) exceeds the limit. Maximum allowed is 10. ";

            if (colorsCount > 6)
                warningMessage += $"colorsCount ({colorsCount}) exceeds the limit. Maximum allowed is 6. ";

            Debug.LogWarning(warningMessage);
            return;
        }

        this.gridHeight = gridHeight;
        this.gridWidth = gridWidth;
        this.lastDefaultIconIndex = lastDefaultIconIndex; 

        gridArray = new GameObject[gridWidth, gridHeight];
        SelectColor(colorsCount);

        int totalObjectCount = GridObjectData.ObjectPrefabs.Length;
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                int selectedColorIndex = colorsSelected[Random.Range(0, colorsSelected.Count)];
                GameObject newObject = Instantiate(GridObjectData.ObjectPrefabs[selectedColorIndex], new Vector3(x * xyoffSet, y * xyoffSet, 0), Quaternion.identity);
                SetupGridObject(newObject, x, y, selectedColorIndex);
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
        blastHandler.FindGroups(gridArray, gridWidth, gridHeight, cubeGroups, leastGroupCount);
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

    private void SetupGridObject(GameObject newEntity, int x, int y, int selectedColorIndex)
    {
        GridObject gridObject = newEntity.GetComponent<GridObject>();
        gridObject.Initialize(x, y, selectedColorIndex);
        gridArray[x, y] = newEntity;
    }

    private void SetupGridPowerUps(GameObject newEntity, int x, int y)
    {
        GridPowerUps gridPowerUps = newEntity.GetComponent<GridPowerUps>();
        gridPowerUps.Initialize(x, y);
        gridArray[x, y] = newEntity;

        if (gridPowerUps.TryGetComponent<Rocket>(out var rocketComponent))
        {
            bool isHorizontal = Random.value > 0.5f; 
            rocketComponent.IsHorizontal(isHorizontal);
        }
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

        int randomX = Random.Range(0, gridWidth);
        int randomY = Random.Range(0, gridHeight);
        ObjectColor randomXYColor = gridArray[randomX, randomY].GetComponent<GridObject>().GetObjectColor();

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
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

                        if (newX < 0 || newX == gridWidth || newY < 0 || newY == gridHeight)
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

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
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

        animationManager.SwapObjectsAnim(movingItems, targetPositions);

        foreach (var pos in positions)
        {
            var entity = gridArray[pos.x, pos.y].GetComponent<GridEntity>();
            entity.SetGridX(pos.x);
            entity.SetGridY(pos.y);
            gridArray[pos.x, pos.y].GetComponent<SpriteRenderer>().sortingOrder = pos.y + 2;
        }
    }

    //_________________________________________________________________________________________________________________________


    public void CheckBlast(GridEntity touchedObject)
    {
        if (isGridUpdating || gameOver)
        {
            return;
        }

        if (touchedObject.IsCube()){
            CubeBlast(touchedObject, false);
            return;
        }

        GridPowerUps powerUpComponent = touchedObject.GetComponent<GridPowerUps>();
        if (powerUpComponent != null)
        {
            if (powerUpComponent.GetPowerUpType() == PowerUpType.Jrynoth)
            {
                jrynothHandler.HandleJrynothBlast(cubeSpriteOrganizer, animationManager, this, gridArray, cubeGroups, lastDefaultIconIndex, gridWidth, gridHeight, xyoffSet);
                StartCoroutine(WaitForDestroyAnimToFinish(animationManager.GetSpecialDestroyAnimDuration(), 0, 0, 0));
            }
            else
            {
                StartCoroutine(ActivateBlast(powerUpComponent));
            }
        }

    }
    private void CubeBlast(GridEntity touchedObject, bool isJrynoth)
    {
        int touchedIndexX = touchedObject.GetGridX();
        int touchedIndexY = touchedObject.GetGridY();
        float touchedObjectX = touchedObject.transform.position.x;
        float touchedObjectY = touchedObject.transform.position.y;
        int x = -1;
        int y = -1;
        foreach (var group in cubeGroups)
        {
            if (group.Value.Contains(new Vector2Int(touchedIndexX, touchedIndexY)))
            {
                foreach (var position in group.Value)
                {
                    x = position.x;
                    y = position.y;
                    GameObject objectInGroup = gridArray[x, y];
                    gridArray[x, y] = null;
                    animationManager.DestroyObjectAnim(objectInGroup, objectInGroup.GetComponent<GridObject>().GetObjectColor(), objectInGroup.GetComponent<GridObject>().IsDefaultSprite(), touchedObjectX, touchedObjectY);
                }
                StartCoroutine(WaitForDestroyAnimToFinish(animationManager.GetSpecialDestroyAnimDuration(), touchedIndexX, touchedIndexY, group.Value.Count));
                group.Value.Clear();
                return;
            }
        }
    }

    private IEnumerator WaitForDestroyAnimToFinish(float delay, int touchedIndexX, int touchedIndexY, int groupCount)
    {
        isGridUpdating = true;
        yield return new WaitForSeconds(delay);

        if (groupCount != 0)
        {
            int spriteIndex = cubeSpriteOrganizer.GetSpriteIndex(groupCount);
            if (spriteIndex != -1)
            {
                GameObject newPowerUp = Instantiate(GridPowerUpsData.PowerUpsPrefabs[spriteIndex], new Vector3(touchedIndexX * xyoffSet, touchedIndexY * xyoffSet, 0), Quaternion.identity);
                SetupGridPowerUps(newPowerUp, touchedIndexX, touchedIndexY);
            }
        }
        BringDefaultObjectSprites();
        UpdateGridAfterBlast(); 
    }

    public void GenerateAllPowerUps(int touchedIndexX, int touchedIndexY, int groupCount)
    {
        isGridUpdating = true;
        int spriteIndex = cubeSpriteOrganizer.GetSpriteIndex(groupCount);
        if (spriteIndex != -1)
        {
            GameObject newPowerUp = Instantiate(GridPowerUpsData.PowerUpsPrefabs[spriteIndex], new Vector3(touchedIndexX * xyoffSet, touchedIndexY * xyoffSet, 0), Quaternion.identity);
            SetupGridPowerUps(newPowerUp, touchedIndexX, touchedIndexY);
        }
    }

    private IEnumerator ActivateBlast(GridPowerUps touchedPowerUp)
    {
        isGridUpdating = true;
        yield return StartCoroutine(blastHandler.PowerUpSequenceController(touchedPowerUp, gridArray, gridWidth, gridHeight, animationManager, powerUpSequenceDelay));
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
        GameObject newObject = Instantiate(GridObjectData.ObjectPrefabs[selectedColorIndex], new Vector3(x * xyoffSet, (gridHeight+1.5f) * xyoffSet, 0), Quaternion.identity);
        SetupGridObject(newObject, x, y, selectedColorIndex);
        animationManager.DropObjectAnim(newObject, new Vector3(x * xyoffSet, y * xyoffSet, 0), gridHeight+1.5f - y);
    }

    //_________________________________________________________________________________________________________________________

    private void UpdateGridAfterBlast()
    {
        scannedColumnCount = 0; // To calculate the gridUpdating process when it is exactly completed
        for (int x = 0; x < gridWidth; x++)
        {
            StartCoroutine(UpdateHelper(x));
        }
        StartCoroutine(WaitForGridUpdateToFinish());
    }

    private IEnumerator UpdateHelper(int x)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            if (gridArray[x, y] == null)
            {
                for (int aboveY = y + 1; aboveY < gridHeight; aboveY++)
                {
                    if (gridArray[x, aboveY] == null) continue;
                    MoveGridItem(x, y, aboveY);
                    break;
                }
            }
        }

        int lowestY = gridHeight;
        for (int y = gridHeight - 1; y >= 0; y--)
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

        for (int y = lowestY; y < gridHeight; y++)
        {
            InstantiateRandomObject(x, y);
            float maxDropDelayCandidate = (gridHeight + 1.5f - y) * animationManager.GetObjectDropDuration();
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
        while (scannedColumnCount < gridWidth)
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
        animationManager.DropObjectAnim(movingItem, targetPosition, aboveY - y);
        movingItem.GetComponent<GridEntity>().SetGridY(y);
        movingItem.GetComponent<SpriteRenderer>().sortingOrder = y + 2;
    }

    //_________________________________________________________________________________________________________________________

    private void AdjustBackground()
    {
        GridBackground.SetActive(true);
        RectTransform gridRectTransform = GridBackground.GetComponent<RectTransform>();
        gridRectTransform.sizeDelta = new Vector2((backgroundWidthMultiplier - gridWidth) * gridWidth, (backgroundHeigthMultiplier - gridHeight)* gridHeight);
    }
    private void AdjustCamera()
    {
        Camera camera = Camera.main;

        camera.transform.position = new Vector3((gridWidth - 1) * xyoffSet / 2, (gridHeight + 4.6F) * xyoffSet / 2, -10);
    }
}
