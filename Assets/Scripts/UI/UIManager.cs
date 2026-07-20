using UnityEngine;
using Game.Events;

namespace Game.UI
{
    public enum GameUIState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Victory,
        Settings
    }

    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panels - keo GameObject panel tuong ung vao day")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("HUD elements")]
        [SerializeField] private UnityEngine.UI.Slider healthSlider;
        [SerializeField] private TMPro.TextMeshProUGUI scoreText;

        [Header("Settings elements")]
        [SerializeField] private UnityEngine.UI.Slider musicVolumeSlider;
        [SerializeField] private UnityEngine.UI.Slider sfxVolumeSlider;

        private GameUIState currentState;
        // Luu lai state truoc khi mo Settings, de bam Close biet quay lai dau
        // (vi Settings co the mo tu MainMenu hoac tu Pause, khong co dinh 1 noi)
        private GameUIState stateBeforeSettings;

        private void Awake()
        {
            // Singleton co ban - chi cho phep 1 UIManager ton tai
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            GameEvents.OnHealthChanged += HandleHealthChanged;
            GameEvents.OnScoreChanged += HandleScoreChanged;
            GameEvents.OnPlayerDied += HandlePlayerDied;
            GameEvents.OnPlayerWin += HandlePlayerWin;
        }

        private void OnDisable()
        {
            // Bat buoc phai huy dang ky, khong thi memory leak va loi
            // "MissingReferenceException" khi doi scene
            GameEvents.OnHealthChanged -= HandleHealthChanged;
            GameEvents.OnScoreChanged -= HandleScoreChanged;
            GameEvents.OnPlayerDied -= HandlePlayerDied;
            GameEvents.OnPlayerWin -= HandlePlayerWin;
        }

        private void Start()
        {
            SetState(GameUIState.MainMenu);
        }

        private void Update()
        {
            // Bam ESC de pause/resume trong luc choi
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentState == GameUIState.Playing) SetState(GameUIState.Paused);
                else if (currentState == GameUIState.Paused) SetState(GameUIState.Playing);
            }
        }

        public void SetState(GameUIState newState)
        {
            currentState = newState;

            mainMenuPanel.SetActive(newState == GameUIState.MainMenu);
            hudPanel.SetActive(newState == GameUIState.Playing || newState == GameUIState.Paused);
            pausePanel.SetActive(newState == GameUIState.Paused);
            gameOverPanel.SetActive(newState == GameUIState.GameOver);
            victoryPanel.SetActive(newState == GameUIState.Victory);
            settingsPanel.SetActive(newState == GameUIState.Settings);

            // Pause thuc su dung game bang cach dung Time.timeScale
            // Luu y: Settings cung phai dung game neu duoc mo tu trong luc Pause,
            // nhung neu mo tu MainMenu thi khong can dung (chua vao game).
            // Cach don gian: giu nguyen timeScale hien tai, chi doi khi vao/roi Paused.
            if (newState == GameUIState.Paused) Time.timeScale = 0f;
            else if (newState == GameUIState.Playing) Time.timeScale = 1f;
        }

        // ==== Gan cac ham nay vao OnClick() cua Button trong Inspector ====
        public void OnClickPlay() => SetState(GameUIState.Playing);
        public void OnClickResume() => SetState(GameUIState.Playing);
        public void OnClickPause() => SetState(GameUIState.Paused);

        public void OnClickRetry() => UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);

        public void OnClickBackToMenu() => UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");

        public void OnClickQuit() => Application.Quit();

        // Mo Settings - nho lai state hien tai de con quay lai dung cho
        public void OnClickOptions()
        {
            stateBeforeSettings = currentState;
            SetState(GameUIState.Settings);

            // Hien gia tri volume hien tai len slider (doc tu AudioManager/PlayerPrefs)
            if (musicVolumeSlider != null)
                musicVolumeSlider.value = PlayerPrefs.GetFloat("musicVolume", 0.75f);
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = PlayerPrefs.GetFloat("sfxVolume", 0.75f);
        }

        // Dong Settings - quay lai dung state truoc do (MainMenu hoac Paused)
        public void OnClickCloseSettings() => SetState(stateBeforeSettings);

        // Gan 2 ham nay vao OnValueChanged() cua 2 Slider trong SettingsPanel
        public void OnMusicVolumeChanged(float value01)
        {
            if (Game.Audio.AudioManager.Instance != null)
                Game.Audio.AudioManager.Instance.SetMusicVolume(value01);
        }

        public void OnSFXVolumeChanged(float value01)
        {
            if (Game.Audio.AudioManager.Instance != null)
                Game.Audio.AudioManager.Instance.SetSFXVolume(value01);
        }

        // ==== Event handlers - tu dong chay khi co event ban len ====
        private void HandleHealthChanged(int current, int max)
        {
            if (healthSlider == null) return;
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }

        private void HandleScoreChanged(int score)
        {
            if (scoreText == null) return;
            scoreText.text = score.ToString();
        }

        private void HandlePlayerDied() => SetState(GameUIState.GameOver);
        private void HandlePlayerWin() => SetState(GameUIState.Victory);
    }
}