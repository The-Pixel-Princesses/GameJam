using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Game";

    void Update()
    {
        // TEMP: keyboard fallback so you can keep working
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("[SceneLoader] Key 1 pressed — loading game scene.");
            LoadGameScene();
        }
    }

    // Called by UI Button (later) AND by key press (now)
    public void LoadGameScene()
    {
        Debug.Log("[SceneLoader] Loading scene: " + gameSceneName);
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("[SceneLoader] Quit requested.");
        UnityEditor.EditorApplication.isPlaying = false;
    }
}
