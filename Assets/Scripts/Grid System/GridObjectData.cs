using UnityEngine;

[CreateAssetMenu(fileName = "GridObjectData", menuName = "ScriptableObjects/GridObjectData", order = 1)]
public class GridObjectData : ScriptableObject
{
    //Class containing prefabs to place on the grid and sprites to change for the hint
    public GameObject[] ObjectPrefabs;
    public Sprite[] ObjectFirstSprites;
    public Sprite[] ObjectSecondSprites;
    public Sprite[] ObjectThirdSprites;
}
