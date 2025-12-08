using UnityEngine;
using UnityEngine.EventSystems;

public class Note : MonoBehaviour
{
    public Lane myLane;

    public bool isLongNote = false;
    public bool isHit = false;
    public bool isHolding = false;

    private float halfHeight = 0f;

    public void SetLane(Lane laneScript)
    {
        myLane = laneScript;
        myLane.RegisterNote(this);
    }

    public void SetLength(float duration)
    {
        if (duration > 0) isLongNote = true;

        // GameManager 속도 가져와서 길이 계산함
        float newHeight = duration * GameManager.Instance.scrollSpeed;

        Vector3 newScale = transform.localScale;
        newScale.y = newHeight;
        transform.localScale = newScale;

        // 위쪽으로 늘어나게 위치 보정함
        transform.position += Vector3.up * (newHeight / 2f);
        halfHeight = newHeight / 2f;
    }

    void Start()
    {
        if (halfHeight == 0)
            halfHeight = transform.localScale.y / 2f;
    }

    public float GetHeadY()
    {
        return transform.position.y - halfHeight;
    }

    public float GetTailY()
    {
        return transform.position.y + halfHeight;
    }

    void Update()
    {
       
        if (!GameManager.Instance.IsPaused) // 일시정지 시 이동 완전히 중단
        {
            transform.Translate(Vector2.down * GameManager.Instance.scrollSpeed * Time.deltaTime);
        }

        if (!isHolding && (transform.position.y + halfHeight) < -20f) // 화면 밖 삭제 로직 (일시정지와 무관하게 처리)
        {
            if (myLane != null) myLane.RemoveNote(this);
            if (ScoreManager.Instance != null) ScoreManager.Instance.DecrementActiveNotes();
            Destroy(gameObject);
        }
    }
}