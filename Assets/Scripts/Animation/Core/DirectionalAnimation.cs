using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DirectionalAnimation
{
    [Header("Direction")]
    public Direction direction = Direction.Down;

    [Header("Animation Clip")]
    public AnimationClip clip;

    [Header("Timing")]
    [Tooltip("Override duration (0 = dùng clip length)")]
    [Min(0f)]
    public float overrideDuration = 0f;

    [Tooltip("Thời điểm impact (0-1, normalized)")]
    [Range(0f, 1f)]
    public float impactFrameTime = 0.5f;

    [Header("Optional Trigger Override")]
    [Tooltip("Trigger riêng cho direction này (để trống = dùng default)")]
    public string animatorTrigger = "";

    // Get duration thực tế
    public float GetDuration()
    {
        if (overrideDuration > 0f)
            return overrideDuration;

        if (clip != null)
            return clip.length;

        return 1f; // fallback
    }

    // Get impact time tính bằng giây
    public float GetImpactTime()
    {
        return GetDuration() * impactFrameTime;
    }

    // Validate data
    public bool IsValid()
    {
        return clip != null;
    }
}
