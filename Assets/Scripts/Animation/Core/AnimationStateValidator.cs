using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Animation
{
    // Quản lý và kiểm tra logic chuyển đổi giữa các trạng thái animation.
    // Mục tiêu: ngăn bug kiểu “đang hoe mà vẫn có thể nhảy sang run”.
    public class AnimationStateValidator
    {
        private readonly Dictionary<AnimationState, int> priorities = new();
        private readonly Dictionary<AnimationState, bool> canInterrupt = new();

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
            if (to == AnimationState.Idle) return true;
            if (IsVisualState(from) && IsVisualState(to)) return true;
            if (IsToolAction(from) && IsToolAction(to)) return false;
            if (from == AnimationState.Sleep && IsToolAction(to)) return false;
            return true;
        }

        public void SetupPriorities()
        {
            priorities.Clear();
            canInterrupt.Clear();

            // ✅ Visual States - CÓ THỂ interrupt
            priorities[AnimationState.Idle] = 1;
            priorities[AnimationState.Running] = 2;
            priorities[AnimationState.PickUpIdle] = 3;  // ✅ Visual state
            priorities[AnimationState.PickUpRun] = 4;   // ✅ Visual state

            // Tool Actions - KHÔNG THỂ interrupt
            priorities[AnimationState.UsingTool] = 10;
            priorities[AnimationState.Hoeing] = 11;
            priorities[AnimationState.Watering] = 12;
            priorities[AnimationState.Sickle] = 13;

            // Sleep
            priorities[AnimationState.Sleep] = 20;

            // ✅ Visual States - Interruptible
            canInterrupt[AnimationState.Idle] = true;
            canInterrupt[AnimationState.Running] = true;
            canInterrupt[AnimationState.PickUpIdle] = true;   // ✅ CÓ THỂ interrupt
            canInterrupt[AnimationState.PickUpRun] = true;    // ✅ CÓ THỂ interrupt

            // Tool Actions - Not Interruptible
            canInterrupt[AnimationState.UsingTool] = false;
            canInterrupt[AnimationState.Hoeing] = false;
            canInterrupt[AnimationState.Watering] = false;
            canInterrupt[AnimationState.Sickle] = false;

            // Sleep
            canInterrupt[AnimationState.Sleep] = true;
        }

        public bool ValidateTransition(AnimationState from, AnimationState to)
        {
            if (from == to) return false;
            if (to == AnimationState.Idle) return true;
            if (IsVisualState(from) && IsVisualState(to)) return true;

            int fromPriority = GetStatePriority(from);
            int toPriority = GetStatePriority(to);

            if (!CanInterruptState(from) && toPriority <= fromPriority) return false;
            return true;
        }

        public bool CanInterruptState(AnimationState state)
        {
            return canInterrupt.TryGetValue(state, out bool result) && result;
        }

        public int GetStatePriority(AnimationState state)
        {
            return priorities.TryGetValue(state, out int p) ? p : 0;
        }

        /// ✅ FIXED: Chỉ Tool Actions (Hoe, Watering, Sickle)
        /// KHÔNG bao gồm PickUpIdle/PickUpRun
        public bool IsToolAction(AnimationState state)
        {
            return state == AnimationState.UsingTool ||
                   state == AnimationState.Hoeing ||
                   state == AnimationState.Watering ||
                   state == AnimationState.Sickle;
        }

        /// ✅ NEW: Visual States (Idle, Running, PickUpIdle, PickUpRun)
        /// Có thể chuyển đổi tự do giữa các state này
        public bool IsVisualState(AnimationState state)
        {
            return state == AnimationState.Idle ||
                   state == AnimationState.Running ||
                   state == AnimationState.PickUpIdle ||
                   state == AnimationState.PickUpRun;
        }

        /// DEPRECATED: Use IsToolAction() hoặc IsVisualState()
        //public bool IsActionState(AnimationState state)
        //{
        //    return IsToolAction(state);
        //}

        public bool IsMovementState(AnimationState state)
        {
            return IsVisualState(state);
        }

#if UNITY_EDITOR
        public void LogStateRules()
        {
            Debug.Log("=== Animation State Rules ===");
            foreach (var kvp in priorities)
            {
                bool _canInterrupt = canInterrupt.TryGetValue(kvp.Key, out bool val) && val;
                Debug.Log($"State: {kvp.Key} | Priority: {kvp.Value} | Interruptible: {_canInterrupt}");
            }
        }
#endif
    }
}

