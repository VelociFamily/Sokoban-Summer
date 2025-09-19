using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartLevel : MonoBehaviour
{
    [Tooltip("Optional: Restart on click (UI or 3D object with collider)")]
    public bool restartOnClick = true;

    private void OnMouseDown()
    {
        if (restartOnClick)
        {
            Restart();
        }
    }

    public void Restart()
    {
        // Reload current active scene
        var currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}