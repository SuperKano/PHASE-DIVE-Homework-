using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NoteSpawner : MonoBehaviour
{
    [Header("노트 프리팹")]
    public GameObject normalNotePrefab;
    public GameObject blueFxPrefab;
    public GameObject redFxPrefab;

    [Header("레인 연결")]
    public Lane[] laneSpawners; // 일반 0~3
    public Lane fxLeftSpawner;  // FX L
    public Lane fxRightSpawner; // FX R

    // noteSpawnY 변수는 삭제함 (각 레인 판정선 기준으로 자동 계산됨)

    Queue<NoteData> noteQueue = new Queue<NoteData>();

    public bool isChartFinished = false; // 노트 생성이 끝났는지 확인 하는 함수.    GameManager로 전달

    struct NoteData
    {
        public float time;
        public int lane;
        public string type;
        public float length;
    }

    void Start()
    {
        LoadChart();
    }

    void LoadChart()
    {
        TextAsset chartData = Resources.Load<TextAsset>("rhythmNote"); 
        if (chartData == null)
        {
            Debug.LogError("차트 파일을 찾을 수 없음. Resources/rhythmNote.txt 확인");
            return;
        }

        string[] lines = chartData.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)   // 0번째 줄은 헤더
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] data = line.Split(',');
            if (data.Length < 4) continue;

            float beat = float.Parse(data[0]);
            int lane = int.Parse(data[1]);
            string type = data[2].Trim();
            float length = float.Parse(data[3]);

            float noteTime = beat * GameManager.Instance.secPerBeat; // 기본 비트 - 초 변환
            noteTime += GameManager.Instance.globalOffset / 1000f;  // 전역 오프셋 적용 (밀리초 - 초)

            NoteData newNote = new NoteData
            {
                time = noteTime,
                lane = lane,
                type = type,
                length = length * GameManager.Instance.secPerBeat
            };

            noteQueue.Enqueue(newNote);
        }
    }

    void Update()
    {
        float musicTime = GameManager.Instance.songTime; // 수월한 플레이를 위해서 대기 시간에도 노트 로드

        float spawnCheckTime = musicTime + GameManager.Instance.noteSpawnOffset;    //  현재 시간 + 낙하 시간(Offset) = 생성해야 할 노트 시간. 2초 뒤에 칠 노트를 지금 미리 만듦

        while (noteQueue.Count > 0 && noteQueue.Peek().time <= spawnCheckTime)  // if 대신 while 사용함 (한 프레임에 여러 노트 생성 가능하게)
        {
            SpawnNote(noteQueue.Dequeue());
        }

        if (noteQueue.Count == 0 && !isChartFinished) // 노트 카운트가 다 지났는지(곡이 끝났는지) 확인
        {
            isChartFinished = true;
        }
    }
    private void SpawnNote(NoteData data)
    {
        GameObject prefabToUse = null;
        Lane targetLaneScript = null;

        // 1. 타입별 프리팹 및 타겟 레인 설정함
        if (data.type == "L")
        {
            prefabToUse = blueFxPrefab;
            targetLaneScript = fxLeftSpawner;
        }
        else if (data.type == "R")
        {
            prefabToUse = redFxPrefab;
            targetLaneScript = fxRightSpawner;
        }
        else // 일반 노트
        {
            prefabToUse = normalNotePrefab;
            targetLaneScript = laneSpawners[data.lane];
        }

        // 생성 위치(Y) 자동 계산함
        // 판정선 위치 + 고속도로 거리(13.63) = 생성 위치
        // 만약 일반/FX 판정선이 달라도 도착 시간이 똑같아짐
        float calculatedSpawnY = targetLaneScript.judgeLineY + GameManager.Instance.highwayDistance;

        Vector3 spawnPos = new Vector3(targetLaneScript.transform.position.x, calculatedSpawnY, 0);

        GameObject newNoteObj = Instantiate(prefabToUse, spawnPos, Quaternion.identity);

        Note noteScript = newNoteObj.GetComponent<Note>();

        ScoreManager.Instance.IncrementTotalNotes(); // 총 노트 개수 증가(결과창)
        ScoreManager.Instance.IncrementActiveNotes(); // 활성되어 있는 노트 수 분석(원활한 플레이 종료를 위해서)

        noteScript.SetLane(targetLaneScript); // 소속 레인 등록함

        if (data.length > 0)
        {
            noteScript.SetLength(data.length);
        }
    }
}