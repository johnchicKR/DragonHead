using UnityEngine;
using UnityEngine.Tilemaps;

public static class GridTileMaker
{
    /// <summary>
    /// 격자 1칸짜리 스프라이트 생성. 외곽선 1px, 내부 투명.
    /// pixelsPerUnit를 size와 동일하게 맞춰 1:1 매칭.
    /// </summary>
    public static Sprite MakeGridCellSprite(Color lineColor, int size = 64, int border = 1)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        var clear = new Color(0, 0, 0, 0);
        var px = new Color[size * size];

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                bool edge = (x < border) || (x >= size - border) || (y < border) || (y >= size - border);
                px[y * size + x] = edge ? lineColor : clear;
            }

        tex.SetPixels(px);
        tex.Apply();

        var rect = new Rect(0, 0, size, size);
        var pivot = new Vector2(0.5f, 0.5f);
        return Sprite.Create(tex, rect, pivot, pixelsPerUnit: size);
    }

    public static Tile MakeTile(Sprite sp)
    {
        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sp;
        return tile;
    }
}
