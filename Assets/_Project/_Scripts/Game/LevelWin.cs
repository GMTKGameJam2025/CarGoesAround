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

    public void NextLevel()
    {
        SceneManager.LoadScene("Level " + (_currentLevel + 1), LoadSceneMode.Single);
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