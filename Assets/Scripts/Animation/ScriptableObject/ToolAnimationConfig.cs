using System.Collections;
using System.Collections.Generic;
using TinyFarm.Items;
using UnityEngine;

namespace TinyFarm.Animation
{
    // ScriptableObject configuration for tool animations
    // Stores all data needed for a specific tool's animation behavior
    // Complexity: Low (3 methods)
    [CreateAssetMenu(fileName = "ToolAnimConfig", menuName = "Farming/Tool Animation Config")]
    public class ToolAnimationConfig : ScriptableObject
    {
        [Header("Tool Info")]
        [Tooltip("Tên cấu hình hiển thị")]
        public string configName = "New Tool Animation";

        [Tooltip("Loại công cụ áp dụng animation")]
        public ToolType toolType;

        [Tooltip("Trạng thái animation tương ứng")]
        public AnimationState animationState;

        [Header("Directional Animations")]
        [Tooltip("Danh sách các animation clip theo hướng (Down/Up/Right, Left sẽ dùng Right + flip)")]
        public List<DirectionalAnimation> directionalAnimations = new List<DirectionalAnimation>();

        [Header("Durations per Direction")]
        public float durationDown = 1.0f;
        public float durationUp = 1.0f;
        public float durationSide = 1.0f;

        [Header("Impact Timing (normalized 0-1)")]
        [Range(0f, 1f)] public float impactTimeNormalized = 0.5f;

        [Header("Animation Settings")]
        [Tooltip("Tên trigger trong Animator")]
        public string animatorTrigger = "Use";

        [Header("Behavior")]
        [Tooltip("Cho phép di chuyển khi đang animation")]
        public bool canMove = false;

        [Tooltip("Có thể hủy animation giữa chừng")]
        public bool canCancel = false;

        [Tooltip("Thời gian bị khóa điều khiển khi thực hiện animation")]
        [Min(0f)]
        public float lockDuration = 0.5f;

        [Tooltip("Thời gian hồi trước khi dùng lại công cụ")]
        [Min(0f)]
        public float cooldownTime = 0.2f;

        [Header("Effects")]
        [Tooltip("Hiệu ứng âm thanh (chọn ngẫu nhiên)")]
        public AudioClip[] soundEffects;

        [Tooltip("Vị trí lệch của hiệu ứng so với nhân vật")]
        public Vector3 effectOffset = Vector3.zero;

        [Tooltip("Tỉ lệ phóng to/thu nhỏ hiệu ứng (1 = mặc định)")]
        [Min(0.1f)]
        public float effectScale = 1f;

        [Header("Tool Visual")]
        [Tooltip("Model hoặc sprite đại diện cho công cụ")]
        public GameObject toolPrefab;

        [Tooltip("Biểu tượng hiển thị trong giao diện (UI)")]
        public Sprite toolIcon;

        // Cached values để tối ưu performance
        private Dictionary<Direction, int> cachedImpactFrames = new Dictionary<Direction, int>();
        private Dictionary<Direction, int> cachedTotalFrames = new Dictionary<Direction, int>();

        public float GetDuration(Direction direction)
        {
            return direction switch
            {
                Direction.Down => durationDown,
                Direction.Up => durationUp,
                Direction.Side => durationSide,
                _ => 1.0f
            };
        }
        //public float GetImpactTime(Direction direction)
        //{
        //    return GetDuration(direction) * impactTimeNormalized;
        //}

        // Lấy animation clip phù hợp với hướng nhân vật, tự động xử lý Left/Right flip
        public AnimationClip GetClipByDirection(Direction dir, out bool shouldFlip)
        {
            shouldFlip = false;

            if (directionalAnimations == null || directionalAnimations.Count == 0)
            {
                Debug.LogError($"ToolAnimationConfig '{configName}': Không có directional animations!");
                return null;
            }

            // Thử tìm clip cho direction chính xác
            foreach (var anim in directionalAnimations)
            {
                if (anim.direction == dir && anim.clip != null)
                    return anim.clip;
            }

            // Nếu là Left, thử dùng Right và flip
            if (dir == Direction.Side)
            {
                foreach (var anim in directionalAnimations)
                {
                    if (anim.direction == Direction.Side && anim.clip != null)
                    {
                        shouldFlip = true;
                        return anim.clip;
                    }
                }
            }

            Debug.LogWarning($"ToolAnimationConfig '{configName}': Không tìm thấy clip cho hướng {dir}");
            return null;
        }

        // Lấy DirectionalAnimation data đầy đủ theo hướng (xử lý Left = Right)
        public DirectionalAnimation GetDirectionalAnimation(Direction dir)
        {
            if (directionalAnimations == null || directionalAnimations.Count == 0)
                return null;

            // Thử tìm direction chính xác
            foreach (var anim in directionalAnimations)
            {
                if (anim.direction == dir)
                    return anim;
            }

            // Nếu là Left, dùng Right
            if (dir == Direction.Side)
            {
                foreach (var anim in directionalAnimations)
                {
                    if (anim.direction == Direction.Side)
                        return anim;
                }
            }

            return null;
        }

        // Lấy animator trigger cho direction cụ thể (có thể override)
        public string GetAnimatorTrigger(Direction dir)
        {
            var dirAnim = GetDirectionalAnimation(dir);
            if (dirAnim != null && !string.IsNullOrEmpty(dirAnim.animatorTrigger))
            {
                return dirAnim.animatorTrigger;
            }
            return animatorTrigger; // Fallback to default trigger
        }

        // Lấy duration cho direction cụ thể
        public float GetAnimationDuration(Direction dir)
        {
            var dirAnim = GetDirectionalAnimation(dir);
            if (dirAnim != null)
            {
                // Nếu có override duration thì dùng
                if (dirAnim.overrideDuration > 0f)
                    return dirAnim.overrideDuration;

                // Nếu có clip thì lấy length từ clip
                if (dirAnim.clip != null)
                    return dirAnim.clip.length;
            }

            Debug.LogWarning($"ToolAnimationConfig '{configName}': Không tìm thấy duration cho {dir}, dùng 1.0f");
            return 1.0f;
        }

        // Lấy impact frame time cho direction cụ thể
        public float GetImpactFrameTime(Direction dir)
        {
            var dirAnim = GetDirectionalAnimation(dir);
            if (dirAnim != null)
            {
                return dirAnim.impactFrameTime;
            }
            return 0.5f; // Default
        }

        // Lấy frame number khi xảy ra impact/effect theo direction
        public int GetImpactFrame(Direction dir)
        {
            if (!cachedImpactFrames.ContainsKey(dir) || !Application.isPlaying)
            {
                float impactTime = GetImpactFrameTime(dir);
                cachedImpactFrames[dir] = Mathf.RoundToInt(GetTotalFrames(dir) * impactTime);
            }
            return cachedImpactFrames[dir];
        }

        // Lấy tổng số frame trong animation theo direction
        public int GetTotalFrames(Direction dir)
        {
            if (!cachedTotalFrames.ContainsKey(dir) || !Application.isPlaying)
            {
                bool shouldFlip;
                var clip = GetClipByDirection(dir, out shouldFlip);
                float duration = GetAnimationDuration(dir);

                if (clip != null)
                {
                    float fps = clip.frameRate;
                    cachedTotalFrames[dir] = Mathf.RoundToInt(duration * fps);
                }
                else
                {
                    // Mặc định 60 FPS nếu không có clip
                    cachedTotalFrames[dir] = Mathf.RoundToInt(duration * 60f);
                }
            }
            return cachedTotalFrames[dir];
        }

        // Lấy thời gian thực tế của impact theo direction (tính bằng giây)
        public float GetImpactTime(Direction dir)
        {
            return GetAnimationDuration(dir) * GetImpactFrameTime(dir);
        }

        // Validate và tự động điền dữ liệu còn thiếu
        public void Validate()
        {
            // Validate directional animations
            if (directionalAnimations != null)
            {
                foreach (var dirAnim in directionalAnimations)
                {
                    if (dirAnim.clip != null && dirAnim.overrideDuration <= 0f)
                    {
                        dirAnim.overrideDuration = dirAnim.clip.length;
                    }
                    dirAnim.impactFrameTime = Mathf.Clamp01(dirAnim.impactFrameTime);
                }
            }

            // Tự động đặt tên config
            if (string.IsNullOrEmpty(configName))
            {
                configName = $"{toolType}_{animationState}";
            }

            // Tự động đặt animator trigger
            if (string.IsNullOrEmpty(animatorTrigger))
            {
                animatorTrigger = animationState.ToString();
            }

            // Clamp các giá trị
            lockDuration = Mathf.Max(0f, lockDuration);
            cooldownTime = Mathf.Max(0f, cooldownTime);
            effectScale = Mathf.Max(0.1f, effectScale);

            // Xóa cache
            ClearCache();

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        // Xóa tất cả cached values
        public void ClearCache()
        {
            cachedImpactFrames.Clear();
            cachedTotalFrames.Clear();
        }

        // Lấy một sound effect ngẫu nhiên từ mảng
        public AudioClip GetRandomSound()
        {
            if (soundEffects == null || soundEffects.Length == 0)
                return null;

            return soundEffects[Random.Range(0, soundEffects.Length)];
        }

        // Kiểm tra config có hợp lệ không
        public bool IsValid()
        {
            if (directionalAnimations == null || directionalAnimations.Count == 0)
                return false;

            // Kiểm tra có ít nhất 1 clip hợp lệ
            bool hasValidClips = false;
            foreach (var dirAnim in directionalAnimations)
            {
                if (dirAnim.clip != null)
                {
                    hasValidClips = true;
                    break;
                }
            }

            return hasValidClips && !string.IsNullOrEmpty(animatorTrigger);
        }

        // Kiểm tra xem direction có clip hay không
        public bool HasClipForDirection(Direction dir)
        {
            bool shouldFlip;
            return GetClipByDirection(dir, out shouldFlip) != null;
        }

        // Kiểm tra các direction còn thiếu (để show warning trong Editor)
        public List<Direction> GetMissingDirections()
        {
            var missing = new List<Direction>();
            var allDirections = new[] { Direction.Down, Direction.Up, Direction.Side };

            foreach (var dir in allDirections)
            {
                bool found = false;
                foreach (var anim in directionalAnimations)
                {
                    if (anim.direction == dir && anim.clip != null)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    missing.Add(dir);
            }

            return missing;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            Validate();

            // Show warning nếu thiếu direction
            var missing = GetMissingDirections();
            if (missing.Count > 0)
            {
                Debug.LogWarning($"ToolAnimationConfig '{configName}': Thiếu animations cho: {string.Join(", ", missing)}", this);
            }
        }
#endif
    }
}

