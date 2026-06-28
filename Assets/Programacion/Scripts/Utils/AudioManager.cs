using UnityEngine;
using UnityEngine.Audio;

[DefaultExecutionOrder(-100)]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField]
    private AudioMixerGroup masterGroup;
    [SerializeField]
    public AudioSource sfx, music, voice;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(AudioClip clip)
    {
        sfx.PlayOneShot(clip);
    }

    public void PlayLoopSound(AudioClip sound)
    {
        sfx.clip = sound;
        sfx.loop = true;
        sfx.Play();
    }

    public void StopLoopSound()
    {
        sfx.Stop();
        sfx.clip = null;
        sfx.loop = false;
    }
}
