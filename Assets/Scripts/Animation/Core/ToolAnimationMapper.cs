using System.Collections.Generic;
using TinyFarm.Items;
using UnityEngine;

namespace TinyFarm.Animation
{
    // Ánh xạ (mapping) giữa ToolType và các thông tin animation liên quan.
    // Class này giúp PlayerAnimationController biết mỗi tool dùng animation nào,
    // trigger nào, thời lượng bao lâu, và có thể phát hay không.
    public class ToolAnimationMapper
    {
        private readonly Dictionary<ToolType, AnimationState> toolToAnimState = new();
        private readonly Dictionary<ToolType, string> toolToTrigger = new();
        private readonly Dictionary<ToolType, ToolAnimationConfig> toolConfigs = new();

        // Tạo instance mới, chưa có dữ liệu. Cần gọi Initialize() hoặc LoadConfigs()
        public ToolAnimationMapper() { }

        // Gọi khi PlayerAnimationController khởi tạo.
        public void Initialize(ToolAnimationConfig[] configs = null)
        {
            toolConfigs.Clear();
            toolToAnimState.Clear();
            toolToTrigger.Clear();

            if (configs != null && configs.Length > 0)
            {
                LoadConfigs(configs);
            }
        }

        // Nạp danh sách cấu hình animation cho từng tool (thường từ ScriptableObject).
        public void LoadConfigs(ToolAnimationConfig[] configs)
        {
            if (configs == null || configs.Length == 0)
            {
                Debug.LogWarning("[ToolAnimationMapper] No ToolAnimationConfig provided!");
                return;
            }

            int loadedCount = 0;

            foreach (var config in configs)
            {
                if (config == null)
                {
                    Debug.LogWarning("[ToolAnimationMapper] Null config in array, skipping...");
                    continue;
                }

                // Validate config
                if (!config.IsValid())
                {
                    Debug.LogWarning($"[ToolAnimationMapper] Invalid config '{config.name}' (ToolType={config.toolType}), skipping...");
                    continue;
                }

                ToolType toolType = config.toolType;

                // ✅ FIXED: Lưu vào Dictionary, không phải array
                toolConfigs[toolType] = config;
                toolToAnimState[toolType] = config.animationState;

                string trigger = string.IsNullOrEmpty(config.animatorTrigger)
                    ? "UseTool"
                    : config.animatorTrigger;
                toolToTrigger[toolType] = trigger;

                loadedCount++;
            }

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

        public string GetAnimationTrigger(ToolType tool, Direction dir)
        {
            if (!toolConfigs.TryGetValue(tool, out var config) || config == null)
                return GetAnimationTrigger(tool);

            string trigger = config.GetAnimatorTrigger(dir);
            if (string.IsNullOrEmpty(trigger))
                trigger = GetAnimationTrigger(tool);

            return trigger;
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
            if (!toolConfigs.TryGetValue(tool, out var config) || config == null)
                return false;

            // Nếu config valid thì có thể play
            return config.IsValid();
        }

        // Lấy thời lượng animation (auto fallback nếu không có config).
        public float GetAnimationDuration(ToolType tool, Direction dir = Direction.Down)
        {
            if (toolConfigs.TryGetValue(tool, out var config) && config != null)
            {
                return config.GetAnimationDuration(dir);
            }
            return 1f; // fallback
        }

        public bool HasClipForDirection(ToolType tool, Direction dir)
        {
            if (!toolConfigs.TryGetValue(tool, out var config) || config == null)
                return false;

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

