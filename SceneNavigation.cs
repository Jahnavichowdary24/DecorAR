using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    // Method to load a specific scene
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Method to exit the application (optional for the back button)
    public void QuitApplication()
    {
        Application.Quit();
    }
}
