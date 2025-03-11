using UnityEngine;

public class Box : Obstacle
{
    /*void Start()
    {
        canBeDestroyedByNormalBlast = true;
        canBeDestroyedByRocket = true;
        canBeDestroyedByTnt = true;
        durability = 1;
        canDrop = false;
    }
    */
    public override void TakeHit(ExplosionType explosionType)
    {
        Debug.Log("Box");
        base.TakeHit(explosionType);
    }
}
