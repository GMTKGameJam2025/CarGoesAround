using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CarWarehouse : MonoBehaviour
{
    [SerializeField] private float startDuration = 15f;
    [SerializeField] private TMP_Text text;
    [SerializeField] private GameObject car;

    private float _currentTime;
    private bool _isTimerActive;
    
    private void OnEnable()
    {
        EventBus.Subscribe<GameStartedEvent>(GameStarted);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<GameStartedEvent>(GameStarted);
    }

    private void Start()
    {
        car.SetActive(false);
        _currentTime = startDuration;
        _isTimerActive = false;
        text.text = Mathf.RoundToInt(startDuration).ToString();
    }

    private void GameStarted(GameStartedEvent @event)
    {
        _isTimerActive = true;
    }

    private void Update()
    {
        if (!_isTimerActive)
            return;
        if (_currentTime > 0)
        {
            _currentTime -= Time.deltaTime;
            text.text = Mathf.RoundToInt(_currentTime).ToString();
        }
        else
        {
            _isTimerActive = false;
            car.SetActive(true);
        }
    }
}
