using UnityEngine;

public abstract class BaseGridFactory : IGridFactory
{
    protected const float gridPositionXYOffset = 0.5f;

    public abstract void Initialize(object data);
    public abstract GameObject CreateObject(string gridItem, float x, float y);

    public abstract int GetIndexFromItem(string gridItem);
    protected GameObject InstantiateObject(GameObject prefab, float x, float y)
    {
        return Object.Instantiate(prefab, new Vector3(x * gridPositionXYOffset, y * gridPositionXYOffset, 0), Quaternion.identity);
    }

}
