using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverEvents : MonoBehaviour
{
    public string mainMenuScene; // Public variable for menu scene

    // Called when the "Back to Menu" button is clicked
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    // Called when the "Exit Game" button is clicked
    public void ExitGame()
    {
        Application.Quit();
    }

    void Start()
    {
        Cursor.visible = true; // Show the mouse cursor
        Cursor.lockState = CursorLockMode.None; // Unlock the mouse from the center

    }
}
