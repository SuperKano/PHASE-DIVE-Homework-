using UnityEngine;

public static class ResultDataHolder // Static 클래스 (씬이 바뀌어도 메모리에 유지)
{
    public static int finalScore = 0; // 최종 점수
    public static int maxCombo = 0; // 최대 콤보
    public static int perfectCount = 0; // 퍼펙 수
    public static int goodCount = 0; // 굿 수
    public static int missCount = 0; // 하즈레 수
}