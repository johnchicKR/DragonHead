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
        if (!sr) sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (board == null || grid == null) return;
        if (!cam) cam = Camera.main; if (!cam) return;
        if (!Application.isFocused) return;

        Vector3 p = Input.touchCount > 0 ? (Vector3)Input.touches[0].position : (Vector3)Input.mousePosition;
        if (!cam.pixelRect.Contains(new Vector2(p.x, p.y))) return;

        Vector3 w = cam.ScreenToWorldPoint(new Vector3(p.x, p.y, 0f));
        w.z = 0f;

        Vector2Int cell = GridUtil.WorldToCell(grid, w);

        // µø¿œ ±‚¡ÿ æﬁƒø∑Œ Ω∫≥¿
        Vector3 pos = GridUtil.CellPos(grid, cell);
        pos.z = transform.position.z;
        transform.position = pos;

        sr.color = GridUtil.InBounds(cell, board.N) ? ok : bad;
    }
}
