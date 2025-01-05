using System.Collections;
using System.IO;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private const string levelInfoFilePath = "Assets/Levels/current_level.json";
    private const string levelJsonPathFormat = "Assets/Levels/level_{0:D2}.json";

    private GridManager gridManager;
    private LevelData currentLevelData;

    public LevelInfo CurrentLevelInfo { get; private set; }

    void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("GridManager not found!");
            return;
        }

        StartCoroutine(SetupLevel());
    }

 
    // Delays the level setup briefly to ensure all objects are initialized and accessible
    private IEnumerator SetupLevel()
    {
        yield return new WaitForSeconds(0.01f);
        LoadLevelInfo();
        LoadLevelData();
        InitializeLevel();
    }

    private void LoadLevelInfo()
    {
        if (LoadJsonFile(levelInfoFilePath, out LevelInfo levelInfo))
        {
            CurrentLevelInfo = levelInfo;
        }
    }
    private void LoadLevelData()
    {
        string levelJsonPath = string.Format(levelJsonPathFormat, CurrentLevelInfo.level_number);
        LoadJsonFile(levelJsonPath, out currentLevelData);
    }

    private bool LoadJsonFile<T>(string filePath, out T data) where T : class
    {
        data = null;
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<T>(json);
            return true;
        }
        else
        {
            Debug.LogError($"{filePath} not found!");
            return false;
        }
    }

    public void UpdateCurrentLevel()
    {
        if (File.Exists(levelInfoFilePath))
        {
            CurrentLevelInfo.level_number++;
            string updatedJson = JsonUtility.ToJson(CurrentLevelInfo, true);
            File.WriteAllText(levelInfoFilePath, updatedJson);

        }
        else
        {
            Debug.LogError("Level info file not found!");
        }
    }



    public void RestartLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LevelScene");
    }

    private void InitializeLevel()
    {
        gridManager.InitializeGridWithLevelData(currentLevelData.grid_rows, currentLevelData.grid_columns, currentLevelData.colors_count, currentLevelData.last_default_icon_index, currentLevelData.last_first_icon_index, currentLevelData.last_second_icon_index);
    }

    public int GetLevelNumber()
    {
        return CurrentLevelInfo.level_number;
    }
}


[System.Serializable]
public class LevelInfo
{
    public int level_number;
}