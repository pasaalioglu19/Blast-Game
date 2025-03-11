using UnityEngine;

public class PowerupFactory : BaseGridFactory
{
    private PowerupData powerupsData;

    private static PowerupFactory _instance;
    public static PowerupFactory Instance => _instance ??= new PowerupFactory();
    public override void Initialize(object data)
    {
        if (data is PowerupData gridData)
        {
            this.powerupsData = gridData;
        }
        else
        {
            Debug.LogError("Invalid data type for PowerupFactory!");
        }
    }

    // This function is for powerups that will be created from the grid array at the beginning of the game.
    public override GameObject CreateObject(string gridItem, float x, float y)
    {
        int index = GetIndexFromItem(gridItem);
        return CreateObject(index, x, y);
    }

    // This function is for powerups that occur as a result of explosions during the game.
    public GameObject CreateObject(int index, float x, float y)
    {
        if (index < 0 || index >= powerupsData.PowerupsPrefabs.Length)
        {
            Debug.LogError($"Invalid Powerup index: {index}");
            return null;
        }

        return InstantiateObject(powerupsData.PowerupsPrefabs[index], x, y);
    }

    public override int GetIndexFromItem(string gridItem)
    {
        return gridItem switch
        {
            "t" => 0, //tnt
            "ro" => 1, //rocket
            "j" => 2, //jrynoth
            _ => -1
        };
    }

    public void CreatePowerup(int index, int x, int y)
    {
        GameObject newObject = CreateObject(index, x, y);
        SetupPowerup(newObject, x, y);
    }

    private void SetupPowerup(GameObject newEntity, int x, int y)
    {
        Powerups newPowerup = newEntity.GetComponent<Powerups>();
        newPowerup.Initialize(x, y);
        GameGrid.Instance.GridArray[x, y] = newEntity;

        if (newPowerup.TryGetComponent<Rocket>(out var rocketComponent))
        {
            bool isHorizontal = Random.value > 0.5f;
            rocketComponent.IsHorizontal(isHorizontal);
        }
    }
}
