using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class VideoAssets : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string videoFileName = "121651-724710483_small.mp4";

    // Start()에서 Coroutine 시작
    void Start()
    {
        StartCoroutine(LoadAndPlayVideo());
    }

    IEnumerator LoadAndPlayVideo()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        string videoPath = Application.streamingAssetsPath + "/" + videoFileName;  // WebGL에 적합한 URL 경로 생성 (Path.Combine 대신 + "/" 사용)

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoPath;

        
        videoPlayer.Prepare();   // 비디오 준비 

      
        while (!videoPlayer.isPrepared)   // 준비가 완료될 때까지 대기
        {
            yield return null;
        }

        Debug.Log("[Video] Preparation complete. Playing video.");
        videoPlayer.Play();
    }

    void Update()
    {
        
    }
}
