using System;
using System.Collections;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
public class LevelLose : MonoBehaviour
{
    public CanvasGroup canvas;
    public InventoryUI inventoryUI;
    public BuildingManager buildingManager;
    public TMP_Text gameOverMessageText;

    public float waitBeforeStart = 5f;
    public float introTime = 1f;
    
    public void Initialize(string message)
    {
        gameOverMessageText.text = message;
        gameObject.SetActive(true);
    }

    public IEnumerator PlayLevelLose()
    {
        inventoryUI.HideUI();
        canvas.alpha = 0f;
        canvas.interactable = false;
        buildingManager.Exit();

        yield return new WaitForSeconds(waitBeforeStart);
        yield return Tween.Alpha(canvas, 1f, introTime).ToYieldInstruction();
        canvas.interactable = true;
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }
    
    public void ToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}