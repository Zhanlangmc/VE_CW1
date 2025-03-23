using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

//public class LocalHUDManager : MonoBehaviour
//{
//    // ��� Text ���������ʾ������ҵ÷�
//    public Text scoreText;
//    // ������ҵ� Score �ű����ã���Ҫ��������ɺ�ֵ
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
    public Text scoreText; // �� Inspector ������ LocalHUD �е� Text ���
    private Score localScore;

    // �˷������ڰ���ҵ� Score ����
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
        // ÿ֡���£������ϣ������ʵʱ���£�Ҳ����ͨ���¼�������
        UpdateScoreUI();
    }
}
