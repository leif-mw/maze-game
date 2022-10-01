using System;
using UnityEngine;
using TMPro;

public class Stopwatch : MonoBehaviour
{
    private const float MAX_TIME = 3599.999f;
    
    [SerializeField]
    private TMP_Text stopwatchText;

    private float currentTime;

    public static float finalTime;
    

    // Start is called before the first frame update
    void Start()
    {
        currentTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        TimeSpan time;

        if(currentTime > MAX_TIME)
            time = TimeSpan.FromSeconds(MAX_TIME);
        else
            time = TimeSpan.FromSeconds(currentTime);

        stopwatchText.text = time.ToString(@"mm\:ss\:fff");
    }

    // This function is called when the behaviour becomes disabled
    void OnDisable()
    {
        finalTime = currentTime;
    }
}
