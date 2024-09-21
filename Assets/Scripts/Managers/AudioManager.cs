using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    private AudioSource _musicSource;
    private AudioSource _sfxSource;

    [Range(0f, 1f)]
    [SerializeField] private float _volume;

    [SerializeField] private AudioClip _buttonClickSound;
    [SerializeField] private AudioClip _shootSound;
    [SerializeField] private AudioClip _popSound;
    [SerializeField] private AudioClip _backgroundMusic;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            _musicSource = gameObject.AddComponent<AudioSource>();
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _musicSource.volume = _volume;
            _sfxSource.volume = _volume;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayMusic();
    }

    public void PlayMusic()
    {
        if (_musicSource != null && _backgroundMusic != null)
        {
            _musicSource.clip = _backgroundMusic;
            _musicSource.loop = true;
            _musicSource.Play();
        }
    }

    public void PlayButtonClick()
    {
        if (_sfxSource != null && _buttonClickSound != null)
        {
            _sfxSource.PlayOneShot(_buttonClickSound);
        }
    }

    public void PlayShootSound()
    {
        if (_sfxSource != null && _shootSound != null)
        {
            _sfxSource.PlayOneShot(_shootSound);
        }
    }

    public void PlayPopSound()
    {
        if (_sfxSource != null && _popSound != null)
        {
            _sfxSource.PlayOneShot(_popSound);
        }
    }
    
    
}
