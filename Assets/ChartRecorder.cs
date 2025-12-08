using System.Collections.Generic;
using System.IO; // 파일 저장을 위해 필요
using UnityEngine;

public class ChartRecorder : MonoBehaviour
{
    [Header("시각적 피드백")]
    public GameObject previewNotePrefab;  
    public Lane[] laneSpawners;

    [Header("설정")]
    public AudioSource musicSource;
    public float bpm = 148f; // 만들려는 곡의 BPM을 정확히 기재
    public string saveFileName = "MyNewChart"; // 저장될 파일 이름 (.txt 자동추가)

    [Header("상태")]
    public bool isRecording = false;
    private float secPerBeat;

    // 녹음된 노트 데이터를 임시 저장할 리스트
    private List<RecordedNote> recordedNotes = new List<RecordedNote>();

    // 키를 누른 시점을 기억하기 위한 배열 (레인 0~5)
    private float[] pressTimes = new float[6];
    private bool[] isPressing = new bool[6];

    // 내부 데이터 구조
    class RecordedNote
    {
        public float startTime; // 초 단위
        public int lane;
        public string type;     // N, L, R
        public float length;    // 초 단위
    }

    void Start()
    {
        secPerBeat = 60f / bpm; // 1박자의 시간 계산

        // 배열 초기화
        for (int i = 0; i < 6; i++) pressTimes[i] = -1f;

        Debug.Log("★ 스페이스바를 누르면 녹음이 시작");
    }

    void Update()
    {
        // 시작 제어
        if (!isRecording)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isRecording = true;
                musicSource.Play();
                Debug.Log("녹음 시작 (음악 재생 중...)");
            }
            return;
        }

        // 녹음 종료 및 저장 (P키)
        if (Input.GetKeyDown(KeyCode.P))
        {
            SaveChartFile();
            isRecording = false;
            musicSource.Stop();
            return;
        }

        float currentTime = musicSource.time;

        // 키 입력 감지 (0~5번 레인)
        CheckInput(0, KeyCode.D, "N", currentTime);
        CheckInput(1, KeyCode.F, "N", currentTime);
        CheckInput(2, KeyCode.J, "N", currentTime);
        CheckInput(3, KeyCode.K, "N", currentTime);
        CheckInput(4, KeyCode.S, "L", currentTime);
        CheckInput(5, KeyCode.L, "R", currentTime);

        // Undo(BackSpace)
        if (Input.GetKeyDown(KeyCode.Backspace) && recordedNotes.Count > 0)
        {
            RecordedNote lastNote = recordedNotes[recordedNotes.Count - 1];
            recordedNotes.RemoveAt(recordedNotes.Count - 1);
            Debug.Log($"Undo: Lane {lastNote.lane} at {lastNote.startTime:F2}s");
        }
    }

    // 입력 체크 함수
    void CheckInput(int laneIndex, KeyCode key, string type, float time)
    {
        // 누르는 순간
        if (Input.GetKeyDown(key))
        {
            pressTimes[laneIndex] = time;  // pressTimes 배열 사용 (s 추가!)
            isPressing[laneIndex] = true;
            Debug.Log($"Input: Lane {laneIndex}");
        }

        // 떼는 순간
        if (Input.GetKeyUp(key))
        {
            if (isPressing[laneIndex])
            {
                float pressTime = pressTimes[laneIndex];  // 여기서 pressTime 선언
                float duration = time - pressTime;

                if (duration < 0.1f) duration = 0f;

                // newNote 여기서 선언
                RecordedNote newNote = new RecordedNote();
                newNote.startTime = pressTime;
                newNote.lane = laneIndex;
                newNote.type = type;
                newNote.length = duration;

                recordedNotes.Add(newNote);
                isPressing[laneIndex] = false;
            }
        }
    }

    void SaveChartFile()
    {
        string path = Application.dataPath + "/" + saveFileName + ".txt";

        // 파일을 쓰기 위한 스트림 생성
        using (StreamWriter writer = new StreamWriter(path))
        {
            // 헤더 작성 (NoteSpawner 형식이랑 같아야 함: beat,lane,type,length)
            writer.WriteLine("Beat,Lane,Type,Length");

            // 시간순 정렬 (혹시 뒤죽박죽일까봐)
            recordedNotes.Sort((a, b) => a.startTime.CompareTo(b.startTime));

            foreach (RecordedNote note in recordedNotes)
            {
                // [중요] 초(Second)를 박자(Beat)로 변환!
                // Spawner는 (beat * secPerBeat)로 시간을 계산하므로,
                // 여기서는 (time / secPerBeat)로 나눠서 저장해야 함.
                float beatPos = note.startTime / secPerBeat;
                float beatLength = note.length / secPerBeat; // 길이도 박자 단위로 변환

                // 소수점 셋째 자리까지 반올림 (깔끔하게 저장 위해)
                beatPos = Mathf.Round(beatPos * 1000f) / 1000f;
                beatLength = Mathf.Round(beatLength * 1000f) / 1000f;

                // 파일 쓰기: beat,lane,type,length
                string line = $"{beatPos},{note.lane},{note.type},{beatLength}";
                writer.WriteLine(line);
            }
        }

        Debug.Log($"<b>[저장 완료]</b> 파일 위치: {path}");
        Debug.Log("이 파일을 Resources 폴더로 옮겨서 사용");
    }
}