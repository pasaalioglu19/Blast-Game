using static GridObject;
using System.Collections.Generic;
using UnityEngine;

public class CubeSpriteOrganizer
{
    private GridObjectData gridObjectData;
    private readonly int lastDefaultIconIndex;
    private readonly int lastFirstIconIndex;
    private readonly int lastSecondIconIndex;

    public CubeSpriteOrganizer(GridObjectData gridObjectData, int lastDefaultIconIndex, int lastFirstIconIndex, int lastSecondIconIndex)
    {
        this.gridObjectData = gridObjectData;
        this.lastDefaultIconIndex = lastDefaultIconIndex;
        this.lastFirstIconIndex = lastFirstIconIndex;
        this.lastSecondIconIndex = lastSecondIconIndex;
    }

    public void OrganizeCubeSprites(Dictionary<int, List<Vector2Int>> cubeGroups, GameObject[,] gridArray)
    {
        foreach (var group in cubeGroups)
        {
            int spriteIndex = GetSpriteIndex(group.Value.Count);
            foreach (var position in group.Value)
            {
                GameObject cube = gridArray[position.x, position.y];
                if (cube != null)
                {
                    SpriteRenderer spriteRenderer = cube.GetComponent<SpriteRenderer>();
                    ObjectColor color = cube.GetComponent<GridObject>().GetObjectColor();
                    ChangeCubeSprites(spriteRenderer, color, spriteIndex);
                }
            }
        }
    }

    private int GetSpriteIndex(int groupSize)
    {
        if (groupSize > lastSecondIconIndex)
            return 2;
        else if (groupSize > lastFirstIconIndex)
            return 1;
        else if (groupSize > lastDefaultIconIndex)
            return 0;
        return -1;
    }

    private void ChangeCubeSprites(SpriteRenderer spriteRenderer, ObjectColor color, int index)
    {
        Sprite[] sprites = GetSpriteArrayByIndex(index);
        if (sprites != null && (int)color < sprites.Length)
        {
            spriteRenderer.sprite = sprites[(int)color];
        }
    }

    private Sprite[] GetSpriteArrayByIndex(int index)
    {
        switch (index)
        {
            case 0:
                return gridObjectData.ObjectFirstSprites;
            case 1:
                return gridObjectData.ObjectSecondSprites;
            case 2:
                return gridObjectData.ObjectThirdSprites;
            default:
                return null;
        }
    }
}
