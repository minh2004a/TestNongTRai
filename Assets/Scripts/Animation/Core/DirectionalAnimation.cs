using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DirectionalAnimation
{
    [Tooltip("Hướng của animation (Up/Down/Left/Right)")]
    public Direction direction;

    [Tooltip("Clip animation tương ứng với hướng này")]
    public AnimationClip clip;

    [Tooltip("Tên trigger trong Animator (nếu có)")]
    public string animatorTrigger = "Use";

    [Tooltip("Thời lượng animation (nếu muốn override clip length)")]
    [Min(0f)] public float overrideDuration = 0f;

    [Tooltip("Thời điểm va chạm hoặc tạo hiệu ứng (0–1)")]
    [Range(0f, 1f)] public float impactFrameTime = 0.5f;
}
