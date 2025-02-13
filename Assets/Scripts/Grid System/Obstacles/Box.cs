using UnityEngine;

public class Box : GridObstacle
{
    void Start()
    {
        durability = 1; 
    }

    protected override void DestroyObstacle()
    {
        base.DestroyObstacle();
    }
}
