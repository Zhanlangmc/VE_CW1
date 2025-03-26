using UnityEngine;

public class bgmManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public AudioClip bgmClip;
    public float volume = 0.1f;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        // Configuring AudioSource Parameters
        audioSource.clip = bgmClip;
        audioSource.loop = true; // Loop Playback
        audioSource.volume = Mathf.Clamp(volume, 0f, 1f);
        audioSource.playOnAwake = true; // Autoplay

        // Play background music
        audioSource.Play();
    }
}
