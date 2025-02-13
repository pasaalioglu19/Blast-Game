using UnityEngine;

public abstract class GridPowerUps : GridEntity
{
    public enum PowerUpType { Tnt, Rocket, Jrynoth}
    private PowerUpType powerUpType;
    public abstract void Activate();

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void OnMouseDown()
    {
        Activate();
    }
    protected void SetPowerUpType(PowerUpType type)
    {
        powerUpType = type;
    }

    public PowerUpType GetPowerUpType()
    {
        return powerUpType;
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
