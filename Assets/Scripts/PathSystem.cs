using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathSystem : MonoBehaviour
{
    [Header("Scene refs")]
    public Grid grid;                 // BoardRoot/Grid
    public LineRenderer line;         // PathInk (LineRenderer)

    [Header("Colors")]
    public Color colorNormal = new(0.3f, 1f, 0.5f, 0.9f);
    public Color colorFail = new(1f, 0.3f, 0.2f, 0.9f);
    public Color colorKey = new(1f, 1f, 0.2f, 0.9f);

    [HideInInspector] public BoardState board;

    Camera cam;
    readonly List<Vector2Int> pathCells = new();
    readonly List<Vector3> worldPts = new();
    HashSet<int> visited = new();
    bool drawing;
    bool hasKey;

    void Awake()
    {
        cam = Camera.main ?? FindFirstObjectByType<Camera>();
        if (!line) line = GetComponent<LineRenderer>();
        if (line)
        {
            line.useWorldSpace = true;
            line.positionCount = 0;
        }
    }

    void Update()
    {
        if (board == null || grid == null || line == null) return;
        if (!cam) cam = Camera.main;

        // 입력 픽셀 좌표
        bool down = Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began);
        bool hold = Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.touches[0].phase != TouchPhase.Ended);

        Vector3 p = Input.touchCount > 0 ? (Vector3)Input.touches[0].position : (Vector3)Input.mousePosition;
        if (!cam.pixelRect.Contains(new Vector2(p.x, p.y))) return;

        Vector3 w = cam.ScreenToWorldPoint(new Vector3(p.x, p.y, 0f));
        w.z = 0f;
        Vector2Int cell = GridUtil.WorldToCell(grid, w);

        if (down) BeginDraw(cell);
        if (drawing && hold) ContinueDraw(cell);
        if (!hold && drawing) EndDraw();
    }

    void BeginDraw(Vector2Int startCell)
    {
        // 시작은 반드시 START 위에서
        if (!GridUtil.InBounds(startCell, board.N)) return;
        if (GetTile(startCell) != TileType.START) return;

        drawing = true;
        hasKey = board.HasKey; // 시작 시 키 보유 여부
        pathCells.Clear();
        worldPts.Clear();
        visited.Clear();

        pathCells.Add(startCell);
        visited.Add(Hash(startCell));
        AddWorldPoint(startCell);

        line.colorGradient = MakeSolid(colorNormal);
        line.positionCount = worldPts.Count;
        line.SetPositions(worldPts.ToArray());

        Debug.Log($"BeginDraw at {startCell}");
    }

    void ContinueDraw(Vector2Int cell)
    {
        if (!drawing) return;
        if (pathCells.Count == 0) return;

        Vector2Int last = pathCells[pathCells.Count - 1];
        if (cell == last) return;

        // 4방향 이웃만 허용
        if (!IsNeighbor4(last, cell)) return;

        // 보드 범위
        if (!GridUtil.InBounds(cell, board.N))
        {
            line.colorGradient = MakeSolid(colorFail);
            return;
        }

        // 재방문 금지
        if (visited.Contains(Hash(cell)))
        {
            line.colorGradient = MakeSolid(colorFail);
            return;
        }

        // 타일 판정
        var t = GetTile(cell);
        if (t == TileType.BLOCK || t == TileType.TRAP)
        {
            line.colorGradient = MakeSolid(colorFail);
            return;
        }
        if (t == TileType.LOCK && !hasKey)
        {
            line.colorGradient = MakeSolid(colorFail);
            return;
        }

        // 진행 확정
        pathCells.Add(cell);
        visited.Add(Hash(cell));
        AddWorldPoint(cell);

        // 키 획득 시 색 잠깐 변경
        if (t == TileType.KEY)
        {
            hasKey = true;
            line.colorGradient = MakeSolid(colorKey);
        }
        else
        {
            line.colorGradient = MakeSolid(colorNormal);
        }

        line.positionCount = worldPts.Count;
        line.SetPosition(worldPts.Count - 1, worldPts[worldPts.Count - 1]);

        // 성공 체크
        if (t == TileType.END)
        {
            Debug.Log("Success!");
            drawing = false;
        }
    }

    void EndDraw()
    {
        drawing = false;
        // 성공이 아니면 색 유지(또는 리셋 로직을 원하는 대로)
        // Debug.Log("EndDraw");
    }

    // === helpers ===

    void AddWorldPoint(Vector2Int cell)
    {
        var p = GridUtil.CellPos(grid, cell); // ★ GridUtil과 동일 기준 사용
        worldPts.Add(p);
    }

    bool IsNeighbor4(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx + dy) == 1; // 상하좌우 중 하나
    }

    TileType GetTile(Vector2Int c)
    {
        if (!GridUtil.InBounds(c, board.N)) return TileType.BLOCK;
        int i = GridUtil.Idx(c.x, c.y, board.N);
        return board.tiles[i];
    }

    int Hash(Vector2Int c) => (c.y << 16) ^ c.x;

    Gradient MakeSolid(Color c)
    {
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(c, 0f), new GradientColorKey(c, 1f) },
            new[] { new GradientAlphaKey(c.a, 0f), new GradientAlphaKey(c.a, 1f) }
        );
        return g;
    }
}
