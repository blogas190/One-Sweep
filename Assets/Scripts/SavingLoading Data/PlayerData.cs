using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public string sceneName;       // e.g. "Level 1-4"
    public int furthestStage;      // e.g. 1
    public int furthestLevel;      // e.g. 4

    /// <summary>
    /// Returns true if the given stage-level has been reached (i.e. is unlocked for selection).
    /// A level is unlocked if it comes strictly before the furthest reached level.
    /// e.g. furthest = 1-4  levels 1-1, 1-2, 1-3 are unlocked; 1-4 and beyond are not.
    /// </summary>
    public bool IsUnlocked(int stage, int level)
    {
        if (stage == 1 && level == 1) return true;
        if (stage < furthestStage) return true;
        if (stage == furthestStage && level <= furthestLevel) return true;
        return false;
    }

    /// <summary>
    /// Parses a scene name like "Level 1-4" and updates furthest stage/level if this is further.
    /// </summary>
    public void UpdateFurthest(string levelSceneName)
    {
        sceneName = levelSceneName;

        // Parse "Level X-Y"
        string stripped = levelSceneName.Replace("Level ", "");
        string[] parts = stripped.Split('-');
        if (parts.Length != 2) return;

        if (!int.TryParse(parts[0], out int stage)) return;
        if (!int.TryParse(parts[1], out int level)) return;

        // Only update if this is further than what we have
        if (stage > furthestStage || (stage == furthestStage && level > furthestLevel))
        {
            furthestStage = stage;
            furthestLevel = level;
        }
    }
}