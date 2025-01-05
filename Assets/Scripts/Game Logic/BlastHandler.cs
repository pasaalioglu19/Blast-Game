using System.Collections;
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

    private void DFS(GameObject[,] gridArray, int gridColumns, int gridRows, int x, int y, bool[,] visited, List<Vector2Int> group, ObjectColor color)
    {
        if (x < 0 || x >= gridColumns || y < 0 || y >= gridRows || visited[x, y] || gridArray[x, y] == null || gridArray[x, y].GetComponent<GridObject>().GetObjectColor() != color)
            return;

        visited[x, y] = true;
        group.Add(new Vector2Int(x, y));

        DFS(gridArray,  gridColumns,  gridRows, x + 1, y, visited, group, color);
        DFS(gridArray,  gridColumns,  gridRows, x - 1, y, visited, group, color);
        DFS(gridArray,  gridColumns,  gridRows, x, y + 1, visited, group, color);
        DFS(gridArray,  gridColumns,  gridRows, x, y - 1, visited, group, color);
    }
}
