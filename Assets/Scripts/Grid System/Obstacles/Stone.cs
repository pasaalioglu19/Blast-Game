using UnityEngine;

public class Stone : GridObstacle
{
    void Start()
    {
        durability = int.MaxValue; // Yok edilemez
    }

    public override void TakeHit()
    {

    }
}
