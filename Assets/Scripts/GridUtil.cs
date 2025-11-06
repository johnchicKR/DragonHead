using UnityEngine;

public static class GridUtil
{
    // 게임 좌표계(cell: x 오른쪽 +, y 아래쪽 +) → 유니티 타일맵 좌표계(Vector3Int)
    public static Vector3 CellToWorld(Grid grid, Vector2Int cell)
    {
        // 타일맵은 y 위쪽이 + 이므로 y에 마이너스를 한 뒤, 해당 셀의 "정중앙" 월드 좌표를 받는다.
        return grid.GetCellCenterWorld(new Vector3Int(cell.x, -cell.y, 0));
    }

    // 유니티 월드 좌표 → 게임 좌표계의 셀 (x 오른쪽 +, y 아래쪽 +)
    public static Vector2Int WorldToCell(Grid grid, Vector3 world)
    {
        var c = grid.WorldToCell(world); // 타일맵 기준(위쪽 +) 셀 좌표
        return new Vector2Int(c.x, -c.y); // 게임 기준(아래쪽 +)으로 반전
    }

    public static int Idx(int x, int y, int N) => y * N + x;
}
