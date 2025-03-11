using UnityEngine;

[CreateAssetMenu(fileName = "ObjectData", menuName = "ScriptableObjects/ObjectData", order = 1)]
public class ObjectData : ScriptableObject
{
    //Class containing prefabs to place on the grid and sprites to change for the hint
    public GameObject[] ObjectPrefabs; // Order alphabetic -> 0: Blue, 1: Green, 2: Pink, 3: Purple, 4: Red, 5: Yellow 
    public Sprite[] ObjectFirstSprites;
    public Sprite[] ObjectSecondSprites;
    public Sprite[] ObjectThirdSprites;
}
