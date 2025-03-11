using UnityEngine;

public interface IGridFactory
{
    void Initialize(object data); 
    GameObject CreateObject(string gridItem, float x, float y);
}
