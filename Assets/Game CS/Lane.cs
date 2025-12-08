using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Lane : MonoBehaviour
{
    public List<Note> notes = new List<Note>();

    private Lane thisLane; // 자기 자신 참조용
    public float perfectTime = 0.080f;  // 80ms, 시간 참조
    public float goodTime = 0.150f; // 150ms
    public float missTime = 0.250f;// 250ms

    [Header("오브젝트 연결")]
    public Transform judgeLineTransform;

    [Header("이펙트 연결")]
    public SpriteRenderer laneLight;
    public SpriteRenderer laneFlare;

    [Header("★ 롱노트 이펙트 연결")]
    public GameObject holdEffectPrefab; // 롱노트용 루핑 이펙트 프리팹 연결
    private GameObject currentHoldEffectObj; // 현재 켜져있는 이펙트 저장용
    public Vector3 holdEffectOffset = new Vector3(0, 0, 0); // 이펙트 위치 변경

    public Vector3 hitEffectOffset = new Vector3(0, 0, 0); // 이펙트 위치 변경(단타)

    [Header("판정 범위 설정")]
    public float perfectDist = 0.15f; // 초록
    public float goodDist = 0.28f;    // 노랑
    public float missDist = 0.5f;    // 빨강 (이 범위 안에서 잘못 치면 Miss, 밖으로 나가도 Miss)

    [Header("디버그")]
    public bool showDebugGizmos = true;

    // NoteSpawner 접근용
    public float judgeLineY => judgeLineTransform != null ? judgeLineTransform.position.y : -3.0f;

    // 내부 변수
    private Note currentLongNote = null;
    private float holdTimer = 0f;
    private Color defaultColor = new Color(1f, 1f, 1f, 0.2f);
    private Color pressColor = new Color(1f, 1f, 1f, 1.0f);

    void Start()
    {
        thisLane = this;
        if (laneLight != null) laneLight.color = defaultColor;
        if (laneFlare != null) { Color c = laneFlare.color; c.a = 0f; laneFlare.color = c; }

        if (judgeLineTransform == null)
            Debug.LogError($"[Lane] {gameObject.name}에 판정선이 연결되지 않음");
    }

    void Update()
    {
        // Miss 노트를 안 치고 흘려보냈을 때
        if (notes.Count > 0)
        {
            Note firstNote = notes[0];
            if (firstNote == null) notes.RemoveAt(0);
            else
            {
                float multiplier = 1.0f;
                if (GameManager.Instance != null) // Late Miss에도 보정 추가
                    multiplier = GameManager.Instance.speedMultiplier;

                // missDist * multiplier 적용
                if (!firstNote.isHit && firstNote.GetHeadY() < judgeLineY - (missDist * multiplier))
                {
                    // 판정선보다 (missDist * 배속)만큼 아래로 내려가면 놓친 것으로 처리
                    Debug.Log($"<color=red>[Miss]</color> {gameObject.name} (Late)");
                    if (ScoreManager.Instance != null) ScoreManager.Instance.ProcessHit("Miss", transform.position);
                    ScoreManager.Instance.DamageGauge(0.5f); // 게이지 감소

                    notes.RemoveAt(0);

                    if (firstNote.GetComponent<SpriteRenderer>())
                        firstNote.GetComponent<SpriteRenderer>().color = Color.gray;

                    // Late Miss도 카운트 감소 필수 (게임 종료 처리를 위해)
                    if (ScoreManager.Instance != null) ScoreManager.Instance.DecrementActiveNotes();
                }
            }
        }

        // 롱노트 처리
        if (currentLongNote != null)
        {
            if (currentLongNote.GetTailY() <= judgeLineY - 0.2f)
                CompleteLongNote();
            else
            {
                holdTimer += Time.deltaTime;
                if (holdTimer >= 0.1f)
                {
                    holdTimer = 0f;
                    if (ScoreManager.Instance != null) ScoreManager.Instance.ProcessHoldTick();
                }
            }
        }
    }

    //키를 눌렀을 때의 판정 로직
    public void CheckHit()
    {
        while (notes.Count > 0 && notes[0] == null) notes.RemoveAt(0);
        if (notes.Count == 0) return;

        Note targetNote = notes[0];
        if (targetNote.isHit) return;

        float distance = Mathf.Abs(targetNote.GetHeadY() - judgeLineY);
        float noteSpeed = GameManager.Instance.scrollSpeed; // 초당 떨어지는 유닛 수
        float perfectThreshold = perfectTime * noteSpeed; // 거리에서 초 단위로 변경
        float goodThreshold = goodTime * noteSpeed;
        float missThreshold = missTime * noteSpeed;
        // 이펙트가 터질 고정 위치 계산
        Vector3 hitPos = judgeLineTransform.position;

        // 1. Perfect
        if (distance <= perfectThreshold)
        {
            Debug.Log($"<color=cyan>PERFECT</color> (오차: {distance:F4})");

            if (ScoreManager.Instance != null)
                ScoreManager.Instance.ProcessHit("Perfect", hitPos);

            ProcessNoteHit(targetNote);
        }
        // 2. Good
        else if (distance <= goodThreshold)
        {
            Debug.Log($"<color=yellow>GOOD</color> (오차: {distance:F4})");

            if (ScoreManager.Instance != null)
                ScoreManager.Instance.ProcessHit("Good", hitPos);

            ProcessNoteHit(targetNote);
        }
        // Early Miss (너무 빨리 침)
        else if (distance <= missThreshold)
        {
            Debug.Log($"<color=red>MISS (Early)</color> (오차: {distance:F4})");

            if (ScoreManager.Instance != null)
                ScoreManager.Instance.ProcessHit("Miss", hitPos);
                ScoreManager.Instance.DamageGauge(0.5f); // 게이지 감소

            if (notes.Contains(targetNote)) notes.Remove(targetNote);
            if (ScoreManager.Instance != null) ScoreManager.Instance.DecrementActiveNotes(); // miss, Early Miss로 노트 삭제될 시 카운트 감소(원활한 종료)
            Destroy(targetNote.gameObject);
        }
        else
        {
            // 범위 밖 (무시)
        }
    }

    public void PressKey()
    {
        if (laneLight != null) laneLight.color = pressColor;
        if (laneFlare != null) { Color c = laneFlare.color; c.a = 1f; laneFlare.color = c; }
        CheckHit();
    }
    public void ReleaseKey()
    {
        if (laneLight != null) laneLight.color = defaultColor;
        if (laneFlare != null) { Color c = laneFlare.color; c.a = 0f; laneFlare.color = c; }
        StopHoldEffect(); // 키를 뗄 경우 이펙트 종료
        if (currentLongNote != null)
        {
            currentLongNote.isHolding = false;
            if (currentLongNote.GetComponent<SpriteRenderer>())
                currentLongNote.GetComponent<SpriteRenderer>().color = Color.gray;
            currentLongNote.myLane.RemoveNote(currentLongNote);
            ScoreManager.Instance.DecrementActiveNotes();
            currentLongNote = null;
            if (ScoreManager.Instance != null) ScoreManager.Instance.ProcessHit("Miss", transform.position);
        }
    }
    void ProcessNoteHit(Note note)
    {
        if (note.isLongNote)
        {
            note.isHit = true;
            note.isHolding = true;
            currentLongNote = note;
            holdTimer = 0f;
            if (notes.Contains(note)) notes.Remove(note);
            note.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
            PlayHoldEffect(); // 롱노트 처리시 롱노트용 이펙트 시작
        }
        else // 단타 히트
        {
            if (notes.Contains(note)) notes.Remove(note);
            if (ScoreManager.Instance != null) ScoreManager.Instance.DecrementActiveNotes(); // 노트가 처리되어 삭제될 시 카운트 감소(원활한 종료)
            Destroy(note.gameObject);
        }
    }
    void CompleteLongNote() // 롱노트 히트
    {
        StopHoldEffect(); // 롱노트 다 처리시(완벽 처리) 이펙트 종료
        if (currentLongNote == null) return;
        if (ScoreManager.Instance != null) ScoreManager.Instance.ProcessHit("Perfect", currentLongNote.transform.position);
        ScoreManager.Instance.DecrementActiveNotes(); // 노트가 처리되어 삭제될 시 카운트 감소(원활한 종료)
        Destroy(currentLongNote.gameObject);
        currentLongNote = null;
    }
    void PlayHoldEffect()
    {
        if (holdEffectPrefab != null && currentHoldEffectObj == null)
        {
            // 판정선 위치에 이펙트 생성
            currentHoldEffectObj = Instantiate(holdEffectPrefab, judgeLineTransform.position, judgeLineTransform.rotation);
            currentHoldEffectObj.transform.SetParent(judgeLineTransform);
            currentHoldEffectObj.transform.localPosition = Vector3.zero;

        }
    }
    void StopHoldEffect()
    {
        if (currentHoldEffectObj != null)
        {
            Destroy(currentHoldEffectObj);
            currentHoldEffectObj = null;
        }
    }

    public void RegisterNote(Note note) { notes.Add(note); }
    public void RemoveNote(Note note) { if (notes.Contains(note)) notes.Remove(note); }
    
    void OnDrawGizmos() // 판정 범위를 쉽게 구별하기 위해 만든 표시
    {
        // 기본 체크: 디버그가 꺼져있거나 판정선이 없으면 그리지 않음
        if (!showDebugGizmos || judgeLineTransform == null) return;

        float currentY = judgeLineTransform.position.y;
        Vector3 center = transform.position;
        float zPos = -1f; // 스프라이트보다 앞에 그리기 위해 Z축 조정

        // 위아래 대칭으로 그리기 위해 중심을 currentY로 잡고 높이를 missDist * 2로 설정
        Gizmos.color = new Color(1, 0, 0, 0.3f); // Miss 판정 범위 (빨간색 박스)
        Gizmos.DrawWireCube(new Vector3(center.x, currentY, zPos), new Vector3(1f, missDist * 2, 0)); 

        Gizmos.color = Color.yellow; // Good 판정 범위 (노란색 박스)
        Gizmos.DrawWireCube(new Vector3(center.x, currentY, zPos), new Vector3(1f, goodDist * 2, 0));

        Gizmos.color = Color.green; // Perfect 판정 범위 (초록색 박스)
        Gizmos.DrawWireCube(new Vector3(center.x, currentY, zPos), new Vector3(1f, perfectDist * 2, 0)); // 가장 중요하니 나중에 그려서 위에 덮음

        Gizmos.color = Color.red; // 판정선 중심 (빨간 실선)
        Gizmos.DrawLine(new Vector3(center.x - 0.5f, currentY, zPos), new Vector3(center.x + 0.5f, currentY, zPos));

        float dist = 0f; // 노트 생성 위치 (하늘색 선) - 싱크 확인용(노트가 내려오는 위치)
        if (Application.isPlaying && GameManager.Instance != null)
        {
            dist = GameManager.Instance.highwayDistance;
        }
        else // 에디터 상태일 때 임시로 찾음
        {
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null) dist = gm.highwayDistance;
        }

        if (dist > 0)
        {
            float spawnY = currentY + dist;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(new Vector3(center.x - 0.5f, spawnY, zPos), new Vector3(center.x + 0.5f, spawnY, zPos));
        }
    }
}