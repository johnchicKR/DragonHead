using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public TextAsset levelJson;
    public LevelPack Pack { get; private set; }

    void Awake()
    {
        if (levelJson == null)
        {
            Debug.LogError("LevelLoader: levelJson not assigned!");
            return;
        }
        Pack = JsonUtility.FromJson<LevelPack>(levelJson.text);
        if (Pack == null || Pack.levels == null)
        {
            Debug.LogError("LevelLoader: JSON parse failed.");
        }
        else
        {
            Debug.Log($"LevelLoader: Loaded {Pack.levels.Length} levels from {Pack.pack_id}");
        }
    }
}
