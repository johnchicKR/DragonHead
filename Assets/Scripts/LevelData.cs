using UnityEngine;

[System.Serializable]
public class LevelPack
{
    public string pack_id;
    public LevelData[] levels;
}

[System.Serializable]
public class LevelData
{
    public string level_id;
    public int size;
    public int[] tiles;
    public int par_moves;
    public string theme;
}

public enum TileType
{
    EMPTY = 0, BLOCK = 1, TRAP = 2, KEY = 3, LOCK = 4, START = 5, END = 6
}
