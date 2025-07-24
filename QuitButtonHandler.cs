using UnityEngine;

public class QuitButtonHandler : MonoBehaviour
{
    // Method to quit the application
    public void QuitApplication()
    {
        #if UNITY_EDITOR
            // For testing in the editor
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // For the built application
            Application.Quit();
        #endif
    }
}