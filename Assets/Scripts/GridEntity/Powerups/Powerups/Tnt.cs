

public class Tnt : Powerups
{
    private GridManager gridManager;
    void Awake()
    {
        SetPowerupType(PowerupType.Tnt);
        gridManager = FindFirstObjectByType<GridManager>();
    }

    public override void Activate()
    {
        gridManager.CheckBlast(this);
    }
}
