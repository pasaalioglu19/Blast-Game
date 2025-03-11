using UnityEngine;

[CreateAssetMenu(fileName = "PowerupData", menuName = "ScriptableObjects/PowerupData")]
public class PowerupData : ScriptableObject
{
    // Index 0 for bomb, Index 1 for rocket, Index 2 for Jrynoth
    public GameObject[] PowerupsPrefabs;
}
