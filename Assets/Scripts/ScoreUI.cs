using UnityEngine;
using UnityEngine.UI; // Legacy UI Text

public class ScoreUI : MonoBehaviour
{
    public Text scoreText; //
    public Score scoreManager; // `Score.cs`

    void Start()
    {
        // selfcheck `Score.cs`
        scoreManager = FindObjectOfType<Score>();
        if (scoreManager == null)
        {
            Debug.LogError("Score script not found! Make sure it's in the scene.");
        }

        // selfcheck `Text (Legacy)`
        scoreText = GameObject.Find("Text (Legacy)")?.GetComponent<Text>();
        if (scoreText == null)
        {
            Debug.LogError("Legacy UI Text element not found! Make sure it exists in the UI.");
        }
    }

    void Update()
    {
        if (scoreText != null && scoreManager != null)
        {
            scoreText.text = "Score: " + scoreManager.score;
        }
    }
}
