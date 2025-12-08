using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; 

public class ResultDisplay : MonoBehaviour
{
    // 인스펙터에 UI 요소를 연결
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI perfectText;
    public TextMeshProUGUI goodText;
    public TextMeshProUGUI missText;
    public TextMeshProUGUI maxComboText;

    void Start()
    {
        // 씬이 로드될 때 ResultDataHolder에 저장된 데이터를 가져와 표시
        scoreText.text = ResultDataHolder.finalScore.ToString("N0");
        perfectText.text = ResultDataHolder.perfectCount.ToString();
        goodText.text = ResultDataHolder.goodCount.ToString();
        missText.text = ResultDataHolder.missCount.ToString();
        maxComboText.text = ResultDataHolder.maxCombo.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // 스페이스바를 누룰 시 타이틀로
        {
            GoToTitle();
        }
    }
    public void GoToTitle()
    {
        SceneManager.LoadScene("Title");
    }
}
