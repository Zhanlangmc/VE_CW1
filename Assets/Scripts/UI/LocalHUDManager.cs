using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

//public class LocalHUDManager : MonoBehaviour
//{
//    // 这个 Text 组件用于显示本地玩家得分
//    public Text scoreText;
//    // 本地玩家的 Score 脚本引用，需要在玩家生成后赋值
//    public Score localScore;


//    private void Update()
//    {
//        if (localScore != null && scoreText != null)
//        {
//            scoreText.text = "Score: " + localScore.score;
//        }
//    }
//}


public class LocalHUDManager : MonoBehaviour
{
    public Text scoreText; // 在 Inspector 中拖入 LocalHUD 中的 Text 组件
    private Score localScore;

    // 此方法用于绑定玩家的 Score 数据
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
        }
    }

    private void Update()
    {
        // 每帧更新，如果你希望分数实时更新（也可以通过事件驱动）
        UpdateScoreUI();
    }
}
