using System.IO;
using UnityEditor;
using UnityEngine;

public static class EventChannelAssetGenerator
{
    private const string EventsFolderPath = "Assets/ScriptableObjects/Events";

    private static readonly string[] EventAssetNames =
    {
        "EnemyDetectedEvent",
        "EnemyTouchedPlayerEvent",
        "HealthChangedEvent",
        "SpecialCrateReachedGoalEvent",
        "ExitUnlockedEvent",
        "PlayerDiedEvent",
        "PlayerWonEvent",
        "LevelRestartedEvent"
    };

    [MenuItem("Tools/Generate Game Event Assets")]
    public static void GenerateAssets()
    {
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
        {
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        }

        if (!AssetDatabase.IsValidFolder(EventsFolderPath))
        {
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Events");
        }

        int createdCount = 0;
        foreach (string eventName in EventAssetNames)
        {
            string assetPath = $"{EventsFolderPath}/{eventName}.asset";
            GameEventChannelSO existing = AssetDatabase.LoadAssetAtPath<GameEventChannelSO>(assetPath);
            if (existing == null)
            {
                GameEventChannelSO newAsset = ScriptableObject.CreateInstance<GameEventChannelSO>();
                AssetDatabase.CreateAsset(newAsset, assetPath);
                createdCount++;
                Debug.Log($"Created Event Channel Asset: {assetPath}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Event Channel Asset Generation Complete. Created {createdCount} new assets.");
    }
}
