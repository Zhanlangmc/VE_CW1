using UnityEngine;

public class bgmManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public AudioClip bgmClip;      // ����������Ƶ�ļ�
    public float volume = 0.5f;    // Ĭ������������ Inspector �е���

    private AudioSource audioSource;

    void Start()
    {
        // ���AudioSource��������û�еĻ�
        audioSource = gameObject.AddComponent<AudioSource>();

        // ����AudioSource����
        audioSource.clip = bgmClip;
        audioSource.loop = true;             // ѭ������
        audioSource.volume = Mathf.Clamp(volume, 0f, 1f);
        audioSource.playOnAwake = true;      // �Զ�����

        // ���ű�������
        audioSource.Play();
    }
}
