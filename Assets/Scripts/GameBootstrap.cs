using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    public LevelLoader loader;
    public LevelSpawner spawner;

    void Start()
    {
        var level = loader.Pack.levels[1];            // 1-1 ·Îµå
        spawner.Spawn(level, out var board);
        Debug.Log($"Spawned Level {level.level_id} (N={board.N})");
    }
}
