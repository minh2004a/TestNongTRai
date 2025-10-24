using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Animation
{
    // Quản lý và kiểm tra logic chuyển đổi giữa các trạng thái animation.
    // Mục tiêu: ngăn bug kiểu “đang hoe mà vẫn có thể nhảy sang run”.
    public class AnimationStateValidator
    {
        private readonly Dictionary<AnimationState, int> priorities = new();      // Độ ưu tiên từng state
        private readonly Dictionary<AnimationState, bool> canInterrupt = new();   // Có thể bị ngắt giữa chừng không

        public AnimationStateValidator()
        {
            SetupPriorities();
        }

        public void Initialize()
        {
            // Setup validation rules nếu cần
        }

        public bool CanTransition(AnimationState from, AnimationState to)
        {
            // Luôn cho phép về Idle
            if (to == AnimationState.Idle)
                return true;

            // Không cho transition giữa các tool actions
            if (IsToolAction(from) && IsToolAction(to))
                return false;

            // Sleep không thể bị interrupt bởi tool actions
            if (from == AnimationState.Sleep && IsToolAction(to))
                return false;

            return true;
        }

        // Thiết lập độ ưu tiên mặc định cho từng AnimationState.
        // Bạn có thể mở rộng nếu thêm hành động mới (như Fishing, Riding, v.v.).
        public void SetupPriorities()
        {
            priorities.Clear();
            canInterrupt.Clear();

            priorities[AnimationState.Idle] = 1;
            priorities[AnimationState.Running] = 2;

            priorities[AnimationState.UsingTool] = 10;
            priorities[AnimationState.Hoeing] = 11;
            priorities[AnimationState.Watering] = 12;
            priorities[AnimationState.Sickle] = 13;
            priorities[AnimationState.PickUpIdle] = 14;
            priorities[AnimationState.PickUpRun] = 15;

            canInterrupt[AnimationState.Idle] = true;
            canInterrupt[AnimationState.Running] = true;
            canInterrupt[AnimationState.Sleep] = true;

            canInterrupt[AnimationState.UsingTool] = false;
            canInterrupt[AnimationState.Hoeing] = false;
            canInterrupt[AnimationState.Watering] = false;
            canInterrupt[AnimationState.Sickle] = false;
            canInterrupt[AnimationState.PickUpIdle] = false;
            canInterrupt[AnimationState.PickUpRun] = false;
        }

        // Kiểm tra xem có thể chuyển từ state A sang state B không.
        public bool ValidateTransition(AnimationState from, AnimationState to)
        {
            // Same state - no transition needed
            if (from == to)
                return false;

            // Always allow transition to Idle
            if (to == AnimationState.Idle)
                return true;

            int fromPriority = GetStatePriority(from);
            int toPriority = GetStatePriority(to);

            // If current state cannot be interrupted and new state doesn't have higher priority
            if (!CanInterruptState(from) && toPriority <= fromPriority)
                return false;

            return true;
        }

        // Kiểm tra state có thể bị hủy giữa chừng hay không.
        public bool CanInterruptState(AnimationState state)
        {
            return canInterrupt.TryGetValue(state, out bool result) && result;
        }

        // Trả về độ ưu tiên (priority) của state.
        public int GetStatePriority(AnimationState state)
        {
            return priorities.TryGetValue(state, out int p) ? p : 0;
        }

        // Xác định xem state có phải loại “action” (có animation riêng, lock input).
        public bool IsActionState(AnimationState state)
        {
            return state == AnimationState.UsingTool ||
                   state == AnimationState.Hoeing ||
                   state == AnimationState.Watering ||
                   state == AnimationState.Sickle ||
                   state == AnimationState.PickUpIdle ||
                   state == AnimationState.PickUpRun;
        }

        // ADDED: Helper method cho tool actions
        public bool IsToolAction(AnimationState state)
        {
            return IsActionState(state);
        }

        // Xác định xem state có phải loại “movement” (idle, walk, run).
        public bool IsMovementState(AnimationState state)
        {
            return state == AnimationState.Idle ||
                   state == AnimationState.Running;
        }

#if UNITY_EDITOR
        public void LogStateRules()
        {
            Debug.Log("=== Animation State Rules ===");
            foreach (var kvp in priorities)
            {
                bool _canInterrupt = canInterrupt.TryGetValue(kvp.Key, out bool val) && val;
                Debug.Log($"State: {kvp.Key} | Priority: {kvp.Value} | Interruptible: {canInterrupt}");
            }
        }
#endif
    }
}

