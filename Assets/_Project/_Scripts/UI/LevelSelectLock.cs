using System;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectLock : MonoBehaviour
{
    public int level;
    public Button button;
    public GameObject lockImage;

    private void Start()
    {
        SetLock(GameManager.Instance.currentMaxLevel < level);
        
        button.onClick.AddListener(() => GameManager.Instance.LoadLevel(level));
    }

    public void SetLock(bool isLock)
    {
        button.interactable = !isLock;
        if (lockImage) lockImage.SetActive(isLock);
    }
}
