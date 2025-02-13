using UnityEngine;
using static GridPowerUps;

public class Tnt : GridPowerUps
{
    private GridManager gridManager;
    void Awake()
    {
        SetPowerUpType(PowerUpType.Tnt);
        gridManager = FindFirstObjectByType<GridManager>();
    }

    public override void Activate()
    {
        gridManager.CheckBlast(this);
    }
}
