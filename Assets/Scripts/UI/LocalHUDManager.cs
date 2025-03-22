using UnityEngine;
using UnityEngine.UI;

public class LocalHUDManager : MonoBehaviour
{
    // 这个 Text 组件用于显示本地玩家得分
    public Text scoreText;
    // 本地玩家的 Score 脚本引用，需要在玩家生成后赋值
    public Score localScore;

    private void Update()
    {
        if (localScore != null && scoreText != null)
        {
            scoreText.text = "Score: " + localScore.score;
        }
    }
}
