using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Animation
{
    // Caching system cho Animator parameters — giúp tối ưu hiệu năng
    // và tránh lỗi khi set trigger, bool, float bằng string trực tiếp.
    public class AnimationParameterCache
    {
        private readonly Dictionary<string, int> hashCache = new();  // Lưu hash các parameter
        private Animator animator;                                  // Animator hiện tại được gán

        // Gọi khi khởi tạo PlayerAnimationController hoặc bất kỳ hệ thống nào có Animator.
        public void Initialize(Animator animator)
        {
            this.animator = animator;
            CacheAllParameters();
        }

        // Cache tất cả parameter từ Animator hiện tại (khi bắt đầu game).
        public void CacheAllParameters()
        {
            hashCache.Clear();
            if (animator == null)
            {
                return;
            }

            foreach (var param in animator.parameters)
            {
                hashCache[param.name] = Animator.StringToHash(param.name);
            }
        }

        // Cache riêng một parameter nếu chưa tồn tại.
        // Dùng khi bạn có parameter dynamic (ví dụ tool upgrade trigger).
        public void CacheParameter(string paramName)
        {
            if (!hashCache.ContainsKey(paramName))
            {
                hashCache[paramName] = Animator.StringToHash(paramName);
            }
        }

        // Xóa toàn bộ cache (ít khi cần, trừ khi thay Animator runtime).
        public void ClearCache()
        {
            hashCache.Clear();
        }

        // Lấy hash code của parameter.
        // Nếu chưa tồn tại, tự động cache lại để tránh null reference.
        public int GetHash(string paramName)
        {
            if (hashCache.TryGetValue(paramName, out var hash)) 
            {
                hash = Animator.StringToHash(paramName);
                hashCache[paramName] = hash;
            }

            return hash;
        }

        // Kiểm tra xem Animator có chứa parameter đó không.
        // Hữu ích để tránh lỗi khi set sai tên.
        public bool HasParameter(string paramName)
        {
            if (hashCache.ContainsKey(paramName)) return true;

            if (animator == null) return false;

            foreach (var param in animator.parameters)
            {
                if ((param.name == paramName)) return true;
            }

            return false;
        }

#if UNITY_EDITOR
        public void LogAllParameters()
        {
            if (animator == null)
            {
                Debug.LogWarning("No animator assigned to AnimationParameterCache!");
                return;
            }

            Debug.Log($"Animator '{animator.name}' has {animator.parameters.Length} parameters:");
            foreach (var p in animator.parameters)
            {
                Debug.Log($"  - {p.name} ({p.type})");
            }
        }
#endif
    }

}
