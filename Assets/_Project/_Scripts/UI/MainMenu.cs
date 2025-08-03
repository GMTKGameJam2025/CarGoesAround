using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject levelSelectCanvas;
    [SerializeField] private GameObject creditCanvas;

    public void ToMainMenu()
    {
        mainMenuCanvas.SetActive(true);
        creditCanvas.SetActive(false);
        levelSelectCanvas.SetActive(false);
    }
    
    public void ToLevelSelect()
    {
        mainMenuCanvas.SetActive(false);
        creditCanvas.SetActive(false);
        levelSelectCanvas.SetActive(true);
    }
    
    public void ToCredit()
    {
        mainMenuCanvas.SetActive(false);
        creditCanvas.SetActive(true);
        levelSelectCanvas.SetActive(false);
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
