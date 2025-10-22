using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Animation
{
    // Global animation settings cho character controller
    // Quản lý tốc độ di chuyển, transition times và blend settings
    // Tối ưu cho game 2D farming (Stardew Valley style)
    // Complexity: Low (2 methods)
    [CreateAssetMenu(fileName = "AnimSettings", menuName = "Farming/Animation Settings")]
    public class AnimationSettings : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Tốc độ chạy của nhân vật (units/giây)")]
        [Range(1f, 10f)]
        public float moveSpeed = 4f;

        [Tooltip("Ngưỡng tốc độ để kích hoạt animation chạy (thay vì idle)")]
        [Range(0.01f, 2f)]
        public float movementThreshold = 0.1f;

        [Header("Transitions")]
        [Tooltip("Thời gian chuyển đổi mặc định giữa các trạng thái")]
        [Range(0f, 1f)]
        public float defaultTransitionTime = 0.1f;

        [Tooltip("Sử dụng chuyển đổi mượt mà giữa các animation")]
        public bool smoothTransitions = true;

        [Tooltip("Đường cong cho animation speed dựa trên tốc độ di chuyển")]
        public AnimationCurve speedCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("State Blend Times")]
        [Tooltip("Thời gian blend tùy chỉnh cho các chuyển đổi cụ thể")]
        public List<StateTransitionBlend> customBlendTimes = new List<StateTransitionBlend>();

        [Header("Advanced Settings")]
        [Tooltip("Thời gian tăng tốc để đạt tốc độ mục tiêu")]
        [Range(0f, 2f)]
        public float accelerationTime = 0.15f;

        [Tooltip("Thời gian giảm tốc để dừng lại")]
        [Range(0f, 2f)]
        public float decelerationTime = 0.1f;

        [Tooltip("Hệ số nhân cho tốc độ animation (để điều chỉnh timing)")]
        [Range(0.5f, 2f)]
        public float animationSpeedMultiplier = 1f;

        [Tooltip("Khóa input trong thời gian thực hiện tool animation")]
        public bool lockInputDuringToolUse = true;

        // Cache cho tra cứu blend time nhanh hơn
        private Dictionary<string, float> blendTimeCache;

        private void OnEnable()
        {
            BuildBlendTimeCache();
        }

        // Lấy thời gian blend khi chuyển đổi giữa hai trạng thái
        /// </summary>
        public float GetBlendTime(AnimationState from, AnimationState to)
        {
            if (blendTimeCache == null)
                BuildBlendTimeCache();

            string key = GetTransitionKey(from, to);

            if (blendTimeCache.TryGetValue(key, out float blendTime))
            {
                return blendTime;
            }

            // Trả về giá trị mặc định nếu không tìm thấy
            return defaultTransitionTime;
        }

        // Lấy hệ số tốc độ dựa trên tốc độ di chuyển hiện tại
        // Chỉ có 2 trạng thái: Idle (0) hoặc Moving (1+)
        public float GetSpeedMultiplier(float currentSpeed)
        {
            if (currentSpeed < movementThreshold)
            {
                return 0f; // Idle - không di chuyển
            }
            else
            {
                // Moving - scale animation speed theo tốc độ thực tế
                float normalizedSpeed = currentSpeed / moveSpeed;
                return speedCurve.Evaluate(normalizedSpeed) * animationSpeedMultiplier;
            }
        }

        // Kiểm tra nhân vật có đang di chuyển không
        public bool IsMoving(float speed)
        {
            return speed >= movementThreshold;
        }

        // Lấy tốc độ animation dựa trên tốc độ di chuyển
        public float GetAnimationSpeed(float movementSpeed)
        {
            if (movementSpeed < movementThreshold)
                return 1f * animationSpeedMultiplier;

            // Scale animation speed theo tỷ lệ với move speed
            return (movementSpeed / moveSpeed) * animationSpeedMultiplier;
        }

        // Kiểm tra có nên chuyển sang animation Idle không
        public bool ShouldPlayIdle(float speed)
        {
            return speed < movementThreshold;
        }

        // Kiểm tra có nên chuyển sang animation Run không
        public bool ShouldPlayRun(float speed)
        {
            return speed >= movementThreshold;
        }

        // Xây dựng cache cho tra cứu blend time nhanh hơn
        private void BuildBlendTimeCache()
        {
            blendTimeCache = new Dictionary<string, float>();

            if (customBlendTimes == null)
                return;

            foreach (var blend in customBlendTimes)
            {
                if (blend != null)
                {
                    string key = GetTransitionKey(blend.fromState, blend.toState);
                    blendTimeCache[key] = blend.blendTime;
                }
            }
        }

        // Tạo key duy nhất cho state transition
        private string GetTransitionKey(AnimationState from, AnimationState to)
        {
            return $"{from}_{to}";
        }

        // Thêm blend time tùy chỉnh cho transition cụ thể
        public void SetCustomBlendTime(AnimationState from, AnimationState to, float blendTime)
        {
            if (customBlendTimes == null)
                customBlendTimes = new List<StateTransitionBlend>();

            // Kiểm tra đã tồn tại chưa
            var existing = customBlendTimes.Find(b => b.fromState == from && b.toState == to);

            if (existing != null)
            {
                existing.blendTime = blendTime;
            }
            else
            {
                customBlendTimes.Add(new StateTransitionBlend
                {
                    fromState = from,
                    toState = to,
                    blendTime = blendTime
                });
            }

            BuildBlendTimeCache();

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        // Reset về giá trị mặc định (dùng cho testing)
        /// </summary>
        public void ResetToDefaults()
        {
            moveSpeed = 4f;
            movementThreshold = 0.1f;
            defaultTransitionTime = 0.1f;
            smoothTransitions = true;
            accelerationTime = 0.15f;
            decelerationTime = 0.1f;
            animationSpeedMultiplier = 1f;
            lockInputDuringToolUse = true;
            customBlendTimes?.Clear();
            BuildBlendTimeCache();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Clamp các giá trị
            moveSpeed = Mathf.Max(0.1f, moveSpeed);
            movementThreshold = Mathf.Max(0.01f, movementThreshold);
            defaultTransitionTime = Mathf.Max(0f, defaultTransitionTime);
            accelerationTime = Mathf.Max(0f, accelerationTime);
            decelerationTime = Mathf.Max(0f, decelerationTime);
            animationSpeedMultiplier = Mathf.Clamp(animationSpeedMultiplier, 0.5f, 2f);

            BuildBlendTimeCache();
        }
#endif
    }
}

