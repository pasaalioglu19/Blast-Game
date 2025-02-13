using UnityEngine;

public abstract class GridObstacle : GridEntity
{
    [SerializeField] protected int durability; 

    public virtual void TakeHit()
    {
        durability--;
        if (durability <= 0)
        {
            DestroyObstacle();
        }
    }

    protected virtual void DestroyObstacle()
    {
        Destroy(gameObject); 
    }
}
