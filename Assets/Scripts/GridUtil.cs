using UnityEngine;

public static class GridUtil
{
    // ====== 프로젝트 전역 앵커 기준 ======
    // (0,0)=셀 좌상단, (0.5,0.5)=정중앙, (1,1)=셀 우하단
    public static Vector2 ANCHOR = new Vector2(0.5f, 0.5f); // ★ 지금 네가 눈으로 맞춘 기준

    // 우리 게임: x 오른쪽+, y 아래쪽+  ←→  유니티 타일맵: x 오른쪽+, y 위쪽+
    public static Vector3Int ToUnityCell(Vector2Int gameCell)
        => new Vector3Int(gameCell.x, -gameCell.y, 0);

    public static Vector2Int ToGameCell(Vector3Int unityCell)
        => new Vector2Int(unityCell.x, -unityCell.y);

    public static Vector2Int WorldToCell(Grid grid, Vector3 world)
        => ToGameCell(grid.WorldToCell(world));

    // ✅ 셀 안 임의 앵커(0~1)의 "월드 좌표"를 일관되게 계산
    public static Vector3 CellAnchor(Grid grid, Vector2Int gameCell, Vector2 anchor01)
    {
        // 1) 게임셀 → 유니티셀( y 부호 반전 포함 )
        var uc = ToUnityCell(gameCell);

        // 2) 그 셀의 '정중앙' 월드 좌표
        var center = grid.GetCellCenterWorld(uc);

        // 3) 중앙 기준 오프셋
        //    x: (anchor-0.5)*cellSize.x  (0=왼쪽, 1=오른쪽)
        //    y: (0.5-anchor)*cellSize.y  (게임은 아래가 +이므로 부호 반전)
        var off = new Vector3(
            (anchor01.x - 0.5f) * grid.cellSize.x,
            (0.5f - anchor01.y) * grid.cellSize.y,
            0f
        );

        return center + off;
    }

    /// <summary>
    /// 현재 프로젝트 전역 앵커(=ANCHOR)를 적용한 좌표
    /// </summary>
    public static Vector3 CellPos(Grid grid, Vector2Int gameCell)
        => CellAnchor(grid, gameCell, ANCHOR);

    public static int Idx(int x, int y, int N) => y * N + x;

    public static bool InBounds(Vector2Int c, int N)
        => c.x >= 0 && c.y >= 0 && c.x < N && c.y < N;
}
