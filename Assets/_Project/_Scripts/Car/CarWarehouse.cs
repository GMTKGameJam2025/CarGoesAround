using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CarWarehouse : MonoBehaviour, IBuildable
{
    [SerializeField] private float startDuration = 15f;
    [SerializeField] private TMP_Text text;
    [SerializeField] private GameObject car;

    private float _currentTime;
    private bool _hasCarStarted;
    
    private void OnEnable()
    {
        EventBus.Subscribe<GameStartedEvent>(GameStarted);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<GameStartedEvent>(GameStarted);
    }

    private void Awake()
    {
        car.SetActive(false);
    }

    public void GameStarted(GameStartedEvent @event)
    {
        _currentTime = startDuration;
    }

    public void OnBuild(GridBuildPiece piece)
    {
        if (piece.builtSource == "GridLoader")
        {
            return;
        }

        Debug.Log("Warehouse Built from GridLoader");
        _currentTime = startDuration;
    }

    private void Update()
    {
        if (!_hasCarStarted)
        {
            if (_currentTime > 0)
            {
                _currentTime -= Time.deltaTime;
                text.text = Mathf.RoundToInt(_currentTime).ToString();
            }
            else
            {
                if (_hasCarStarted) return;

                _hasCarStarted = true;
                car.SetActive(true);
            }
        }
    }
}
