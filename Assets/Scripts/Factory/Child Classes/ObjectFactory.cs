using UnityEngine;

public class ObjectFactory : BaseGridFactory
{
    private ObjectData objectData;
    private string[] colorsSelected;

    private static ObjectFactory _instance;
    public static ObjectFactory Instance => _instance ??= new ObjectFactory();

    public override void Initialize(object data)
    {
        if (data is ObjectData gridData)
        {
            this.objectData = gridData;
        }
        else
        {
            Debug.LogError("Invalid data type for ObjectFactory!");
        }
    }

    public void SetColors(string[] colors)
    {
        colorsSelected = colors;
    }

    public override GameObject CreateObject(string gridItem, float x, float y)
    {
        int index = GetIndexFromItem(gridItem);
        if (index < 0 || index >= objectData.ObjectPrefabs.Length) 
        {
            Debug.LogError($"Invalid Object index: {index}");
            return null;
        }

        return InstantiateObject(objectData.ObjectPrefabs[index], x, y);
    }

    public override int GetIndexFromItem(string gridItem)
    {
        return gridItem switch
        {
            "b" => 0, //blue
            "g" => 1, //green
            "pi" => 2, //purple
            "pu" => 3, //pink
            "r" => 4, //red
            "y" => 5, //yellow
            _ => -1
        };
    }

    public void InstantiateRandomObject(int x, int y)
    {
        int selectedColor = Random.Range(0, colorsSelected.Length);
        string colorName = colorsSelected[selectedColor];
        float objectHeight = GameGrid.Instance.GridHeight + 1.5f;
        GameObject newObject = CreateObject(colorName, x, objectHeight);
        SetupObject(newObject, x, y, colorName);
        AnimationManager.Instance.DropObjectAnim(newObject, new Vector3(x * gridPositionXYOffset, y * gridPositionXYOffset, 0), objectHeight - y);
    }

    private void SetupObject(GameObject newEntity, int x, int y, string colorName)
    {
        Object newObject = newEntity.GetComponent<Object>();
        newObject.Initialize(x, y, colorName);
        GameGrid.Instance.GridArray[x, y] = newEntity;
    }
}
