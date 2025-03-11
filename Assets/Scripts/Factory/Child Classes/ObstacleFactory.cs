using UnityEngine;

public class ObstacleFactory : BaseGridFactory
{
    ObstacleData obstacleData;

    public override void Initialize(object data)
    {
        if(data is ObstacleData gridData)
        {
            this.obstacleData = gridData;
        }
        else
        {
            Debug.LogError("Invalid data type for ObstacleFactory!");
        }
    }
    public override GameObject CreateObject(string gridItem, float x, float y)
    {
        int index = GetIndexFromItem(gridItem);
        if (index < 0 || index >= obstacleData.ObstaclePrefabs.Length)
        {
            Debug.LogError($"Invalid Obstacle index: {index}");
            return null;
        }

        return InstantiateObject(obstacleData.ObstaclePrefabs[index], x, y);
    }

    public override int GetIndexFromItem(string gridItem) {
        return gridItem switch
        {
            "bo" => 0, //box
            "s" => 1, //stone
            "v" => 2, //vase
            _ => -1
        };
    }
}
