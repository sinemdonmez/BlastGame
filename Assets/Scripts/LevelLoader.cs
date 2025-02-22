using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public int level_number;
    public int grid_width;
    public int grid_height;
    public int move_count;
    public List<string> grid;
}

public class LevelLoader : MonoBehaviour
{
    [Header("Level Data")]
    public string levelFileName;
    private LevelData currentLevelData;
    public int totalLevels = 10; // Change based on your game

    [Header("References")]
    public GridManager gridManager; // Manages the grid system
    public UIAnimator uiAnimator; // Controls UI animations (GridContainer + UI_top)

    [Header("Animation Settings")]
    public float delayBeforeUIAnimations = 0.5f; // Delay before UI animates
    public float delayBeforeGridSetup = 1.0f; // Delay before initializing the grid

    public void Awake(){
        SaveLastPlayedLevel(1);
    }


    public void LoadLevel(){
        
        int lastPlayedLevel = GetLastPlayedLevel();

        if (lastPlayedLevel > totalLevels)
        {
            Debug.Log("All levels are completed!");
            return;
        }

        levelFileName = $"level_{lastPlayedLevel:D2}.json"; // Format level file (e.g., level_03.json)
        string filePath = Path.Combine(Application.dataPath, "Levels", levelFileName);

        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            currentLevelData = JsonUtility.FromJson<LevelData>(jsonData);
            Debug.Log("Level loaded: " + currentLevelData.level_number);

            LoadLevelWithAnimations();
        }
        else
        {
            Debug.LogError("Level file not found: " + filePath);
        }
    }

    private void LoadLevelWithAnimations(){

        if (gridManager != null)
        {
            gridManager.InitializeGrid(currentLevelData); // Initialize grid before animations
        }
        else
        {
            Debug.LogError("GridManager reference is missing in LevelLoader!");
        }
        //TODO: is this wait necessary
       // yield return new WaitForSeconds(delayBeforeUIAnimations); // Wait before UI animations

        if (uiAnimator != null)
        {
            uiAnimator.AnimateUI(); // Start GridContainer & UI_top animations
        }
        else{
            Debug.Log("uianimator is null");
        }

        //yield return new WaitForSeconds(delayBeforeGridSetup); // Wait for UI animations to complete

    }

    public int GetLastPlayedLevel(){
        return PlayerPrefs.GetInt("LastPlayedLevel", 1); // Default to level 1
    }

    public void SaveLastPlayedLevel(int levelNumber){
        PlayerPrefs.SetInt("LastPlayedLevel", levelNumber); // Move to the next level
        PlayerPrefs.Save();
    }

    public void SetLastPlayedLevel(int levelNumber){
        SaveLastPlayedLevel(levelNumber);
        Debug.Log("Manually set last played level to: " + levelNumber);
    }

    public void IncreaseLevel(){
        int levelNumber = GetLastPlayedLevel();
        SetLastPlayedLevel(levelNumber+1);
    }
}
