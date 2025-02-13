using UnityEngine;

public class Vase : GridObstacle
{
    void Start()
    {
        durability = 2; 
    }

    protected override void DestroyObstacle()
    {
        base.DestroyObstacle();
    }
}
