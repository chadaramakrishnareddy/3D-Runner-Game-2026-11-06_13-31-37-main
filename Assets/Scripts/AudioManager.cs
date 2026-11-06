using UnityEngine;

/// <summary>
/// Manages background desert music and one-shot sound effects for gameplay actions.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip desertMusicClip;
    [SerializeField] private AudioClip coinCollectClip;
    [SerializeField] private AudioClip gemCollectClip;
    [SerializeField] private AudioClip obstacleCrashClip;
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip gameOverClip;

    [Header("Volume Controls")]
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.8f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSources();
    }

    private void Start()
    {
        PlayDesertMusic();
    }

    private void EnsureAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
    }

    public void PlayDesertMusic()
    {
        if (musicSource != null && desertMusicClip != null)
        {
            if (musicSource.clip != desertMusicClip || !musicSource.isPlaying)
            {
                musicSource.clip = desertMusicClip;
                musicSource.volume = musicVolume;
                musicSource.Play();
            }
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void PlayCoinSound()
    {
        PlaySfx(coinCollectClip, 0.7f);
    }

    public void PlayGemSound()
    {
        PlaySfx(gemCollectClip, 0.9f);
    }

    public void PlayObstacleCrashSound()
    {
        PlaySfx(obstacleCrashClip, 1.0f);
    }

    public void PlayButtonClickSound()
    {
        PlaySfx(buttonClickClip, 0.6f);
    }

    public void PlayGameOverSound()
    {
        PlaySfx(gameOverClip, 0.9f);
    }

    private void PlaySfx(AudioClip clip, float volumeScale = 1.0f)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null) musicSource.volume = musicVolume;
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }
}
