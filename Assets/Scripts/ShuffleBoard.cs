using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShuffleBoard
{
    private int gridWidth;
    private int gridHeight;
    private float gridPositionXYOffset;
    private readonly int SHUFFLERETRYCOUNT = 100;
    public IEnumerator WaitAndShuffle()
    {
        gridWidth = GameGrid.Instance.GridWidth;
        gridHeight = GameGrid.Instance.GridHeight; 
        gridPositionXYOffset = GameGrid.Instance.GridPositionXYOffset;
        yield return new WaitForSeconds(1.5f);
        ResolveDeadlockAndShuffle(0);
    }

    private void ResolveDeadlockAndShuffle(int retryCount = 0)
    {
        ObjectColor randomXYColor = ObjectColor.Blue;

        if (retryCount > SHUFFLERETRYCOUNT) return;

        int randomX = Random.Range(0, gridWidth);
        int randomY = Random.Range(0, gridHeight);

        var entity = GameGrid.Instance.GridArray[randomX, randomY];
        if (entity == null || !entity.TryGetComponent(out Object objectComponent))
        {
            ResolveDeadlockAndShuffle(retryCount + 1);
            return;
        }

        randomXYColor = objectComponent.GetObjectColor();

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (x != randomX || y != randomY)
                {
                    entity = GameGrid.Instance.GridArray[x, y];
                    if (!entity)
                    {
                        continue;
                    }
                    var obj = entity.GetComponent<Object>();
                    if (obj != null && obj.GetObjectColor() == randomXYColor)
                    {
                        Vector2Int[] directions = new Vector2Int[]
                        {
                            new(0, 1),
                            new(0, -1),
                            new(1, 0),
                            new(-1, 0)
                        };

                        // Randomize directions
                        directions = directions.OrderBy(_ => Random.value).ToArray();

                        foreach (var dir in directions)
                        {
                            int newX = randomX + dir.x;
                            int newY = randomY + dir.y;

                            if (newX < 0 || newX >= gridWidth || newY < 0 || newY >= gridHeight)
                                continue;

                            var neighbour = GameGrid.Instance.GridArray[newX, newY];
                            if (neighbour == null)
                                continue;

                            if (neighbour.GetComponent<Object>() == null && neighbour.GetComponent<Powerups>() == null)
                                continue;

                            ReplacePieces(new List<Vector2Int> { new(randomX, randomY), new(x, y), new(newX, newY) });
                            RandomShuffle(randomX, randomY, x, y, newX, newY);
                            return;
                        }
                    }
                }
            }
        }
        ResolveDeadlockAndShuffle(retryCount + 1);
    }

    // Except for 3 cells in the guaranteed match process, all other cells are shuffled.
    public void RandomShuffle(int matchedX, int matchedY, int swappedFirstX, int swappedFirstY, int swappedSecondX, int swappedSecondY)
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                var entity = GameGrid.Instance.GridArray[x, y];

                if(entity == null || (!entity.GetComponent<Object>() && !entity.GetComponent<Powerups>()) )
                    continue;

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
        GameObject[] movingItems = positions.Select(pos => GameGrid.Instance.GridArray[pos.x, pos.y]).ToArray();

        for (int i = 0; i < count - 1; i++)
        {
            GameGrid.Instance.GridArray[positions[i].x, positions[i].y] = movingItems[i + 1];
        }
        GameGrid.Instance.GridArray[positions[count - 1].x, positions[count - 1].y] = movingItems[0];

        Vector3[] targetPositions = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            targetPositions[i] = new Vector3(positions[(i + (count - 1)) % count].x * gridPositionXYOffset, positions[(i + (count - 1)) % count].y * gridPositionXYOffset, 0);
        }

        AnimationManager.Instance.SwapObjectsAnim(movingItems, targetPositions);

        foreach (var pos in positions)
        {
            var entity = GameGrid.Instance.GridArray[pos.x, pos.y].GetComponent<GridEntity>();
            entity.SetGridX(pos.x);
            entity.SetGridY(pos.y);
            GameGrid.Instance.GridArray[pos.x, pos.y].GetComponent<SpriteRenderer>().sortingOrder = pos.y + 2;
        }
    }
}
