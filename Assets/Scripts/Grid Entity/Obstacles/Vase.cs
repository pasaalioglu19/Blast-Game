using UnityEngine;

public class Vase : Obstacle
{
    [SerializeField] private Sprite brokenVase;
    /*void Start()
    {
        canBeDestroyedByNormalBlast = true;
        canBeDestroyedByRocket = true;
        canBeDestroyedByTnt = true;
        durability = 2; 
        canDrop = true;
    }
    */

    public override void TakeHit(ExplosionType explosionType)
    {
        if(durability == 2)
        {
            this.GetComponent<SpriteRenderer>().sprite = brokenVase;
        }
        
        base.TakeHit(explosionType);
    }
}
