using UnityEngine;

public class bgmManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public AudioClip bgmClip;      // 背景音乐音频文件
    public float volume = 0.5f;    // 默认音量，可在 Inspector 中调整

    private AudioSource audioSource;

    void Start()
    {
        // 添加AudioSource组件，如果没有的话
        audioSource = gameObject.AddComponent<AudioSource>();

        // 配置AudioSource参数
        audioSource.clip = bgmClip;
        audioSource.loop = true;             // 循环播放
        audioSource.volume = Mathf.Clamp(volume, 0f, 1f);
        audioSource.playOnAwake = true;      // 自动播放

        // 播放背景音乐
        audioSource.Play();
    }
}
