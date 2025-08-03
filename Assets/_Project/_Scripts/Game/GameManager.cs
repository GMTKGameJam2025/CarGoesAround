using System;
using UnityEngine;

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    private static readonly object Lock = new object();

    public int currentLevel = 1;
    public int currentMaxLevel;
    public LevelDataSO levelData;

    // Singleton Instance Property
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        // Try to find existing instance in scene
                        _instance = FindFirstObjectByType<GameManager>();
                        
                        if (_instance == null)
                        {
                            // Create new GameObject with GameManager
                            GameObject go = new GameObject("GameManager");
                            _instance = go.AddComponent<GameManager>();
                            DontDestroyOnLoad(go);
                        }
                    }
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        // Ensure only one instance exists
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        currentMaxLevel = PlayerPrefs.GetInt("MaxReachedLevel", 1);
    }

    public void LoadLevel(int level)
    {
        currentLevel = level;
        if (currentMaxLevel < currentLevel)
        {
            currentMaxLevel = currentLevel;
        }

        LevelData data = GetLevelData(level);
        
        SceneManager.LoadScene(data.sceneName, LoadSceneMode.Single);
    }

    public LevelData GetLevelData(int level)
    {
        if (level <= 0 || level >= levelData.allLevelData.Count)
        {
            return null;
        }
        
        return levelData.allLevelData[level - 1];
    }

    
    public LevelData GetCurrentLevelData()
    {
        return GetLevelData(currentLevel);
    }

    public void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("MaxReachedLevel", currentMaxLevel);
    }
}