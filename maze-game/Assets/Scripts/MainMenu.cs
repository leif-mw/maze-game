using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Controls the play button and loads the game scene
    public void Play()
    {
        Cursor.visible = false;
        SceneManager.LoadScene("Game");
    }

    // Controls the quit button and exits the application
    public void Quit()
    {
        Application.Quit();
    }
}
