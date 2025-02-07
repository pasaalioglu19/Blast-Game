using System.Collections.Generic;
using UnityEngine;
using static GridObject;

public interface IBlastHandler
{
    void FindGroups(GameObject[,] gridArray, int gridColumns, int gridRows, Dictionary<int, List<Vector2Int>> cubeGroups);
}

public class BlastHandler : IBlastHandler
{
    public void FindGroups(GameObject[,] gridArray, int gridColumns, int gridRows, Dictionary<int, List<Vector2Int>> cubeGroups)
    {
        bool[,] visited = new bool[gridColumns, gridRows];

        int groupId = 0;

        for (int x = 0; x < gridColumns; x++)
        {
            for (int y = 0; y < gridRows; y++)
            {
                if (!visited[x, y] && gridArray[x, y] != null)
                {
                    List<Vector2Int> group = new List<Vector2Int>();
                    ObjectColor color = gridArray[x, y].GetComponent<GridObject>().GetObjectColor();
                    DFS(gridArray, gridColumns, gridRows, x, y, visited, group, color);

                    if (group.Count >= 2) 
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

}
