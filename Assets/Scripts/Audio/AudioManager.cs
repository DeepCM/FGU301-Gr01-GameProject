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

        [Header("Centralized Audio Engine Clips")]
        [SerializeField] private AudioClip clickClip;
        [SerializeField, Range(0f, 1f)] private float clickVolume = 0.5f;

        [SerializeField] private AudioClip hurtClip;
        [SerializeField, Range(0f, 1f)] private float hurtVolume = 0.6f;

        [SerializeField] private AudioClip jumpClip;
        [SerializeField, Range(0f, 1f)] private float jumpVolume = 0.5f;

        [SerializeField] private AudioClip pauseClip;
        [SerializeField, Range(0f, 1f)] private float pauseVolume = 0.5f;

        [SerializeField] private AudioClip lowHealthClip;
        [SerializeField, Range(0f, 1f)] private float lowHealthVolume = 0.4f;

        [SerializeField] private AudioSource lowHealthSource;

        private bool isLowHealthActive = false;

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

        private void Start()
        {
            // Create procedural clip fallbacks if no clips are assigned in inspector
            if (clickClip == null) clickClip = CreateProceduralClip(600f, 0.05f, "Click");
            if (hurtClip == null) hurtClip = CreateProceduralClip(120f, 0.3f, "Hurt");
            if (jumpClip == null) jumpClip = CreateProceduralClip(300f, 0.25f, "Jump");
            if (pauseClip == null) pauseClip = CreateProceduralClip(220f, 0.4f, "Pause");
            if (lowHealthClip == null) lowHealthClip = CreateScrambleClip();

            if (lowHealthSource == null)
            {
                lowHealthSource = gameObject.AddComponent<AudioSource>();
                lowHealthSource.playOnAwake = false;
                lowHealthSource.loop = true;
            }
            lowHealthSource.clip = lowHealthClip;
            lowHealthSource.volume = lowHealthVolume;
        }

        private void Update()
        {
            // Automatically adjust pitch and volume parameters over time when low health is active
            if (isLowHealthActive && lowHealthSource != null)
            {
                // Rapidly sweep pitch and modulate volume for a glitched scramble effect
                lowHealthSource.pitch = 1.5f + Mathf.PingPong(Time.time * 12f, 1.2f) + Random.Range(-0.08f, 0.08f);
                lowHealthSource.volume = lowHealthVolume * (0.5f + Mathf.PingPong(Time.time * 18f, 0.5f));
            }
        }

        // ==== Centralized Sound Event Triggers ====
        public void PlayClick()
        {
            if (sfxSource != null && clickClip != null)
                sfxSource.PlayOneShot(clickClip, clickVolume);
        }

        public void PlayHurt()
        {
            if (sfxSource != null && hurtClip != null)
                sfxSource.PlayOneShot(hurtClip, hurtVolume);
        }

        public void PlayJump()
        {
            if (sfxSource != null && jumpClip != null)
                sfxSource.PlayOneShot(jumpClip, jumpVolume);
        }

        public void PlayPause()
        {
            if (sfxSource != null && pauseClip != null)
                sfxSource.PlayOneShot(pauseClip, pauseVolume);
        }

        public void SetLowHealthActive(bool active)
        {
            if (isLowHealthActive == active) return;
            isLowHealthActive = active;

            if (lowHealthSource != null)
            {
                if (active)
                {
                    lowHealthSource.Play();
                }
                else
                {
                    lowHealthSource.Stop();
                }
            }
        }

        // Procedural Audio Generators
        private AudioClip CreateProceduralClip(float frequency, float duration, string clipName)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * (1f - (t / duration));
            }
            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateScrambleClip()
        {
            int sampleRate = 44100;
            int sampleCount = sampleRate; // 1 second loop
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                // High frequency scramble sawtooth waveform
                samples[i] = (Mathf.Repeat(t * 1200f, 2f) - 1f) * 0.3f;
            }
            AudioClip clip = AudioClip.Create("LowHealthScramble", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        // ==== Legacy support to avoid breaking existing UI or scenes ====
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
            sfxSource.PlayOneShot(clip);
        }

        public void SetMusicVolume(float value01)
        {
            if (mixer != null)
                mixer.SetFloat(MUSIC_PARAM, LinearToDecibel(value01));
            PlayerPrefs.SetFloat(MUSIC_PREF_KEY, value01);
        }

        public void SetSFXVolume(float value01)
        {
            if (mixer != null)
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
