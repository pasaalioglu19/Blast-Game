using UnityEngine;

public class GridInitializer
{
    private float gridPositionXYOffset = 0.5f;
    private GameObject[,] gridArray;

    public void InitializeGrid(string[] gridData, int gridWidth, int gridHeight, ObstacleFactory obstacleFactory)
    {
        if (gridHeight > 10 || gridWidth > 10)
        {
            string warningMessage = "Invalid input: ";

            if (gridHeight > 10)
                warningMessage += $"gridHeight ({gridHeight}) exceeds the limit. Maximum allowed is 10. ";

            if (gridWidth > 10)
                warningMessage += $"gridWidth ({gridWidth}) exceeds the limit. Maximum allowed is 10. ";

            Debug.LogWarning(warningMessage);
            return;
        }

        gridArray = new GameObject[gridWidth, gridHeight];

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                int index = y * gridWidth + x;
                string gridItem = gridData[index];
                GameObject newObject;
                if (ObjectFactory.Instance.GetIndexFromItem(gridItem) != -1)
                {
                    newObject = ObjectFactory.Instance.CreateObject(gridItem, x, y);
                    SetupObject(newObject, x, y, gridItem);
                }
                else if (PowerupFactory.Instance.GetIndexFromItem(gridItem) != -1)
                {
                    newObject = PowerupFactory.Instance.CreateObject(gridItem, x, y);
                    SetupPowerup(newObject, x, y);

                }
                else if (obstacleFactory.GetIndexFromItem(gridItem) != -1)
                {
                    newObject = obstacleFactory.CreateObject(gridItem, x, y);
                    SetupObstacle(newObject, x, y);

                }


            }
        }

        GameGrid.Instance.InitializeGrid(gridArray, gridWidth, gridHeight, gridPositionXYOffset);
    }

    private void SetupObject(GameObject newEntity, int x, int y, string gridItem)
    {
        Object newObject = newEntity.GetComponent<Object>();
        newObject.Initialize(x, y, gridItem);
        gridArray[x, y] = newEntity;
    }

    private void SetupPowerup(GameObject newEntity, int x, int y)
    {
        Powerups newPowerup = newEntity.GetComponent<Powerups>();
        newPowerup.Initialize(x, y);
        gridArray[x, y] = newEntity;

        if (newPowerup.TryGetComponent<Rocket>(out var rocketComponent))
        {
            bool isHorizontal = Random.value > 0.5f;
            rocketComponent.IsHorizontal(isHorizontal);
        }
    }

    private void SetupObstacle(GameObject newEntity, int x, int y)
    {
        Obstacle newObstacle = newEntity.GetComponent<Obstacle>();
        newObstacle.Initialize(x, y);
        gridArray[x, y] = newEntity;

        if (newObstacle.TryGetComponent<Rocket>(out var rocketComponent))
        {
            bool isHorizontal = Random.value > 0.5f;
            rocketComponent.IsHorizontal(isHorizontal);
        }
    }
}
