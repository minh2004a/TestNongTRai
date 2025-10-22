using UnityEngine;

[System.Serializable]
public class StateTransitionBlend
{
    // Định nghĩa blend time tùy chỉnh cho transition cụ thể
    [Tooltip("Trạng thái bắt đầu")]
    public AnimationState fromState;

    [Tooltip("Trạng thái đích")]
    public AnimationState toState;

    [Tooltip("Thời gian blend cho transition này")]
    [Range(0f, 1f)]
    public float blendTime = 0.1f;
}
