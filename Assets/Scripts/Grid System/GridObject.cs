using UnityEngine;

public class GridObject : GridEntity
{
    private Sprite originalSprite; 
    private GridManager gridManager;

    void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        originalSprite = GetComponent<SpriteRenderer>().sprite;
    }

    void OnMouseDown()
    {
        //gridManager.CheckBlast(this);
    }

    public void ResetObjectSprites()
    {
        //gameObject.GetComponent<SpriteRenderer>().sprite = originalSprite; //Changed sprites for tnt hint reverts to original
    }
}
