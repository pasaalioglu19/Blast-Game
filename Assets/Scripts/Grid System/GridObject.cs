using UnityEngine;


public class GridObject : GridEntity
{
    public enum ObjectColor
    {
        Blue,
        Green,
        Pink,
        Purple,
        Red,
        Yellow
    }

    private ObjectColor objectColor;
    private Sprite originalSprite; 
    private GridManager gridManager;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        originalSprite = GetComponent<SpriteRenderer>().sprite;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnMouseDown()
    {
        gridManager.CheckBlast(this);
    }

    public bool IsDefaultSprite()
    {
        return spriteRenderer.sprite == originalSprite;
    }

    public void ResetObjectSprites()
    {
        spriteRenderer.sprite = originalSprite;
    } 

    public void SetColor(int index)
    {
        switch (index)
        {
            case 0:
                objectColor = ObjectColor.Blue;
                break;
            case 1:
                objectColor = ObjectColor.Green; 
                break;
            case 2:
                objectColor = ObjectColor.Pink; 
                break;
            case 3:
                objectColor = ObjectColor.Purple; 
                break;
            case 4:
                objectColor = ObjectColor.Red; 
                break;
            case 5:
                objectColor = ObjectColor.Yellow; 
                break;
        }
    }

    public ObjectColor GetObjectColor()
    {
        return objectColor;
    }
}
