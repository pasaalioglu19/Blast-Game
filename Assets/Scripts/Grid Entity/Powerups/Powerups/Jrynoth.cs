using UnityEngine;

public class Jrynoth : Powerups
{
    private GridManager gridManager;
    void Awake()
    {
        SetPowerupType(PowerupType.Jrynoth);
        gridManager = FindFirstObjectByType<GridManager>();
    }

    public override void Activate()
    {
        gridManager.CheckBlast(this);
    }
}
