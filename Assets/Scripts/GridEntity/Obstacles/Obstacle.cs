using UnityEngine;

public abstract class Obstacle : GridEntity, IExplosionReaction
{
    private ObstacleType obstacletype;
    private SpriteRenderer spriteRenderer;

    [SerializeField] protected bool canBeDestroyedByNormalBlast;
    [SerializeField] protected bool canBeDestroyedByRocket;
    [SerializeField] protected bool canBeDestroyedByTnt;
    [SerializeField] protected int durability;
    [SerializeField] protected bool canDrop;

    public bool CanDrop => canDrop;

    public bool CanBeDestroyedBy(ExplosionType explosionType)
    {
        return explosionType switch
        {
            ExplosionType.NormalBlast => canBeDestroyedByNormalBlast,
            ExplosionType.RocketBlast => canBeDestroyedByRocket,
            ExplosionType.TntBlast => canBeDestroyedByTnt,
            _ => false
        };
    }

    public virtual void TakeHit(ExplosionType explosionType)
    {
        if (!CanBeDestroyedBy(explosionType))
        {
            return;
        }
        durability--;
        if (durability <= 0)
        {
            DestroyObstacle();
        }
    }

    private void DestroyObstacle()
    {
        Destroy(gameObject); 
    }

    public void Initialize(int x, int y)
    {
        SetGridX(x);
        SetGridY(y);
        SetIsCube(false);

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = y + 3;
    }

    protected void SetObstacleType(ObstacleType type)
    {
        obstacletype = type;
    }

    public ObstacleType GetObstacleType()
    {
        return obstacletype;
    }
}
