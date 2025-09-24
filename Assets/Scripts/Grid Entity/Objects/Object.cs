using UnityEngine;


public class Object : GridEntity
{
    private ObjectColor objectColor;
    private Sprite originalSprite; 
    private GridManager gridManager;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        originalSprite = GetComponent<SpriteRenderer>().sprite;
    }

    void OnMouseDown()
    {
        gridManager.CheckBlast(this);
    }

    public void Initialize(int x, int y, string colorName)
    {
        SetGridX(x);
        SetGridY(y);
        SetIsCube(true);
        SetColor(colorName);

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = y + 2; 
    }
    public bool IsDefaultSprite()
    {
        return spriteRenderer.sprite == originalSprite;
    }

    public void ResetObjectSprites()
    {
        spriteRenderer.sprite = originalSprite;
    } 

    public void SetColor(string colorName)
    {
        switch (colorName)
        {
            case "b":
                objectColor = ObjectColor.Blue;
                break;
            case "g":
                objectColor = ObjectColor.Green; 
                break;
            case "pi":
                objectColor = ObjectColor.Pink; 
                break;
            case "pu":
                objectColor = ObjectColor.Purple; 
                break;
            case "r":
                objectColor = ObjectColor.Red; 
                break;
            case "y":
                objectColor = ObjectColor.Yellow; 
                break;
        }
    }

    public ObjectColor GetObjectColor()
    {
        return objectColor;
    }
}
