using UnityEngine;

[CreateAssetMenu(fileName = "ObstacleData", menuName = "ScriptableObjects/ObstacleData")]
public class ObstacleData : ScriptableObject
{
    // Index 0 for box, Index 1 for stone, Index 2 for vase
    public GameObject[] ObstaclePrefabs;
}
