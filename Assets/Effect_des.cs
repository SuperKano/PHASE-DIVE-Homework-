using UnityEngine;

public class Effect_des : MonoBehaviour
{
    public float delayTime = 0.5f; // 이펙트 0.5초 뒤 삭제
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, delayTime); // 지정된 시간이 지날 경우 삭제
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
