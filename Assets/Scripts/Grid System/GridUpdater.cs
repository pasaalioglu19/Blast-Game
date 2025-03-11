using System.Collections;
using UnityEngine;

public class GridUpdater {
    private int scannedColumnCount = 0;
    private float objectDropDelay;
    private float objectDropDuration = 0;
    public void UpdateGridAfterBlast(GridManager gridManager)
    {
        objectDropDelay = 0.1f;
        objectDropDuration  = AnimationManager.Instance.GetObjectDropDuration();
        scannedColumnCount = 0; // To calculate the gridUpdating process when it is exactly completed
        for (int x = 0; x < GameGrid.Instance.GridWidth; x++)
        {
            CoroutineHelper.Instance.StartRoutine(UpdateHelper(x));
        }
        CoroutineHelper.Instance.StartRoutine(WaitForGridUpdateToFinish(gridManager));
    }

    private IEnumerator UpdateHelper(int x)
    {
        int gridHeight = GameGrid.Instance.GridHeight;
        for (int y = 0; y < gridHeight; y++)
        {
            if (GameGrid.Instance.GridArray[x, y] == null)
            {
                for (int aboveY = y + 1; aboveY < gridHeight; aboveY++)
                {
                    var aboveItem = GameGrid.Instance.GridArray[x, aboveY];
                    if (aboveItem == null) continue;

                    Obstacle obstacle = aboveItem.GetComponent<Obstacle>(); 
                    if (obstacle != null && !obstacle.CanDrop)
                    {
                        break; 
                    }

                    MoveGridItem(x, y, aboveY);
                    break;
                }
            }
        }

        int lowestY = gridHeight;
        for (int y = gridHeight - 1; y >= 0; y--)
        {
            if (GameGrid.Instance.GridArray[x, y] == null)
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
            ObjectFactory.Instance.InstantiateRandomObject(x, y);
            float maxDropDelayCandidate = (gridHeight + 1.5f - y) * objectDropDuration;
            if (maxDropDelayCandidate > objectDropDelay)
            {
                objectDropDelay = maxDropDelayCandidate;
            }

            yield return new WaitForSeconds(0.12f);
        }
        scannedColumnCount++;
    }

    private IEnumerator WaitForGridUpdateToFinish(GridManager gridManager)
    {
        while (scannedColumnCount < GameGrid.Instance.GridWidth-1)
        {
            yield return new WaitForSeconds(0.01f);
        }
        yield return new WaitForSeconds(objectDropDelay);

        gridManager.UpdateFinished();
    }

    private void MoveGridItem(int x, int y, int aboveY)
    {
        GameObject movingItem = GameGrid.Instance.GridArray[x, aboveY];
        float gridPositionXYOffset = GameGrid.Instance.GridPositionXYOffset;

        GameGrid.Instance.GridArray[x, y] = movingItem;
        GameGrid.Instance.GridArray[x, aboveY] = null;
        Vector3 targetPosition = new(x * gridPositionXYOffset, y * gridPositionXYOffset, 0);
        AnimationManager.Instance.DropObjectAnim(movingItem, targetPosition, aboveY - y);
        movingItem.GetComponent<GridEntity>().SetGridY(y);
        movingItem.GetComponent<SpriteRenderer>().sortingOrder = y + 2;
    }

}
