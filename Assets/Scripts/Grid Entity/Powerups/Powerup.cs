using UnityEngine;

public abstract class Powerups : GridEntity
{
    private PowerupType powerupType;
    public abstract void Activate();

    private SpriteRenderer spriteRenderer;

    void Awake()
    {

    }
    void OnMouseDown()
    {
        Activate();
    }
    protected void SetPowerupType(PowerupType type)
    {
        powerupType = type;
    }

    public PowerupType GetPowerupType()
    {
        return powerupType;
    }

    public void Initialize(int x, int y)
    {
        SetGridX(x);
        SetGridY(y);
        SetIsCube(false);

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = y + 2;
    }
}
