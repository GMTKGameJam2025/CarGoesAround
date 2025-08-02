using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private int levelNumber;
    [SerializeField] private LevelIntro intro;
    [SerializeField] private LevelLose lose;
    [SerializeField] private InventoryUI inventoryUI;

    private void OnEnable()
    {
        EventBus.Subscribe<GameOverEvent>(StartLose);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<GameOverEvent>(StartLose);
    }

    private void Start()
    {
        StartLevelSequence();
    }

    private void StartLevelSequence()
    {
        intro.Initialize(levelNumber);
        StartCoroutine(intro.PlayLevelIntro());
    }

    private void StartLose(GameOverEvent @event)
    {
        lose.Initialize(@event.GameOverMessage);
        StartCoroutine(lose.PlayLevelLose());
    }
    
}