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
    private bool _levelEnded;
    
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

        _levelEnded = false;

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
        if (_levelEnded) return;
        lose.Initialize(@event.GameOverMessage);
        _levelEnded = false;
        StartCoroutine(lose.PlayLevelLose());
    }
    
    private void StartWin(WinEvent @event)
    {
        if (_levelEnded) return;
        win.Initialize(_levelData.levelNumber);
        _levelEnded = true;
        StartCoroutine(win.PlayLevelWin());
    }
}