using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuEvents : MonoBehaviour
{
    public GameObject difficultyButtons; // Parent object for buttons
    public TMP_Text startGameButtonText;

    public string selectedScene = "GameScene"; // Public variable for scene selection

    void Start()
    {
        difficultyButtons.SetActive(false); // Hide difficulty buttons initially
    }

    public void ShowDifficultyOptions()
    {
        difficultyButtons.SetActive(true); // Show difficulty buttons
    }

    public void SelectDifficulty(string difficulty)
    {
        PlayerPrefs.SetString("Difficulty", difficulty); // Save difficulty setting
        SceneManager.LoadScene(selectedScene); // Load the selected game scene
    }
}
