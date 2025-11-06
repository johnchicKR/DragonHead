using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathSystem : MonoBehaviour
{
    [Header("Scene refs")]
    public Grid grid;
    public LineRenderer line;

    [Header("Runtime state (assigned by LevelSpawner)")]
    public BoardState board;   // LevelSpawner.Spawn() 끝에서 세팅해줄 것

    [Header("Colors")]
    public Color colorNormal = new Color(0.47f, 0.88f, 0.56f, 1f); // 초록
    public Color colorFail = new Color(1f, 0.33f, 0.33f, 1f);     // 빨강
    public Color colorKey = new Color(1f, 0.84f, 0.2f, 1f);      // 노랑

    [Header("Debug")]
    public bool allowStartAnywhere = true;  // 일단 true로 테스트

    bool drawing;
    readonly List<Vector2Int> path = new();
    readonly HashSet<Vector2Int> visited = new();

    Camera cam;

    void Awake() { cam = Camera.main; }

    void Update()
    {
        if (board == null) return;

        if (PointerDown())
            BeginDraw();

        if (drawing)
            ContinueDraw();

        if (PointerUp())
            EndDraw();
    }

    // --- 입력 공용 유틸 ---
    Vector3 PointerWorld()
    {
        Vector3 p = (Input.touchCount > 0) ? (Vector3)Input.touches[0].position : Input.mousePosition;
        var w = cam.ScreenToWorldPoint(p);
        w.z = 0;
        return w;
    }
    bool PointerDown() => (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began) || Input.GetMouseButtonDown(0);
    bool PointerUp() => (Input.touchCount > 0 && (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)) || Input.GetMouseButtonUp(0);

    // --- 드로우 루프 ---
    void BeginDraw()
    {
        var cell = WorldToCell(PointerWorld());
        if (!board.InBounds(cell.x, cell.y)) return;

        // allowStartAnywhere 옵션 쓰고 있든 아니든, 시작하면 바로 1포인트 찍음
        if (allowStartAnywhere || cell == board.Start)
        {
            drawing = true;
            path.Clear(); visited.Clear();
            path.Add(cell); visited.Add(cell);

            if (line)
            {
                line.useWorldSpace = true;   // 안전장치
                line.sortingOrder = 50;     // 타일 위
                line.positionCount = 1;
                line.SetPosition(0, CellToWorld(cell));
                line.startColor = line.endColor = colorNormal;
            }
            Debug.Log($"BeginDraw at {cell}");


            // ★ 이웃 4칸 타입 로깅
            Vector2Int[] dirs = { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };
            foreach (var d in dirs)
            {
                var nb = cell + d;
                string t = board.InBounds(nb.x, nb.y) ? board.Get(nb.x, nb.y).ToString() : "OUT";
                Debug.Log($" neighbor {nb} = {t}");
            }
        }
    }


    void ContinueDraw()
    {
        var cell = WorldToCell(PointerWorld());
        // 셀 좌표가 바뀌었을 때만, 그리고 인접(맨해튼 1칸)일 때만 시도
        if (cell != path[^1] &&
            Mathf.Abs(cell.x - path[^1].x) + Mathf.Abs(cell.y - path[^1].y) == 1)
        {
            TryStep(cell);
        }
        // 대각/멀리 이동은 무시 (보간 없음)
    }


    void EndDraw()
    {
        if (!drawing) return;
        drawing = false;

        if (path.Count == 0) return;
        if (path[^1] != board.End)
            Fail("not reached");
    }

    void TryStep(Vector2Int cell)
    {
        if (path.Count == 0) return;

        if (!board.InBounds(cell.x, cell.y)) return;               // 바깥: 무시
        var last = path[^1];
        if (Mathf.Abs(cell.x - last.x) + Mathf.Abs(cell.y - last.y) != 1) return; // 비인접: 무시
        if (visited.Contains(cell)) return;                         // 재방문: 무시

        var t = board.Get(cell.x, cell.y);
        if (t == TileType.BLOCK) return;                            // 벽: 무시
        if (t == TileType.LOCK && !board.HasKey) { Fail("need key"); return; } // 잠금만 실패

        path.Add(cell);
        visited.Add(cell);
        RenderPath();

        if (t == TileType.KEY) { board.HasKey = true; FlashLine(colorKey, 0.15f); }
        else if (t == TileType.TRAP) { Fail("trap"); return; }      // 진짜 함정만 실패

        if (cell == board.End) Success();
    }

    // --- 렌더/연출 ---
    void RenderPath()
    {
        line.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
            line.SetPosition(i, CellToWorld(path[i]));
    }

    void FlashLine(Color c, float t)
    {
        StopAllCoroutines();
        StartCoroutine(FlashCo(c, t));
    }
    System.Collections.IEnumerator FlashCo(Color c, float t)
    {
        var a = line.startColor; var b = line.endColor;
        line.startColor = line.endColor = c;
        yield return new WaitForSeconds(t);
        line.startColor = a; line.endColor = b;
    }

    void Success()
    {
        drawing = false;
        Debug.Log("Success!");
        // TODO: 꼬리 흡수 파티클/사운드
        // 간단히: 0.8초 후 다음 레벨 로드 등
    }

    void Fail(string reason)
    {
        drawing = false;
        Debug.Log($"Fail: {reason}");
        // 실패 연출: 라인을 빨갛게 -> 잠시 뒤 리셋
        line.startColor = line.endColor = colorFail;
        Invoke(nameof(ClearPath), 0.3f);
        board.HasKey = false; // 키 초기화
    }

    void ClearPath()
    {
        line.positionCount = 0;
        path.Clear();
        visited.Clear();
        line.startColor = line.endColor = colorNormal;
    }

    // --- 좌표 변환(단일 소스: GridUtil) ---
    Vector3 CellToWorld(Vector2Int cell) => GridUtil.CellToWorld(grid, cell);
    Vector2Int WorldToCell(Vector3 world) => GridUtil.WorldToCell(grid, world);


}
