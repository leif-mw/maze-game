using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EndScreen : MonoBehaviour
{
    // Text containing the final time
    [SerializeField]
    private TMP_Text timeText;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = true;

        float finalTime = Stopwatch.finalTime;
        TimeSpan time = TimeSpan.FromSeconds(finalTime);
        timeText.text = time.ToString(@"mm\:ss\:fff");
    }

    // Controls the play again button and loads the game scene
    public void PlayAgain()
    {
        Cursor.visible = false;
        SceneManager.LoadScene("Game");
    }

    // Controls the main menu button and loads the main menu scene
    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
