using UnityEngine;

public class GridCursor : MonoBehaviour
{
    public Grid grid;
    public SpriteRenderer sr;
    public Color ok = new(0f, 1f, 0.5f, 0.25f);
    public Color bad = new(1f, 0f, 0f, 0.25f);
    public BoardState board;

    Camera cam;

    void Awake() 
    { 
        cam = Camera.main ?? FindFirstObjectByType<Camera>();
        if (sr == null) sr = GetComponent<SpriteRenderer>(); // 자동 할당    
    }

    void LateUpdate()
    {
        if (board == null || grid == null) return;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        // 포커스 없으면 무시 (에디터 전환/창 밖 등)
        if (!Application.isFocused) return;

        // 입력 좌표 얻기
        Vector3 p = Input.touchCount > 0
            ? (Vector3)Input.touches[0].position
            : (Vector3)Input.mousePosition;

        // 카메라 픽셀 영역 밖이면 처리하지 않음
        var rect = cam.pixelRect;
        if (!rect.Contains(new Vector2(p.x, p.y))) return;

        // 방어적 체크
        if (float.IsNaN(p.x) || float.IsNaN(p.y) || float.IsInfinity(p.x) || float.IsInfinity(p.y)) return;

        // 월드 변환 (직교카메라라 z는 의미 없음)
        var w = cam.ScreenToWorldPoint(new Vector3(p.x, p.y, 0f));
        w.z = 0f;

        var c = GridUtil.WorldToCell(grid, w);
        transform.position = GridUtil.CellToWorld(grid, c);
        sr.color = board.InBounds(c.x, c.y) ? ok : bad;
    }


    Vector2Int WorldToCell(Vector3 world)
    {
        return GridUtil.WorldToCell(grid, world); // ← GridUtil로 단일화
    }
}
