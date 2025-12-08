using UnityEngine;
using UnityEngine.UI;      
using System.Collections;

public class Pulsing : MonoBehaviour
{
    public Image targetImage;       // 연결할 이미지 컴포넌트
    public float pulseDuration = 1.0f; // 1회 파장 지속 시간(초)
    public float maxScale = 1.5f;     // 최대 크기 (1.0에서 1.5배까지 커짐)
    public float waitTime = 0.1f;     // 다음 파장이 시작되기 전 대기 시간

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        if (targetImage != null)  // Image 컴포넌트가 활성화되었는지 확인하고 코루틴 시작
        {
            targetImage.enabled = true;
            StartCoroutine(PulseRoutine());
        }
    }
    IEnumerator PulseRoutine()
    {
        while (true) // 무한 반복
        {
            float timer = 0f;
            Vector3 startScale = Vector3.one;
            Vector3 endScale = Vector3.one * maxScale;
            Color startColor = targetImage.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f); // 투명하게 만듦

            // 1. 스케일 키우기 및 페이드 아웃
            while (timer < pulseDuration)
            {
                timer += Time.deltaTime;
                float t = timer / pulseDuration; // 0에서 1로 부드럽게 증가하는 값

                // 크기 증가 (Scale)
                targetImage.transform.localScale = Vector3.Lerp(startScale, endScale, t);

                // 투명도 감소 (Alpha)
                targetImage.color = Color.Lerp(startColor, endColor, t);

                yield return null; // 다음 프레임까지 대기
            }

            // 2. 초기 상태로 리셋 및 대기
            targetImage.transform.localScale = startScale; // 크기 즉시 원복
            targetImage.color = startColor;                 // 색상 즉시 원복 (다시 불투명하게)

            // 다음 파장이 퍼지기 전 잠깐 대기
            yield return new WaitForSeconds(waitTime);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
