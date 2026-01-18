using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    [Header("Game Scene")]
    [SerializeField] private string gameSceneName = "Game";

    public void OnStartPressed()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnQuitPressed()
    {
        Application.Quit();
    }
}
