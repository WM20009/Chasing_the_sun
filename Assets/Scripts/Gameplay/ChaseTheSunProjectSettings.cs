using UnityEngine;

public static class ChaseTheSunProjectSettings
{
    public const string PlayerLayer = "Player";
    public const string GroundLayer = "Ground";
    public const string PushableLayer = "Pushable";
    public const string ClimbableLayer = "Climbable";
    public const string HazardLayer = "Hazard";
    public const string CheckpointLayer = "Checkpoint";
    public const string DecorationLayer = "Decoration";

    public const string BackgroundSortingLayer = "Background";
    public const string GameplaySortingLayer = "Gameplay";
    public const string PlayerSortingLayer = "Player";
    public const string ForegroundSortingLayer = "Foreground";
    public const string OverlaySortingLayer = "Overlay";

    public const string GeneratedRoot = "Assets/Generated/ChaseTheSun";
    public const string ManualRoot = "Assets/Manual/ChaseTheSun";

    public static LayerMask MaskFromNames(params string[] layerNames)
    {
        var mask = 0;
        if (layerNames == null)
        {
            return mask;
        }

        foreach (var layerName in layerNames)
        {
            if (string.IsNullOrWhiteSpace(layerName))
            {
                continue;
            }

            var layerIndex = LayerMask.NameToLayer(layerName);
            if (layerIndex >= 0)
            {
                mask |= 1 << layerIndex;
            }
        }

        return mask;
    }
}
