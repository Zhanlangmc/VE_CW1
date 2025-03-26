using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;


public class LocalHUDManager : MonoBehaviour
{
    public Text scoreText;
    private Score localScore;

    // This method is used to bind the player's Score data
    public void SetPlayer(Score score)
    {
        localScore = score;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null && localScore != null)
        {
            scoreText.text = "Score: " + localScore.score;
            // scoreText.text = "Score: 9";
            // Debug.Log("Updating HUD: Score = " + localScore.score);
        }
        //if (localScore != null)
        //{
        //    Debug.Log($"当前 HUD 绑定的 Score 实例：{localScore.gameObject.name}，分数：{localScore.score}");
        //}
    }

    private void Update()
    {
        // Update every frame, if you want the score to update in real time (can also be event driven)
        UpdateScoreUI();
    }
}
