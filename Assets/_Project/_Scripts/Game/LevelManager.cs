using System;
using System.Collections;
using UnityEngine;

public struct GameStartedEvent : IGameEvent
{
    
}

public class LevelManager : MonoBehaviour
{
    [SerializeField] private LevelIntro intro;
    [SerializeField] private LevelWin win;
    [SerializeField] private LevelLose lose;
    [SerializeField] private InventoryUI inventoryUI;

    [Header("Level")]
    [SerializeField] private GridExportImportSystem gridExportSystem;
    [SerializeField] private InventoryManager inventory;

    private LevelData _levelData;
    
    private void OnEnable()
    {
        EventBus.Subscribe<GameOverEvent>(StartLose);
        EventBus.Subscribe<WinEvent>(StartWin);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<GameOverEvent>(StartLose);
        EventBus.Unsubscribe<WinEvent>(StartWin);
    }

    private void Start()
    {
        intro.gameObject.SetActive(false);
        win.gameObject.SetActive(false);
        lose.gameObject.SetActive(false);

        _levelData = GameManager.Instance.GetCurrentLevelData();
        gridExportSystem.LoadFromScriptableObject(_levelData.gridData);
        inventory.SetInventoryPreset(_levelData.inventoryPreset);
        
        StartCoroutine(StartIntro());
    }

    private IEnumerator StartIntro()
    {
        intro.Initialize(_levelData.levelNumber, _levelData.subtitle);
        yield return StartCoroutine(intro.PlayLevelIntro());
        EventBus.Fire<GameStartedEvent>(new());
    }

    private void StartLose(GameOverEvent @event)
    {
        lose.Initialize(@event.GameOverMessage);
        StartCoroutine(lose.PlayLevelLose());
    }
    
    private void StartWin(WinEvent @event)
    {
        win.Initialize(_levelData.levelNumber);
        StartCoroutine(win.PlayLevelWin());
    }
}