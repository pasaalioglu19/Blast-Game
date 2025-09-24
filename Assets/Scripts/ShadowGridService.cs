using System.Collections.Generic;
using UnityEngine;

public class ShadowGridService : IShadowGridService
{
    private readonly Dictionary<Vector2Int, Object> shadowGrid = new();

    public void AddToShadowGrid(Vector2Int position, Object obj)
    {
        shadowGrid[position] = obj;
    }

    public void ResetShadowGrid()
    {
        foreach (var position in shadowGrid.Keys)
        {
            shadowGrid[position]?.ResetObjectSprites();
        }
        shadowGrid.Clear();
    }

    public Dictionary<Vector2Int, Object> GetShadowGrid()
    {
        return new Dictionary<Vector2Int, Object>(shadowGrid);
    }
}
