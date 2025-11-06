using UnityEngine;

public class BoardState
{
    public int N;
    public TileType[] tiles;
    public Vector2Int Start, End;
    public bool HasKey;

    public bool InBounds(int x, int y) => (uint)x < (uint)N && (uint)y < (uint)N;

    public TileType Get(int x, int y)
    {
        // 디버그 시 어떤 좌표가 튀는지 보려면 로그 잠깐 켜도 됨
        // if (!InBounds(x,y)) Debug.LogWarning($"Out of bounds: ({x},{y}) N={N}");
        return tiles[GridUtil.Idx(x, y, N)];
    }

    // 안전 버전: 경계 밖은 벽처럼 취급하고 싶다면
    public TileType SafeGet(int x, int y) => InBounds(x, y) ? Get(x, y) : TileType.BLOCK;
}
