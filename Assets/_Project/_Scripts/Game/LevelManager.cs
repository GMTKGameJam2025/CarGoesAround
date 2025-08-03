using System;
using System.Collections;
using UnityEngine;

public struct GameStartedEvent : IGameEvent
{
    
}

public class LevelManager : MonoBehaviour
{
    [SerializeField] private int levelNumber;
    [SerializeField] private LevelIntro intro;
    [SerializeField] private LevelWin win;
    [SerializeField] private LevelLose lose;
    [SerializeField] private InventoryUI inventoryUI;

    [Header("Level")]
    [SerializeField] private GridLevelDataSO levelData;
    [SerializeField] private GridExportImportSystem gridExportSystem;

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

        gridExportSystem.LoadFromScriptableObject(levelData);
        
        StartCoroutine(StartIntro());
    }

    private IEnumerator StartIntro()
    {
        intro.Initialize(levelNumber);
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
        win.Initialize(levelNumber);
        StartCoroutine(win.PlayLevelLose());
    }
}