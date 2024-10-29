using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleProcess : MonoBehaviour
{
    // Start is called before the first frame update

    public void StartButton()
    {
        Debug.Log("Start");
        SceneManager.LoadScene("Game");
    }

    public void restartScene()
    {
        Debug.Log("Restart");
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
