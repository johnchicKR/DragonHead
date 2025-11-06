using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelSpawner : MonoBehaviour
{
    [Header("Scene refs")]
    public Grid grid;
    public Tilemap tilemapBase;
    public Tilemap tilemapOverlay;

    [Header("Tiles")]
    public TileBase tileBlock;
    public TileBase tileTrap;
    public TileBase tileKey;
    public TileBase tileLock;

    [Header("Prefabs")]
    public GameObject headPrefab;
    public GameObject tailPrefab;

    public Transform actorsParent; // Hierarchy에 Actors Empty 만들어 연결
    public PathSystem pathSystem;

    [Header("Grid Lines (Tilemap)")]
    public Tilemap tilemapGridLines;   // BoardRoot/Tilemap_GridLines 연결
    [Range(0f, 1f)] public float gridLineAlpha = 0.28f; // 선 투명도
    public Color gridLineColor = Color.white;          // 선 색상 (알파는 위에서 적용)
    public int gridTextureSize = 64;                   // 1칸 텍스처 해상도(64~128)
    public int gridBorderPx = 1;                       // 테두리 픽셀 두께(1px 추천)

    // 캐시
    Tile gridLineTile;                                 // 생성된 타일 보관


    public void Spawn(LevelData data, out BoardState board)
    {
        // 초기화
        tilemapBase.ClearAllTiles();
        tilemapOverlay.ClearAllTiles();
        foreach (Transform c in actorsParent) Destroy(c.gameObject);

        board = new BoardState
        {
            N = data.size,
            tiles = new TileType[data.size * data.size],
            HasKey = false
        };

        // 타일 루프
        for (int y = 0; y < data.size; y++)
        {
            for (int x = 0; x < data.size; x++)
            {
                var t = (TileType)data.tiles[GridUtil.Idx(x, y, data.size)];
                board.tiles[GridUtil.Idx(x, y, data.size)] = t;

                // 좌표 변환 (Tilemap은 y가 위로 +, 우리는 아래로 + 이므로 -y 사용)
                Vector3Int cell = new Vector3Int(x, -y, 0);

                switch (t)
                {
                    case TileType.BLOCK:
                        tilemapBase.SetTile(cell, tileBlock);
                        break;
                    case TileType.TRAP:
                        tilemapOverlay.SetTile(cell, tileTrap);
                        break;
                    case TileType.KEY:
                        tilemapOverlay.SetTile(cell, tileKey);
                        break;
                    case TileType.LOCK:
                        tilemapOverlay.SetTile(cell, tileLock);
                        break;
                    case TileType.START:
                        board.Start = new Vector2Int(x, y);
                        break;
                    case TileType.END:
                        board.End = new Vector2Int(x, y);
                        break;
                }
            }
        }

        // 머리/꼬리 프리팹 배치 (Actors 아래에)
        if (headPrefab != null)
        {
            Instantiate(headPrefab, GridUtil.CellToWorld(grid, board.Start), Quaternion.identity, actorsParent);
        }
        if (tailPrefab != null)
        {
            Instantiate(tailPrefab, GridUtil.CellToWorld(grid, board.End), Quaternion.identity, actorsParent);
        }

        if (pathSystem != null)
        {
            pathSystem.board = board;
            pathSystem.grid = grid;
        }

        // 스폰 끝난 직후
        FindFirstObjectByType<CameraAutoFitPortrait>()?.FitToBoard(data.size, grid.cellSize.x);
        PaintGridLines(data.size);

        var cursor = FindFirstObjectByType<GridCursor>();
        if (cursor != null)
        {
            cursor.board = board;
            cursor.grid = grid;
        }
    }

    void PaintGridLines(int N)
    {
        if (!tilemapGridLines) return;

        if (gridLineTile == null)
        {
            // 알파 적용된 선 색 만들기
            var c = gridLineColor;
            c.a = gridLineAlpha;

            // 스프라이트 및 타일 1회 생성(캐시)
            var sp = GridTileMaker.MakeGridCellSprite(c, Mathf.Max(8, gridTextureSize), Mathf.Max(1, gridBorderPx));
            gridLineTile = GridTileMaker.MakeTile(sp);

            // 정렬 기준 일치
            tilemapGridLines.tileAnchor = Vector3.zero; // (0,0,0)
        }

        tilemapGridLines.ClearAllTiles();

        for (int y = 0; y < N; y++)
        {
            for (int x = 0; x < N; x++)
            {
                tilemapGridLines.SetTile(new Vector3Int(x, -y, 0), gridLineTile);
            }
        }

        // ★ 픽셀 스냅(중요)
        var cam = Camera.main;
        if (cam)
        {
            float wpp = (cam.orthographicSize * 2f) / cam.pixelHeight;
            var p = tilemapGridLines.transform.position;
            p.x = Mathf.Round(p.x / wpp) * wpp;
            p.y = Mathf.Round(p.y / wpp) * wpp;
            tilemapGridLines.transform.position = p;
        }
    }
}
