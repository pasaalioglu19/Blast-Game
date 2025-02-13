using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GridObject;
using static GridPowerUps;

public interface IBlastHandler
{
    void FindGroups(GameObject[,] gridArray, int gridColumns, int gridRows, Dictionary<int, List<Vector2Int>> cubeGroups, int leastGroupCount);
    IEnumerator PowerUpSequenceController(GridPowerUps touchedObject, GameObject[,] gridArray, int gridWidth, int gridHeight, AnimationManager animationManager, float tntSequenceDelay);
}

public class BlastHandler : IBlastHandler
{
    public void FindGroups(GameObject[,] gridArray, int gridColumns, int gridRows, Dictionary<int, List<Vector2Int>> cubeGroups, int leastGroupCount)
    {
        bool[,] visited = new bool[gridColumns, gridRows];

        int groupId = 0;

        for (int x = 0; x < gridColumns; x++)
        {
            for (int y = 0; y < gridRows; y++)
            {
                if (!visited[x, y] && gridArray[x, y] != null && gridArray[x, y].GetComponent<GridEntity>().IsCube())
                {
                    List<Vector2Int> group = new List<Vector2Int>();
                    ObjectColor color = gridArray[x, y].GetComponent<GridObject>().GetObjectColor();
                    DFS(gridArray, gridColumns, gridRows, x, y, visited, group, color);

                    if (group.Count >= leastGroupCount) 
                    {
                        cubeGroups[groupId] = group;
                        groupId++;
                    }
                }
            }
        }
    }

    private void DFS(GameObject[,] gridArray, int gridColumns, int gridRows, int startX, int startY, bool[,] visited, List<Vector2Int> group, ObjectColor color)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(new Vector2Int(startX, startY));

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Pop();
            int x = current.x, y = current.y;

            if (x < 0 || x >= gridColumns || y < 0 || y >= gridRows || visited[x, y] || gridArray[x, y] == null)
                continue;
            
            if(!gridArray[x, y].GetComponent<GridEntity>().IsCube())
                continue;

            if (gridArray[x, y].GetComponent<GridObject>().GetObjectColor() != color)
                continue;

            visited[x, y] = true;
            group.Add(new Vector2Int(x, y));

            stack.Push(new Vector2Int(x + 1, y));
            stack.Push(new Vector2Int(x - 1, y));
            stack.Push(new Vector2Int(x, y + 1));
            stack.Push(new Vector2Int(x, y - 1));
        }
    }

    // _________________________________________________________________________________________________________________________________________________________


    // Manages the situation where TNTs explode each other
    public IEnumerator PowerUpSequenceController(GridPowerUps touchedObject, GameObject[,] gridArray, int gridWidth, int gridHeight, AnimationManager animationManager, float powerUpSequenceDelay)
    {
        List<GridEntity> matchedObjects;
        Queue<GridPowerUps> matchedPowerUps = new(); 

        matchedPowerUps.Enqueue(touchedObject); 

        bool[,] visitedPowerups = new bool[gridWidth, gridHeight];
        visitedPowerups[touchedObject.GetGridX(), touchedObject.GetGridY()] = true;

        do
        {
            Debug.Log(matchedPowerUps.Count);
            var currentPowerUp = matchedPowerUps.Dequeue();
            int currentX = currentPowerUp.GetGridX();
            int currentY = currentPowerUp.GetGridY();

            PowerUpType currentPowerUpType = currentPowerUp.GetPowerUpType();

            if (currentPowerUpType == PowerUpType.Tnt)
            {
                int isBigTnt = CheckAdjacentTnt(currentX, currentY, visitedPowerups, gridArray, gridWidth, gridHeight);
                matchedObjects = TntBlast(gridArray, gridWidth, gridHeight, currentX, currentY, isBigTnt);
                //animationManager.TntAnimation(currentTnt.transform.position, isBigTnt);
            }
            else if (currentPowerUpType == PowerUpType.Rocket)
            {
                Rocket rocketComponent = currentPowerUp.GetComponent<Rocket>();
                matchedObjects = RocketBlast(gridArray, gridWidth, gridHeight, currentX, currentY, rocketComponent.IsHorizontal());
                animationManager.RocketBlastAnim(currentPowerUp.transform.position.x, currentPowerUp.transform.position.y, !rocketComponent.IsHorizontal(), gridHeight + 2, rocketComponent.GetRocketHalfSprite());
            }
            else
            {
                Debug.Log(currentPowerUpType);
                continue;
            }

            for (int i = 0; i < matchedObjects.Count; i++)
            {
                if (matchedObjects[i].IsCube())
                {
                    break;
                }
                GridPowerUps powerUp = matchedObjects[i].GetComponent<GridPowerUps>();
                PowerUpType powerUpType = powerUp.GetPowerUpType();
                int powerUpX = powerUp.GetGridX();
                int powerUpY = powerUp.GetGridY();

                if (!visitedPowerups[powerUpX, powerUpY])
                {
                    matchedPowerUps.Enqueue(powerUp);
                    visitedPowerups[powerUpX, powerUpY] = true;
                }
            }

            foreach (var powerup in matchedPowerUps)
            {
                matchedObjects.Remove(powerup);
            }

            matchedObjects.Add(currentPowerUp);

            DestroyMatchedCube(matchedObjects, gridArray, animationManager);
            yield return new WaitForSeconds(powerUpSequenceDelay);

        } while (matchedPowerUps.Count > 0);
    }

    private List<GridEntity> TntBlast(GameObject[,] gridArray, int gridWidth, int gridHeight, int x, int y, int isBigTnt)
    {
        List<GridEntity> matchedObjects = new();
        int xyStart = 2 + isBigTnt;
        int xyEnd = 3 + isBigTnt;

        for (int x_value = x - xyStart; x_value < x + xyEnd; x_value++)
        {
            if (x_value < 0 || x_value >= gridWidth)
            {
                continue;
            }
            for (int y_value = y - xyStart; y_value < y + xyEnd; y_value++)
            {
                if (y_value < 0 || y_value >= gridHeight || gridArray[x_value, y_value] == null)
                {
                    continue;
                }

                if (gridArray[x_value, y_value].GetComponent<GridEntity>().IsCube())
                {
                    matchedObjects.Add(gridArray[x_value, y_value].GetComponent<GridEntity>());
                }
                else if (gridArray[x_value, y_value].GetComponent<GridPowerUps>() && (x_value != x || y_value != y))
                {
                    if(gridArray[x_value, y_value].GetComponent<GridPowerUps>().GetPowerUpType() != PowerUpType.Jrynoth)
                    {
                        matchedObjects.Insert(0, gridArray[x_value, y_value].GetComponent<GridEntity>()); //PowerUps are kept specially by adding them to the begin of the list to be detonated later.
                    }
                }
            }
        }
        return matchedObjects;
    }

    private int CheckAdjacentTnt(int x, int y, bool[,] visitedTnt, GameObject[,] gridArray, int gridWidth, int gridHeight)
    {
        Vector2Int[] directions = { new(1, 0), new(0, 1), new(-1, 0), new(0, -1) };

        foreach (var dir in directions)
        {
            int newX = x + dir.x;
            int newY = y + dir.y;


            if (newX < 0 || newX >= gridWidth || newY < 0 || newY >= gridHeight || !gridArray[newX, newY])
            {
                continue;
            }

            GridPowerUps powerUpComponent = gridArray[newX, newY].GetComponent<GridPowerUps>();

            if (powerUpComponent && powerUpComponent.GetPowerUpType() == PowerUpType.Tnt)
            {
                visitedTnt[newX, newY] = true;
                return 1;
            }
        }
        return 0;
    }

    private List<GridEntity> RocketBlast(GameObject[,] gridArray, int gridWidth, int gridHeight, int rocketX, int rocketY, bool isHorizontal)
    {
        List<GridEntity> matchedObjects = new();

        if (isHorizontal)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (gridArray[x, rocketY] == null)
                {
                    continue;
                }
                if (gridArray[x, rocketY].GetComponent<GridEntity>().IsCube())
                {
                    matchedObjects.Add(gridArray[x, rocketY].GetComponent<GridEntity>());
                }
                else if (gridArray[x, rocketY].GetComponent<GridPowerUps>() && (x != rocketX))
                {
                    if (gridArray[x, rocketY].GetComponent<GridPowerUps>().GetPowerUpType() != PowerUpType.Jrynoth)
                    {
                        matchedObjects.Insert(0, gridArray[x, rocketY].GetComponent<GridEntity>()); //PowerUps are kept specially by adding them to the begin of the list to be detonated later.
                    }
                }
            }
        }
        else
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (gridArray[rocketX, y] == null)
                {
                    continue;
                }
                if (gridArray[rocketX, y].GetComponent<GridEntity>().IsCube())
                {
                    matchedObjects.Add(gridArray[rocketX, y].GetComponent<GridEntity>());
                }
                else if (gridArray[rocketX, y].GetComponent<GridPowerUps>() && (y != rocketY))
                {
                    if (gridArray[rocketX, y].GetComponent<GridPowerUps>().GetPowerUpType() != PowerUpType.Jrynoth)
                    {
                        matchedObjects.Insert(0, gridArray[rocketX, y].GetComponent<GridEntity>()); //PowerUps are kept specially by adding them to the begin of the list to be detonated later.
                    }
                }
            }
        }
        return matchedObjects;
    }

    private void DestroyMatchedCube(List<GridEntity> matchedObjects, GameObject[,] gridArray, AnimationManager animationManager)
    {
        foreach (var gridObj in matchedObjects)
        {
            if (gridObj != null)
            {
                gridArray[gridObj.GetGridX(), gridObj.GetGridY()] = null;
                if (gridObj.IsCube())
                {
                    animationManager.DestroyObjectAnim(gridObj.gameObject, gridObj.GetComponent<GridObject>().GetObjectColor(), true, 0, 0);
                    continue;
                }
                animationManager.TntBlastAnim(gridObj.gameObject);
            }
        }
    }
}
