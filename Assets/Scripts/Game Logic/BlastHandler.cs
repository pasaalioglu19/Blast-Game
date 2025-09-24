using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
public interface IBlastHandler
{
    void FindGroups(Dictionary<int, List<Vector2Int>> cubeGroups, int leastGroupCount);
    IEnumerator PowerupSequenceController(Powerups touchedObject, AnimationManager animationManager, float tntSequenceDelay);
}

public class BlastHandler : IBlastHandler
{
    public void FindGroups(Dictionary<int, List<Vector2Int>> cubeGroups, int leastGroupCount)
    {
        var gridArray = GameGrid.Instance.GridArray;
        int gridWidth = GameGrid.Instance.GridWidth;
        int gridHeight = GameGrid.Instance.GridHeight;

        bool[,] visited = new bool[gridWidth, gridHeight];

        int groupId = 0;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (!visited[x, y] && gridArray[x, y] != null && gridArray[x, y].GetComponent<GridEntity>().IsCube())
                {
                    List<Vector2Int> group = new List<Vector2Int>();
                    ObjectColor color = gridArray[x, y].GetComponent<Object>().GetObjectColor();
                    DFS(gridArray, gridWidth, gridHeight, x, y, visited, group, color);

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

            if (gridArray[x, y].GetComponent<Object>().GetObjectColor() != color)
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
    public IEnumerator PowerupSequenceController(Powerups touchedObject, AnimationManager animationManager, float powerupSequenceDelay)
    {
        var gridArray = GameGrid.Instance.GridArray;
        int gridWidth = GameGrid.Instance.GridWidth;
        int gridHeight = GameGrid.Instance.GridHeight;
        List<Powerups> matchedPowerups2;
        Queue<Powerups> matchedPowerups = new();

        matchedPowerups.Enqueue(touchedObject); 

        bool[,] visitedPowerups = new bool[gridWidth, gridHeight];
        visitedPowerups[touchedObject.GetGridX(), touchedObject.GetGridY()] = true;

        do
        {
            var currentPowerup = matchedPowerups.Dequeue();
            if(currentPowerup == null)
            {
                Debug.Log("sa");
                continue;
            }
            int currentX = currentPowerup.GetGridX();
            int currentY = currentPowerup.GetGridY();

            PowerupType currentPowerupType = currentPowerup.GetPowerupType();

            if (currentPowerupType == PowerupType.Tnt)
            {
                int isBigTnt = CheckAdjacentTnt(currentX, currentY, visitedPowerups, gridArray, gridWidth, gridHeight);
                matchedPowerups2 = TntBlast(gridArray, gridWidth, gridHeight, currentX, currentY, isBigTnt);
                //animationManager.TntAnimation(currentTnt.transform.position, isBigTnt);
                animationManager.TntBlastAnim(currentPowerup.gameObject);
            }
            else if (currentPowerupType == PowerupType.Rocket)
            {
                Rocket rocketComponent = currentPowerup.GetComponent<Rocket>();
                matchedPowerups2 = RocketBlast(gridArray, gridWidth, gridHeight, currentX, currentY, rocketComponent.IsHorizontal());
                animationManager.RocketBlastAnim(currentPowerup.gameObject, currentPowerup.transform.position.x, currentPowerup.transform.position.y, !rocketComponent.IsHorizontal(), gridHeight + 2, rocketComponent.GetRocketHalfSprite());
            }
            else
            {
                continue;
            }

            for (int i = 0; i < matchedPowerups2.Count; i++)
            {
                Powerups powerup = matchedPowerups2[i].GetComponent<Powerups>();
                PowerupType powerupType = powerup.GetPowerupType();
                int powerupX = powerup.GetGridX();
                int powerupY = powerup.GetGridY();

                if (!visitedPowerups[powerupX, powerupY])
                {
                    matchedPowerups.Enqueue(powerup);
                    visitedPowerups[powerupX, powerupY] = true;
                }
                else
                {
                    animationManager.TntBlastAnim(matchedPowerups2[i].gameObject);
                    gridArray[powerupX, powerupY] = null;
                }
            }


            gridArray[currentX, currentY] = null;

            yield return new WaitForSeconds(powerupSequenceDelay);

        } while (matchedPowerups.Count > 0);
    }

    private List<Powerups> TntBlast(GameObject[,] gridArray, int gridWidth, int gridHeight, int x, int y, int isBigTnt)
    {
        List<Powerups> matchedPowerups = new();
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
                if (y_value < 0 || y_value >= gridHeight || gridArray[x_value, y_value] == null || (x_value == x && y_value == y))
                {
                    continue;
                }

                if (gridArray[x_value, y_value].GetComponent<GridEntity>().IsCube())
                {
                    AnimationManager.Instance.DestroyObjectAnim(gridArray[x_value, y_value], gridArray[x_value, y_value].GetComponent<GridEntity>().GetComponent<Object>().GetObjectColor(), true, 0, 0);
                    gridArray[x_value, y_value] = null;
                }
                else if (gridArray[x_value, y_value].GetComponent<Powerups>())
                {
                    if(gridArray[x_value, y_value].GetComponent<Powerups>().GetPowerupType() != PowerupType.Jrynoth)
                    {
                        matchedPowerups.Insert(0, gridArray[x_value, y_value].GetComponent<Powerups>()); //Powerups are kept specially by adding them to the begin of the list to be detonated later.
                    }
                }
                else if (gridArray[x_value, y_value].GetComponent<Obstacle>())
                {
                    if (gridArray[x_value, y_value].TryGetComponent<Obstacle>(out var obstacle) )
                    {
                        obstacle.TakeHit(ExplosionType.TntBlast);
                    }
                }
            }
        }
        return matchedPowerups;
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

            Powerups powerupComponent = gridArray[newX, newY].GetComponent<Powerups>();

            if (powerupComponent && powerupComponent.GetPowerupType() == PowerupType.Tnt)
            {
                visitedTnt[newX, newY] = true;
                return 1;
            }
        }
        return 0;
    }

    private List<Powerups> RocketBlast(GameObject[,] gridArray, int gridWidth, int gridHeight, int rocketX, int rocketY, bool isHorizontal)
    {
        List<Powerups> matchedPowerups = new();

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
                    AnimationManager.Instance.DestroyObjectAnim(gridArray[x, rocketY], gridArray[x, rocketY].GetComponent<Object>().GetObjectColor(), true, 0, 0);
                    gridArray[x, rocketY] = null;
                }
                else if (gridArray[x, rocketY].GetComponent<Powerups>() && (x != rocketX))
                {
                    if (gridArray[x, rocketY].GetComponent<Powerups>().GetPowerupType() != PowerupType.Jrynoth)
                    {
                        matchedPowerups.Insert(0, gridArray[x, rocketY].GetComponent<Powerups>()); //Powerups are kept specially by adding them to the begin of the list to be detonated later.
                    }
                }
                else if (gridArray[x, rocketY].GetComponent<Obstacle>())
                {
                    if (gridArray[x, rocketY].TryGetComponent<Obstacle>(out var obstacle))
                    {
                        obstacle.TakeHit(ExplosionType.RocketBlast);
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
                    AnimationManager.Instance.DestroyObjectAnim(gridArray[rocketX, y], gridArray[rocketX, y].GetComponent<Object>().GetObjectColor(), true, 0, 0);
                    gridArray[rocketX, y] = null;
                }
                else if (gridArray[rocketX, y].GetComponent<Powerups>() && (y != rocketY))
                {
                    if (gridArray[rocketX, y].GetComponent<Powerups>().GetPowerupType() != PowerupType.Jrynoth)
                    {
                        matchedPowerups.Insert(0, gridArray[rocketX, y].GetComponent<Powerups>()); //Powerups are kept specially by adding them to the begin of the list to be detonated later.
                    }
                }
                else if (gridArray[rocketX, y].GetComponent<Obstacle>())
                {
                    if (gridArray[rocketX, y].TryGetComponent<Obstacle>(out var obstacle))
                    {
                        obstacle.TakeHit(ExplosionType.RocketBlast);
                    }
                }
            }
        }
        return matchedPowerups;
    }
}
