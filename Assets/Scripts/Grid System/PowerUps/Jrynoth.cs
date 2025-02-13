using UnityEngine;

public class Jrynoth : GridPowerUps
{
    private GridManager gridManager;
    void Awake()
    {
        SetPowerUpType(PowerUpType.Jrynoth);
        gridManager = FindFirstObjectByType<GridManager>();
    }

    public override void Activate()
    {
        gridManager.CheckBlast(this);
    }
}
