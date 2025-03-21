using UnityEngine;
using UnityEngine.UI;

public class ScoreText : MonoBehaviour
{
    // 引用玩家自己挂在 Avatar 上的 Score 脚本
    public Score playerScore;

    private Text textComponent;

    private void Awake()
    {
        // 获取挂在当前UI组件上的 Text 组件
        textComponent = GetComponent<Text>();

        if (textComponent == null)
        {
            Debug.LogError("ScoreText 脚本需要挂在带有 Text 组件的 GameObject 上！");
        }
    }

    private void Update()
    {
        // 每帧更新显示玩家的分数
        if (playerScore != null && textComponent != null)
        {
            textComponent.text = "Score: " + playerScore.score;
        }
    }
}
