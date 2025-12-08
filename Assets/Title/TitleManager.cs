using UnityEngine;
using UnityEngine.SceneManagement; 
using TMPro; 

public class TitleManager : MonoBehaviour
{
    [Header("설정")]
    public string gameSceneName = "Main"; // 이동할 씬 

    [Header("UI 연결")]
    public TextMeshProUGUI pressText; // 깜빡거릴 텍스트

    private float time;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // 스페이스바 누르면 시작
        {
            SceneManager.LoadScene(gameSceneName);
        }

        if (pressText != null) // 2. 텍스트 깜빡임 효과(강조)
        {
            time += Time.deltaTime;
            float alpha = Mathf.PingPong(time, 0.5f) + 0.5f; // 0.5초마다 투명도가 바뀌게 설정
            pressText.color = new Color(pressText.color.r, pressText.color.g, pressText.color.b, alpha);
        }
    }
}
