using UnityEngine;

public class StartEffect : MonoBehaviour
{
    
    [Header("움직임 설정")] // 움직임의 진폭 (위아래로 움직이는 최대 거리)
    [Tooltip("텍스트가 위 아래로 움직이는 최대 거리 (유니티 유닛)")] // 변수에 대한 설명
    public float amplitude = 5f;

    
    [Tooltip("움직임의 빠르기 (클수록 빠름)")] // 움직임의 속도 (위아래로 움직이는 빠르기)
    public float speed = 1f;

    // 초기 위치 저장을 위한 변수
    private Vector3 initialPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialPosition = transform.position; // 플레이 시 오브젝트의 초기 위치를 저장
    }

    // Update is called once per frame
    void Update()
    {
        float sinValue = Mathf.Sin(Time.time * speed); // 시간에 따른 사인(Sine) 값 계산
        float newY = initialPosition.y + (sinValue * amplitude); // 새로운 Y 좌표 계산
        transform.position = new Vector3(initialPosition.x, newY, initialPosition.z); // 오브젝트 위치 적용(Y값만)
    }
}
