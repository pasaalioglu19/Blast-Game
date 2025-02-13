using UnityEngine;

[CreateAssetMenu(fileName = "GridPowerUpsData", menuName = "ScriptableObjects/GridPowerUpsData")]
public class GridPowerUpsData : ScriptableObject
{
    // Index 0 for bomb, Index 1 for rocket
    public GameObject[] PowerUpsPrefabs;
}
