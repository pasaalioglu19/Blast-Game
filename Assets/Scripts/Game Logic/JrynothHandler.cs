using System.Collections.Generic;
using UnityEngine;

public interface IJrynothHandler
{
    void HandleJrynothBlast(CubeSpriteOrganizer cubeSpriteOrganizer, AnimationManager animationManager, GridManager gridManager, GameObject[,] gridArray, Dictionary<int, List<Vector2Int>> cubeGroups, int lastDefaultIconIndex, int gridWidth, int gridHeight, float xyoffSet);
}

public class JrynothHandler : IJrynothHandler
{
    public void HandleJrynothBlast(CubeSpriteOrganizer cubeSpriteOrganizer, AnimationManager animationManager, GridManager gridManager, GameObject[,] gridArray, Dictionary<int, List<Vector2Int>> cubeGroups, int lastDefaultIconIndex, int gridWidth, int gridHeight, float xyoffSet)
    {
        foreach (var group in cubeGroups)
        {
            if (group.Value.Count > lastDefaultIconIndex)
            {
                int touchedIndexX = group.Value[0].x;
                int touchedIndexY = group.Value[0].y;
                float touchedObjectX = touchedIndexX * xyoffSet;
                float touchedObjectY = touchedIndexY * xyoffSet;

                foreach (var position in group.Value)
                {
                    int x = position.x;
                    int y = position.y;
                    GameObject objectInGroup = gridArray[x, y];
                    gridArray[x, y] = null;
                    animationManager.DestroyObjectAnim(objectInGroup, objectInGroup.GetComponent<GridObject>().GetObjectColor(), objectInGroup.GetComponent<GridObject>().IsDefaultSprite(), touchedObjectX, touchedObjectY);
                }

                gridManager.GenerateAllPowerUps(touchedIndexX, touchedIndexY, group.Value.Count);
                group.Value.Clear();
            }
        }
    }
}
