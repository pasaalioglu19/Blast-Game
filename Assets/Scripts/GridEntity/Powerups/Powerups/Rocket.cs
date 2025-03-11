using UnityEngine;

public class Rocket : Powerups
{
    private Sprite rocketHalfSprite;
    private GridManager gridManager;
    private bool isHorizontal;
    void Awake()
    {
        SetPowerupType(PowerupType.Rocket);
        gridManager = FindFirstObjectByType<GridManager>();
        rocketHalfSprite = Resources.Load<Sprite>("Art/Powerups/Rocket/Rocket-Half");

        if (rocketHalfSprite == null)
        {
            Debug.LogError("Rocket sprite could not be loaded! Check the path.");
        }
    }
    public override void Activate()
    {
        gridManager.CheckBlast(this);
    }

    public void IsHorizontal(bool isHorizontal)
    {
        this.isHorizontal = isHorizontal;
        if (isHorizontal)
        {
            transform.rotation = Quaternion.Euler(0, 0, 90f);
        }
    }
    public bool IsHorizontal()
    {
        return isHorizontal;
    }

    public Sprite GetRocketHalfSprite()
    {
        return rocketHalfSprite;
    }
}
