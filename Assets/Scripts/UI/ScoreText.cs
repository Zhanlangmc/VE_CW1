using UnityEngine;
using UnityEngine.UI;

public class ScoreText : MonoBehaviour
{
    // Reference the player's own Score script on the Avatar
    public Score playerScore;

    private Text textComponent;

    private void Awake()
    {
        // Get the Text component attached to the current UI component
        textComponent = GetComponent<Text>();

        if (textComponent == null)
        {
            Debug.LogError("The ScoreText script needs to be attached to a GameObject with a Text component!");
        }
    }

    private void Update()
    {
        // Update the player's score every frame
        if (playerScore != null && textComponent != null)
        {
            textComponent.text = "Score: " + playerScore.score;
        }
    }
}
