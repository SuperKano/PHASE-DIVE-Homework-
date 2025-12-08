using EasyTextEffects;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public AudioSource musicSource;

    [Header("UI 연결")]
    public GameObject pausePanel;
    public Slider speedSlider;
    public TextMeshProUGUI speedText;

    public float[] allowedSpeeds = { 1.0f, 1.5f, 2.0f, 2.5f, 3.0f, 3.5f, 4.0f, 4.5f, 5.0f };
    private int currentSpeedIndex = 0;

    private bool isPaused = false;
    public bool IsPaused => isPaused; // private를 유지하며 isPaused 액세스


    [Header("대기 설정")]
    public float startDelay = 3.0f;

    [Header("기본 설정")]
    public float bpm = 148f;

    [Header("배속 설정")]
    public float speedMultiplier = 1.0f;

    [Header("시작 대기 UI")]
    public TextMeshProUGUI readyText;
    public AnimationCurve scaleCurve;
    public AnimationCurve fadeCurve;

    [Header("싱크 미세 조정")]
    public float globalOffset = 0f;

    [Header("노트 생성 높이 지정")]
    public float highwayDistance = 27.9f;

    [Header("노트의 일정한 속도 지정")]
    public float baseBasicSpeed = 10.0f;

    [Header("플레이 타임 표시")]
    public float songTime;

    [Header("게임오버 연출")]
    public GameObject diveFailPanel;
    public TextMeshProUGUI diveFailText;
    public float gameOverDelay = 2.0f;  // 결과 씬으로 넘어가는 대기 시간

    private bool isGameOver = false;

    [Header("배경 영상")] // 일시정지를 위해
    public VideoPlayer backgroundVideoPlayer;

    [HideInInspector] public float scrollSpeed;
    [HideInInspector] public float noteSpawnOffset;
    [HideInInspector] public float secPerBeat;

    private NoteSpawner noteSpawner;

    void Awake()
    {
        Instance = this;
        secPerBeat = 60f / bpm;
        if (musicSource == null) musicSource = GetComponent<AudioSource>();
        CalculateSpeed();
    }

    void Start()
    {
        songTime = -startDelay; // 시작 시간을 -3초로 설정
        noteSpawner = FindAnyObjectByType<NoteSpawner>();

        if (readyText != null)
        {
            readyText.gameObject.SetActive(false); // 초기에는 비활성화
            readyText.color = new Color(readyText.color.r, readyText.color.g, readyText.color.b, 0f);
            readyText.transform.localScale = Vector3.zero;
        }

        currentSpeedIndex = 0;
        speedMultiplier = allowedSpeeds[0];
        if (speedSlider != null)
        {
            speedSlider.minValue = 1.0f;
            speedSlider.maxValue = 5.0f;
            speedSlider.value = speedMultiplier;
            speedSlider.onValueChanged.RemoveAllListeners();
            speedSlider.onValueChanged.AddListener(OnSpeedSliderChanged);
        }
        if (PlayerPrefs.HasKey("SavedSpeedMultiplier")) // 배속 저장
        {
            speedMultiplier = PlayerPrefs.GetFloat("SavedSpeedMultiplier");
        }
        else
        {
            speedMultiplier = 1.0f;  // 기본값
        }

        currentSpeedIndex = Mathf.Clamp( Mathf.RoundToInt((speedMultiplier - 1.0f) / 0.5f),0, allowedSpeeds.Length - 1);

        if (speedSlider != null)
        {
            speedSlider.value = speedMultiplier;
        }
        UpdateSpeedUI();
        CalculateSpeed();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }

        if (isPaused) return;

        if (musicSource.isPlaying)
        {
            songTime = musicSource.time;
            if (readyText != null && readyText.gameObject.activeSelf) readyText.gameObject.SetActive(false);
        }
        else
        {
            if (songTime < 0)
            {
                if (!isPaused)  // 일시정지 시 대기 시간 멈춤
                {
                    songTime += Time.deltaTime;
                }

                if (readyText != null) // 레디 텍스트 애니메이션
                {
                    readyText.gameObject.SetActive(true);
                    readyText.text = "Ready";

                    float normalizedTime = (startDelay + songTime) / startDelay;
                    float animProgress = Mathf.Clamp01(normalizedTime);

                    float scale = scaleCurve.Evaluate(animProgress);
                    readyText.transform.localScale = Vector3.one * scale;

                    float alpha = fadeCurve.Evaluate(animProgress);
                    readyText.color = new Color(readyText.color.r, readyText.color.g, readyText.color.b, alpha);
                }

                if (songTime >= 0)
                {
                    songTime = 0;
                    musicSource.Play();
                    if (readyText != null) readyText.gameObject.SetActive(false);
                }
            }
            else
            {
                if (noteSpawner != null && noteSpawner.isChartFinished) // 노래가 끝난 후 노트 및 활성화된 노트 수 점검
                {
                    if (ScoreManager.Instance != null && ScoreManager.Instance.activeNoteCount <= 0) //  모든 노트 생성 완료, 화면에 활성 노트가 0개
                    {
                        QuitToResult();
                    }
                }
            }
            if (speedSlider != null && Mathf.Abs(speedSlider.value - speedMultiplier) > 0.01f)
            {
                // 0.5 단위 스냅 로직
                float rawValue = speedSlider.value;
                int newIndex = Mathf.RoundToInt((rawValue - 1.0f) / 0.5f);
                newIndex = Mathf.Clamp(newIndex, 0, allowedSpeeds.Length - 1);

                speedMultiplier = allowedSpeeds[newIndex];
                speedSlider.value = speedMultiplier;  // 핸들 강제 스냅 
                UpdateSpeedUI();
                CalculateSpeed();
            }
        }
    }

    public void TogglePause() // 토글 UI
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f; // 멈춤
            musicSource.Pause();
            pausePanel.SetActive(true);

            CanvasGroup cg = pausePanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = pausePanel.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;

            if (backgroundVideoPlayer != null) // 영상 정지
            {
                backgroundVideoPlayer.Pause();
            }
        }
        else
        {
            Time.timeScale = 1f;
            musicSource.UnPause();
            pausePanel.SetActive(false);

            if (backgroundVideoPlayer != null)
            {
                backgroundVideoPlayer.Play();
            }
        }
    }
    public void OnSpeedSliderChanged(float rawValue)
    {
        int newIndex = Mathf.RoundToInt((rawValue - 1.0f) / 0.5f);
        newIndex = Mathf.Clamp(newIndex, 0, allowedSpeeds.Length - 1);

        if (newIndex != currentSpeedIndex)
        {
            currentSpeedIndex = newIndex;
            speedMultiplier = allowedSpeeds[currentSpeedIndex];

            speedSlider.value = speedMultiplier;  // 스냅

            UpdateSpeedUI();
            CalculateSpeed();

            Debug.Log($"배속 변경 → {speedMultiplier}x");
        }

        PlayerPrefs.SetFloat("SavedSpeedMultiplier", speedMultiplier);
        PlayerPrefs.Save();  // 즉시 저장

        speedSlider.value = speedMultiplier; // 핸들 위치 강제 유지 
    }

    private void UpdateSpeedUI()
    {
        if (speedText != null)
            speedText.text = $"{speedMultiplier:F1}x";
    }

    public void CalculateSpeed()
    {
        scrollSpeed = baseBasicSpeed * speedMultiplier;
        noteSpawnOffset = highwayDistance / scrollSpeed;
    }

    public void ResumeGame() { TogglePause(); }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToResult()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.CalculateFinalResultAndQuit();

        Time.timeScale = 1f;
        SceneManager.LoadScene("ResultScene");
    }
    public void QuitToTitle()
    {
        Time.timeScale = 1f;  // 타이틀 씬 애니메이션 정상 작동
        SceneManager.LoadScene("Title");  
    }
    public void GameOverDiveFail()
    {
        if (isGameOver) return;
        isGameOver = true;

        Time.timeScale = 0f;
        musicSource.Pause();

        // 패널과 텍스트 초기 상태
        if (diveFailPanel != null)
        {
            CanvasGroup panelCG = diveFailPanel.GetComponent<CanvasGroup>();
            if (panelCG == null) panelCG = diveFailPanel.AddComponent<CanvasGroup>();
            panelCG.alpha = 0f;  // 초기 투명
            diveFailPanel.SetActive(true);
        }

        if (diveFailText != null)
        {
            diveFailText.text = "DIVE:FAIL";
            diveFailText.color = new Color(diveFailText.color.r, diveFailText.color.g, diveFailText.color.b, 0f);  // 투명으로 시작
            diveFailText.transform.localScale = Vector3.zero;  // 초기 크기 0

            diveFailText.gameObject.SetActive(true); // 텍스트는 활성화 상태 유지

            TextEffect textEffect = diveFailText.GetComponent<TextEffect>(); // 에셋 효과 강제 재시작
            if (textEffect != null)
            {
                textEffect.StartManualEffects();  // 재활성화로 효과 즉시 시작                                  
            }
        }

        StartCoroutine(DiveFailFadeInSequence());
    }

    private IEnumerator DiveFailFadeInSequence()
    {
        CanvasGroup panelCG = diveFailPanel.GetComponent<CanvasGroup>();

        float fadeTime = 0f; 
        float fadeDuration = 1.0f; // 패널 서서히 페이드인
        while (fadeTime < fadeDuration)
        {
            fadeTime += Time.unscaledDeltaTime;
            if (panelCG != null)
            {
                panelCG.alpha = Mathf.Lerp(0f, 1f, fadeTime / fadeDuration);
            }
            yield return null;
        }
        if (panelCG != null) panelCG.alpha = 1f;

        yield return new WaitForSecondsRealtime(0.3f);  // 패널 이후 텍스트 등장 
        if (diveFailText != null)
        {
            float textAnimTime = 0f;
            float textDuration = 1.0f;
            Vector3 targetScale = Vector3.one * 1f;
            Color targetColor = diveFailText.color;
            targetColor.a = 1f;  // 완전 불투명

            while (textAnimTime < textDuration)
            {
                textAnimTime += Time.unscaledDeltaTime;
                float progress = textAnimTime / textDuration;

                diveFailText.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, progress);
                diveFailText.color = Color.Lerp(new Color(targetColor.r, targetColor.g, targetColor.b, 0f), targetColor, progress);

                yield return null;
            }

            diveFailText.transform.localScale = targetScale;
            diveFailText.color = targetColor;  // 최종 색상
        }

        yield return new WaitForSecondsRealtime(5.0f); // 최종 대기 후 결과 씬
        QuitToTitle();
    }
}