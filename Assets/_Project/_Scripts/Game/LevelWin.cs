using System.Collections;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
public class LevelWin : MonoBehaviour
{
    public CanvasGroup canvas;
    public InventoryUI inventoryUI;
    public BuildingManager buildingManager;

    public float waitBeforeStart = 5f;
    public float introTime = 1f;

    private int _currentLevel;
    
    public void Initialize(int currentLevel)
    {
        _currentLevel = currentLevel;
        gameObject.SetActive(true);

        GameManager.Instance.currentMaxLevel = currentLevel + 1;
    }

    public IEnumerator PlayLevelWin()
    {
        inventoryUI.HideUI();
        canvas.alpha = 0f;
        canvas.interactable = false;
        buildingManager.Exit();

        yield return new WaitForSeconds(waitBeforeStart);
        yield return Tween.Alpha(canvas, 1f, introTime).ToYieldInstruction();
        canvas.interactable = true;
    }

    public void NextLevel()
    {
        GameManager.Instance.LoadLevel(_currentLevel + 1);
    }
    
    public void RestartLevel()
    {
        GameManager.Instance.LoadLevel(_currentLevel);
    }
    
    public void ToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}