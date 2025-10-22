using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TinyFarm.Items;
using UnityEngine;

namespace TinyFarm.Animation
{
    // Ánh xạ (mapping) giữa ToolType và các thông tin animation liên quan.
    // Class này giúp PlayerAnimationController biết mỗi tool dùng animation nào,
    // trigger nào, thời lượng bao lâu, và có thể phát hay không.
    public class ToolAnimationMapper : MonoBehaviour
    {
        private readonly Dictionary<ToolType, AnimationState> toolToAnimState = new();
        private readonly Dictionary<ToolType, string> toolToTrigger = new();
        private readonly Dictionary<ToolType, ToolAnimationConfig> toolConfigs = new();

        // Tạo instance mới, chưa có dữ liệu. Cần gọi Initialize() hoặc LoadConfigs()
        public ToolAnimationMapper() { }

        // Gọi khi PlayerAnimationController khởi tạo.
        public void Initialize()
        {
            toolConfigs.Clear();
            toolToAnimState.Clear();
            toolToTrigger.Clear();
        }

        // Nạp danh sách cấu hình animation cho từng tool (thường từ ScriptableObject).
        public void LoadConfigs(ToolAnimationConfig[] configs)
        {
            Initialize();

            if (configs == null || configs.Length == 0)
            {
                Debug.LogWarning("[ToolAnimationMapper] Không có ToolAnimationConfig nào được cung cấp!");
                return;
            }


            foreach (var config in configs)
            {
                if (config == null)
                {
                    continue;
                }

                // validate config shape quickly (IsValid checks directional animations & trigger)
                if (!config.IsValid())
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"[ToolAnimationMapper] Skipping invalid ToolAnimationConfig '{config.name}' (ToolType={config.toolType}).");
#endif
                    continue;
                }

                var t = config.toolType;

                // store
                configs[(int)t] = config;
                toolToAnimState[t] = config.animationState;
                toolToTrigger[t] = string.IsNullOrEmpty(config.animatorTrigger) ? "UseTool" : config.animatorTrigger;
            }

#if UNITY_EDITOR
            Debug.Log($"[ToolAnimationMapper] Loaded tool configs.");
#endif
        }

        // Lấy AnimationState tương ứng với tool (VD: ToolType.Hoe → AnimationState.Hoeing).
        public AnimationState GetAnimationState(ToolType tool)
        {
            return toolToAnimState.TryGetValue(tool, out var state) ? state : AnimationState.UsingTool; // fallback mặc định
        }

        // Lấy trigger name tương ứng (VD: “UseHoe” hoặc “UseTool”)
        public string GetAnimationTrigger(ToolType tool)
        {
            return toolToTrigger.TryGetValue(tool, out var trigger)
                ? trigger
                : "UseTool";
        }

        // Lấy toàn bộ ToolAnimationConfig tương ứng.
        public ToolAnimationConfig GetConfig(ToolType tool)
        {
            return toolConfigs.TryGetValue(tool, out var config) ? config : null;
        }

        // Kiểm tra tool có config hợp lệ hay không.
        public bool HasConfig(ToolType tool)
        {
            return toolConfigs.ContainsKey(tool) && toolConfigs[tool] != null;
        }

        // Xác định tool có thể phát animation hay không.
        // Dựa vào config hợp lệ và có clip tương ứng.
        public bool CanPlayAnimation(ToolType tool)
        {
            if (!toolConfigs.TryGetValue(tool, out var config) || config == null) return false;
            if (config.IsValid()) return false;

            return true;
        }

        // Lấy thời lượng animation (auto fallback nếu không có config).
        public float GetAnimationDuration(ToolType tool)
        {
            if (toolConfigs.TryGetValue(tool, out var config) && config != null)
            {
                if (config.directionalAnimations != null && config.directionalAnimations.Count > 0)
                {
                    var order = new[] {Direction.Down, Direction.Up, Direction.Side};
                    foreach (var d in order)
                    {
                        var dirAnim = config.GetDirectionalAnimation(d);
                        if (dirAnim != null)
                        {
                            var dur = dirAnim.overrideDuration > 0f ? dirAnim.overrideDuration : (dirAnim.clip != null ? dirAnim.clip.length : 0f);
                            if (dur > 0f) return dur;
                        }
                    }
                }

                try
                {
                    return config.GetAnimationDuration(Direction.Down);
                }
                catch
                {

                }
            }
            return 1f;
        }

        public string GetAnimationTrigger(ToolType tool, Direction dir)
        {
            if (!toolConfigs.TryGetValue(tool, out var config) || config == null)
                return GetAnimationTrigger(tool);

            var trigger = config.GetAnimatorTrigger(dir);
            if (string.IsNullOrEmpty(trigger))
                trigger = GetAnimationTrigger(tool);

            return trigger;
        }

        public bool HasClipForDirection(ToolType tool, Direction dir)
        {
            if (!toolConfigs.TryGetValue(tool, out var config) || config == null) return false;
            return config.HasClipForDirection(dir);
        }

        

#if UNITY_EDITOR
        /// Editor helper: log loaded mappings
        public void LogMappings()
        {
            Debug.Log("[ToolAnimationMapper] Current mappings:");
            foreach (var kv in toolConfigs)
            {
                var tool = kv.Key;
                var cfg = kv.Value;
                var trigger = GetAnimationTrigger(tool);
                var state = GetAnimationState(tool);
                Debug.Log($" - {tool}: cfg='{(cfg != null ? cfg.name : "null")}', state={state}, trigger='{trigger}'");
            }
        }
#endif
    }
}

