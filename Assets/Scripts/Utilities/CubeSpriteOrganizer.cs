using System.Collections.Generic;
using UnityEngine;

public class CubeSpriteOrganizer
{
    private ObjectData objectData;
    private readonly int lastDefaultIconIndex;
    private readonly int lastFirstIconIndex;
    private readonly int lastSecondIconIndex;

    public CubeSpriteOrganizer(ObjectData objectData, int lastDefaultIconIndex, int lastFirstIconIndex, int lastSecondIconIndex)
    {
        this.objectData = objectData;
        this.lastDefaultIconIndex = lastDefaultIconIndex;
        this.lastFirstIconIndex = lastFirstIconIndex;
        this.lastSecondIconIndex = lastSecondIconIndex;
    }

    public void OrganizeCubeSprites(Dictionary<int, List<Vector2Int>> cubeGroups)
    {
        var gridArray = GameGrid.Instance.GridArray;
        foreach (var group in cubeGroups)
        {
            int spriteIndex = GetSpriteIndex(group.Value.Count);
            foreach (var position in group.Value)
            {
                GameObject cube = gridArray[position.x, position.y];
                if (cube != null)
                {
                    SpriteRenderer spriteRenderer = cube.GetComponent<SpriteRenderer>();
                    ObjectColor color = cube.GetComponent<Object>().GetObjectColor();
                    ChangeCubeSprites(spriteRenderer, color, spriteIndex);
                }
            }
        }
    }

    public int GetSpriteIndex(int groupSize)
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
                return objectData.ObjectFirstSprites;
            case 1:
                return objectData.ObjectSecondSprites;
            case 2:
                return objectData.ObjectThirdSprites;
            default:
                return null;
        }
    }

    public void BringDefaultObjectSprites(Dictionary<int, List<Vector2Int>> cubeGroups)
    {
        foreach (var group in cubeGroups)
        {
            foreach (var position in group.Value)
            {
                int x = position.x;
                int y = position.y;

                if (GameGrid.Instance.GridArray[x, y]?.TryGetComponent(out Object objectComponent) == true)
                {
                    objectComponent.ResetObjectSprites();
                }
            }
        }
    }
}
