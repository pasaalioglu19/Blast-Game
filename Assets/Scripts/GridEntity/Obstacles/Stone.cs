using UnityEngine;

public class Stone : Obstacle
{
    /*void Start()
    {
        canBeDestroyedByNormalBlast = false;
        canBeDestroyedByRocket = false;
        canBeDestroyedByTnt = true;
        durability = 1;
        canDrop = false;
    }
    */

    public override void TakeHit(ExplosionType explosionType)
    {
        Debug.Log("Stone");
        base.TakeHit(explosionType);
    }
}
