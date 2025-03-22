using UnityEngine;
using UnityEngine.UI;

public class LocalHUDManager : MonoBehaviour
{
    // ��� Text ���������ʾ������ҵ÷�
    public Text scoreText;
    // ������ҵ� Score �ű����ã���Ҫ��������ɺ�ֵ
    public Score localScore;

    private void Update()
    {
        if (localScore != null && scoreText != null)
        {
            scoreText.text = "Score: " + localScore.score;
        }
    }
}
