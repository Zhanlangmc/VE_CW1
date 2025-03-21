using UnityEngine;
using UnityEngine.UI;

public class ScoreText : MonoBehaviour
{
    // ��������Լ����� Avatar �ϵ� Score �ű�
    public Score playerScore;

    private Text textComponent;

    private void Awake()
    {
        // ��ȡ���ڵ�ǰUI����ϵ� Text ���
        textComponent = GetComponent<Text>();

        if (textComponent == null)
        {
            Debug.LogError("ScoreText �ű���Ҫ���ڴ��� Text ����� GameObject �ϣ�");
        }
    }

    private void Update()
    {
        // ÿ֡������ʾ��ҵķ���
        if (playerScore != null && textComponent != null)
        {
            textComponent.text = "Score: " + playerScore.score;
        }
    }
}
