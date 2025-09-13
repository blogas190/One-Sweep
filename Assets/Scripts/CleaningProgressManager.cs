using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class CleaningProgressManager : MonoBehaviour
{
    public static CleaningProgressManager Instance { get; private set; }

    // Track both active and destroyed dirt spots
    private Dictionary<DirtSpot, float> activeDirtSpotProgress = new Dictionary<DirtSpot, float>();
    public float destroyedDirtSpotsProgress = 0f; // Count of fully cleaned dirt spots
    public int totalDirtSpots = 0;

    [Header("Performance Settings")]
    public float updateInterval = 0.05f; // Update more frequently for responsive feedback
    private float lastUpdateTime;
    private float cachedTotalProgress = 0f;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Register all existing dirt spots
        DirtSpot[] allDirtSpots = FindObjectsOfType<DirtSpot>();
        foreach (DirtSpot dirtSpot in allDirtSpots)
        {
            RegisterDirtSpot(dirtSpot);
        }

        // Initial progress calculation
        UpdateTotalProgress();
    }

    void Update()
    {
        // Update total progress for responsive UI
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateTotalProgress();
            lastUpdateTime = Time.time;
        }
    }

    public void RegisterDirtSpot(DirtSpot dirtSpot)
    {
        if (!activeDirtSpotProgress.ContainsKey(dirtSpot))
        {
            activeDirtSpotProgress.Add(dirtSpot, 0f);
            totalDirtSpots++;
        }
    }

    public void UnregisterDirtSpot(DirtSpot dirtSpot)
    {
        if (activeDirtSpotProgress.ContainsKey(dirtSpot))
        {
            // When a dirt spot is destroyed, add its progress to the destroyed count
            destroyedDirtSpotsProgress += 1f; // Each destroyed dirt spot counts as 1.0 (100%)
            activeDirtSpotProgress.Remove(dirtSpot);
        }
    }

    public void UpdateDirtSpotProgress(DirtSpot dirtSpot, float cleanPercentage)
    {
        if (activeDirtSpotProgress.ContainsKey(dirtSpot))
        {
            activeDirtSpotProgress[dirtSpot] = Mathf.Clamp01(cleanPercentage);
        }
    }

    private void UpdateTotalProgress()
    {
        if (totalDirtSpots == 0)
        {
            cachedTotalProgress = 0f;
            return;
        }

        // Remove null references (destroyed dirt spots that weren't properly unregistered)
        var keysToRemove = activeDirtSpotProgress.Keys.Where(key => key == null).ToList();
        foreach (var key in keysToRemove)
        {
            // Count these as fully cleaned
            destroyedDirtSpotsProgress += 1f;
            activeDirtSpotProgress.Remove(key);
        }

        // Calculate total progress: sum of active progress + destroyed dirt spots
        float sumActiveProgress = activeDirtSpotProgress.Values.Sum();
        float totalProgress = sumActiveProgress + destroyedDirtSpotsProgress;

        cachedTotalProgress = (totalProgress / totalDirtSpots) * 100f;

        // Clamp to 0-100%
        cachedTotalProgress = Mathf.Clamp(cachedTotalProgress, 0f, 100f);
    }

    public float GetTotalCleaningPercentage()
    {
        return cachedTotalProgress;
    }

    public int GetTotalDirtSpots()
    {
        return totalDirtSpots;
    }

    public int GetRemainingDirtSpots()
    {
        return activeDirtSpotProgress.Count;
    }

    public int GetFullyCleanedDirtSpots()
    {
        return Mathf.RoundToInt(destroyedDirtSpotsProgress);
    }

    // Debug method to see detailed progress
    public string GetDetailedProgress()
    {
        float activeSum = activeDirtSpotProgress.Values.Sum();
        return $"Active: {activeSum:F2}, Destroyed: {destroyedDirtSpotsProgress:F2}, Total: {totalDirtSpots}";
    }

    public void Reset()
    {
        // Clear all progress data
        activeDirtSpotProgress.Clear();
        destroyedDirtSpotsProgress = 0f;
        totalDirtSpots = 0;
        cachedTotalProgress = 0f;
        lastUpdateTime = 0f;

        // Re-initialize with current dirt spots
        Invoke("InitializeForCurrentScene", 0.1f);
    }
}