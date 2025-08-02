using System;
using System.Collections;
using PrimeTween;
using TMPro;
using UnityEngine;

public class LevelIntro : MonoBehaviour
{
    public CanvasGroup introCanvas;
    public TMP_Text levelText;
    public InventoryUI inventoryUI;

    public float introTime = 1f;
    public float outroTime = 0.5f;
    
    public void Initialize(int levelNumber)
    {
        levelText.text = "Level: " + levelNumber;
        gameObject.SetActive(true);
        inventoryUI.HideUI(true);
    }
    
    public IEnumerator PlayLevelIntro()
    {
        introCanvas.alpha = 0f;
        yield return Tween.Alpha(introCanvas, 1f, introTime).ToYieldInstruction();
        yield return Tween.Alpha(introCanvas, 0f, outroTime).ToYieldInstruction();
        inventoryUI.ShowUI();
        gameObject.SetActive(false);
    }
}
