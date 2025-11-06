using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAutoFitPortrait : MonoBehaviour
{
    public float referenceAspect = 9f / 16f; // 0.5625
    public float paddingCells = 0.5f;        // 여백(칸 단위)
    public float biasY = 0f;                 // 화면 내 상하 치우침
    public int extraTopSafeAreaPx = 0;       // 노치/펀치홀 보정

    [Header("Zoom tuning")]
    [Tooltip("1보다 크면 더 멀리(보드가 더 작게). 예: 1.2~1.5 추천")]
    public float zoomOutFactor = 1.2f;

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    public void FitToBoard(int N, float cellSize = 1f)
    {
        float board = N * cellSize;
        Vector3 center = new(board * 0.5f, -board * 0.5f, -10f);

        float aspect = (float)Screen.width / Screen.height;
        float refAsp = Mathf.Max(0.01f, referenceAspect);

        // 기본 세로 기준 + 9:16 보호 + 추가 줌아웃
        float halfH = (board * 0.5f) + paddingCells * cellSize;
        float scale = Mathf.Max(1f, refAsp / aspect);
        float ortho = halfH * scale * zoomOutFactor;

        cam.orthographicSize = ortho;

        // ★ 픽셀 정확도용: 카메라 실제 픽셀 높이 사용
        float worldPerPixel = (cam.orthographicSize * 2f) / cam.pixelHeight;

        // 노치 보정(픽셀→월드)
        float safeOffsetY = -0.5f * extraTopSafeAreaPx * worldPerPixel;

        // 바이어스
        float viewH = cam.orthographicSize;
        Vector3 biasOffset = new(0f, viewH * biasY * 0.3f, 0f);

        // 우선 원하는 위치로 배치
        Vector3 pos = cam.transform.position;

        // ★ 카메라를 1px 월드 단위로 스냅(반올림) → 그리드 선 깨짐 방지
        pos.x = Mathf.Round(pos.x / worldPerPixel) * worldPerPixel;
        pos.y = Mathf.Round(pos.y / worldPerPixel) * worldPerPixel;

        cam.transform.position = pos;
    }
}
