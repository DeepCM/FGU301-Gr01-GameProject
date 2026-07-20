using UnityEngine;
using UnityEngine.Audio;

namespace Game.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Mixer - keo Mixer asset vao day")]
        [SerializeField] private AudioMixer mixer;

        [Header("Audio Sources - tao 2 GameObject con, gan AudioSource vao")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        // Ten 2 tham so PHAI trung voi ten "Exposed Parameter" trong Audio Mixer
        private const string MUSIC_PARAM = "MusicVolume";
        private const string SFX_PARAM = "SFXVolume";
        private const string MUSIC_PREF_KEY = "musicVolume";
        private const string SFX_PREF_KEY = "sfxVolume";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // Giu AudioManager song qua moi scene, khong bi mat nhac khi doi scene
            DontDestroyOnLoad(gameObject);

            LoadVolumeSettings();
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource.clip == clip && musicSource.isPlaying) return;
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            // PlayOneShot cho phep nhieu SFX chong len nhau ma khong cat am
            // dang phat, khac voi Play() se bi ngat am dang chay
            sfxSource.PlayOneShot(clip);
        }

        public void SetMusicVolume(float value01)
        {
            mixer.SetFloat(MUSIC_PARAM, LinearToDecibel(value01));
            PlayerPrefs.SetFloat(MUSIC_PREF_KEY, value01);
        }

        public void SetSFXVolume(float value01)
        {
            mixer.SetFloat(SFX_PARAM, LinearToDecibel(value01));
            PlayerPrefs.SetFloat(SFX_PREF_KEY, value01);
        }

        // Audio Mixer lam viec theo thang dB (logarit), khong phai 0-1 tuyen tinh.
        // Neu gan thang value01 vao SetFloat, slider se cam giac "khong tuyen tinh":
        // keo tu 100% xuong 50% gan nhu khong nghe khac biet, roi tu 50% xuong 0%
        // moi thay am luong giam manh. Ham nay convert dung cach.
        private float LinearToDecibel(float value01)
        {
            return value01 <= 0.0001f ? -80f : Mathf.Log10(value01) * 20f;
        }

        private void LoadVolumeSettings()
        {
            float music = PlayerPrefs.GetFloat(MUSIC_PREF_KEY, 0.75f);
            float sfx = PlayerPrefs.GetFloat(SFX_PREF_KEY, 0.75f);
            SetMusicVolume(music);
            SetSFXVolume(sfx);
        }
    }
}
