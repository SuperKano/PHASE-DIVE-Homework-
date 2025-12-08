using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI 연결")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI judgeText;

    [Header("이펙트 프리팹")]
    public GameObject hitEffectPrefab; // 히트 이펙트

    [Header("게이지 UI")]
    public Image gaugeBackground;    // 게이지 배경 이미지
    public Image gaugeFill;          // 채우기 이미지 (Filled 타입)
    public float maxGauge = 100f;    // 최대 게이지
    [HideInInspector] public float currentGauge;

    private int currentScore = 0; // 숫자가 변동되는 변수들
    private int currentCombo = 0;

    public int perfectCount = 0; // Perfect 수
    public int goodCount = 0; // Good 수
    public int missCount = 0; // Miss 수
    public int maxCombo = 0; // 최대 콤보 수

    [HideInInspector] public int activeNoteCount = 0; // 플레이 종료 카운트를 위한 변수
    
        void Awake() // Start보다 우선
        {
          Instance = this;
        }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
          currentCombo = 0; // 시작 할 때 마다 초기화
          currentScore = 0;
          // UI 연결이 Inspector에서 되어 있다면, Start에서 초기화는 안전함
          if (judgeText != null) judgeText.text = "";
          if (comboText != null) comboText.text = "";

          currentGauge = maxGauge * 0.05f;  // 초기 5%
          UpdateGaugeUI();
        } 
      

    public void ProcessHit(string judgment, Vector3 notePosition)
    {
        // 게이지 효과
        if (judgment == "Miss")
        {
            currentGauge -= 0.8f;
            currentGauge = Mathf.Max(0f, currentGauge);
        }
        else if (judgment == "Perfect")
        {
            currentGauge += 0.45f;
        }
        else if (judgment == "Good")
        {   
            currentGauge += 0.2f;
        }
        currentGauge = Mathf.Min(currentGauge, maxGauge);

        // 판정 처리
        if (judgment == "Miss")
        {
            currentCombo = 0;
            judgeText.text = "MISS";
            judgeText.color = Color.gray; // 미스
            missCount++;
        }
        else // Perfect 또는 Good
        {
            currentCombo++;

            if (currentCombo > maxCombo) // Perfect, Good 둘 다 콤보 오름
            {
                maxCombo = currentCombo; // 최대 콤보
            }

            if (judgment == "Perfect")
            {
                currentScore += 1000; // Perfect는 1000 
                perfectCount++;

                judgeText.text = "PERFECT";
                judgeText.color = Color.lightBlue;
            }
            else if (judgment == "Good")
            {
                currentScore += 500; // Good은 500
                goodCount++;

                judgeText.text = "GOOD";
                judgeText.color = Color.lightGreen;
            }

            if (hitEffectPrefab != null)   // 이펙트 생성 (노트가 있던 위치에)
            {
                Instantiate(hitEffectPrefab, notePosition, Quaternion.identity);
            }

            // 공통 UI 갱신
            comboText.text = currentCombo.ToString();
            scoreText.text = currentScore.ToString();
            judgeText.transform.localScale = Vector3.one * 1.5f;
            comboText.transform.localScale = Vector3.one * 1.5f;
        }

        if (gaugeFill != null) // 애니메이터 트리거
        {
            Animator anim = gaugeFill.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("Fill"); 
            }
        }
        if (judgment != "Miss" && hitEffectPrefab != null)  // 히트 이펙트 (Perfect/Good에만
        {
            Instantiate(hitEffectPrefab, notePosition, Quaternion.identity);
        }
        
        if (currentGauge <= 0f)
        {
            GameManager.Instance.GameOverDiveFail();  // r
        }
        UpdateGaugeUI(); // UI 업데이트
    }
    public void ProcessHoldTick()
    {
        currentCombo++;
        currentScore += 10; // 롱노트 틱당 점수
        currentGauge += 0.01f; // 롱노트 틱당 게이지

        comboText.text = currentCombo.ToString();  // 텍스트 업데이트
        scoreText.text = currentScore.ToString();
        UpdateGaugeUI(); // UI 업데이트
    }
    public bool AreAllNotesJudged()
    {
        // 현재까지 판정된 노트의 총합
        int judgedNotes = perfectCount + goodCount + missCount;

        // 총 노트 수가 0보다 크고, 생성된 모든 노트가 판정되었을 때
        return totalNotesGenerated > 0 && totalNotesGenerated <= judgedNotes;
    }

    void Update()
    {
        judgeText.transform.localScale = Vector3.Lerp(judgeText.transform.localScale, Vector3.one, Time.deltaTime * 10f); // 텍스트 크기 변동 후 원래대로
        comboText.transform.localScale = Vector3.Lerp(comboText.transform.localScale, Vector3.one, Time.deltaTime * 10f);
    }
    public void IncrementActiveNotes()
    {
        activeNoteCount++;
    }
    public void DecrementActiveNotes()
    {
        activeNoteCount = Mathf.Max(0, activeNoteCount - 1);  // 0 이하로 내려가지 않도록 방지
    }

    public int totalNotesGenerated = 0; // 결과 씬에 전송
    public void IncrementTotalNotes()
    {
        totalNotesGenerated++;
    }
    public void CalculateFinalResultAndQuit()
    {
        // 현재까지 판정된 노트 수 계산
        int judgedNotes = perfectCount + goodCount + missCount;

        // 남은 노트(Unjudged Notes)를 모두 Miss로 처리(플레이 도중 종료를 대비)
        int unjudgedNotes = totalNotesGenerated - judgedNotes;

        if (unjudgedNotes > 0)
        {
            missCount += unjudgedNotes;
        }
        ResultDataHolder.finalScore = currentScore;
        ResultDataHolder.maxCombo = maxCombo;
        ResultDataHolder.perfectCount = perfectCount;
        ResultDataHolder.goodCount = goodCount;
        ResultDataHolder.missCount = missCount;
    }
    void UpdateGaugeUI()
    {
        if (gaugeFill != null)
        {
            gaugeFill.fillAmount = currentGauge / maxGauge;  // 0 ~ 1로 변환
        }
    }
    public void DamageGauge(float amount)
    {
        currentGauge -= amount;
        currentGauge = Mathf.Max(0f, currentGauge);
        UpdateGaugeUI();  // UI 업데이트
    }
}
