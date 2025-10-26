using System;
using System.Collections;
using System.Collections.Generic;
using TinyFarm.Items;
using UnityEngine;

namespace TinyFarm.Animation
{
    // Component xử lý tất cả Animation Events
    // Nhận events từ Animation Clips và forward đến các systems
    public class AnimationEventHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerAnimationController animController;
        [SerializeField] private Animator animator;

        [Header("Impact Points")]
        [SerializeField] private Transform impactPointDown;
        [SerializeField] private Transform impactPointUp;
        [SerializeField] private Transform impactPointSide;
        [SerializeField] private float impactPointOffset = 1f;

        [Header("Sound Settings")]
        [SerializeField] private bool enableSounds = true;
        [SerializeField] private float footstepVolume = 0.8f;
        [SerializeField] private float toolSoundVolume = 1f;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool logAllEvents = false;

        // Event receivers
        private List<IAnimationEventReceiver> eventReceivers = new List<IAnimationEventReceiver>();

        // C# Events for external systems
        public event Action<AnimationEventData> OnToolImpactEvent;
        public event Action<AnimationEventData> OnFootstepEvent;
        public event Action<AnimationEventData> OnSoundEvent;
        public event Action<AnimationEventData> OnAnimationCompleteEvent;
        public event Action<AnimationEventData> OnGenericEvent;

        // Cached data
        private AnimatorStateInfo currentStateInfo;
        private bool isInitialized = false;

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            InitializeImpactPoints();
            isInitialized = true;
            LogDebug("AnimationEventHandler initialized");
        }

        private void OnValidate()
        {
            if (animController == null)
                animController = GetComponent<PlayerAnimationController>();

            if (animator == null)
                animator = GetComponent<Animator>();
        }

        private void InitializeComponents()
        {
            if (animController == null)
                animController = GetComponent<PlayerAnimationController>();

            if (animator == null)
                animator = GetComponent<Animator>();

            if (animController == null)
            {
                Debug.LogError("[AnimationEventHandler] PlayerAnimationController not found!");
                enabled = false;
                return;
            }

            if (animator == null)
            {
                Debug.LogError("[AnimationEventHandler] Animator not found!");
                enabled = false;
                return;
            }
        }

        private void InitializeImpactPoints()
        {
            // Tạo impact points nếu chưa có (dummy transforms)
            if (impactPointDown == null)
            {
                GameObject go = new GameObject("ImpactPoint_Down");
                go.transform.SetParent(transform);
                go.transform.localPosition = Vector3.down * impactPointOffset;
                impactPointDown = go.transform;
            }

            if (impactPointUp == null)
            {
                GameObject go = new GameObject("ImpactPoint_Up");
                go.transform.SetParent(transform);
                go.transform.localPosition = Vector3.up * impactPointOffset;
                impactPointUp = go.transform;
            }

            if (impactPointSide == null)
            {
                GameObject go = new GameObject("ImpactPoint_Side");
                go.transform.SetParent(transform);
                go.transform.localPosition = Vector3.right * impactPointOffset;
                impactPointSide = go.transform;
            }
        }

        // ==========================================
        // REGISTRATION
        // ==========================================

        // Register receiver để nhận animation events
        public void RegisterReceiver(IAnimationEventReceiver receiver)
        {
            if (receiver != null && !eventReceivers.Contains(receiver))
            {
                eventReceivers.Add(receiver);
                LogDebug($"Registered receiver: {receiver.GetType().Name}");
            }
        }

        // Unregister receiver
        public void UnregisterReceiver(IAnimationEventReceiver receiver)
        {
            if (receiver != null && eventReceivers.Contains(receiver))
            {
                eventReceivers.Remove(receiver);
                LogDebug($"Unregistered receiver: {receiver.GetType().Name}");
            }
        }

        // ==========================================
        // ANIMATION EVENT METHODS (Called from Animation Clips)
        // ==========================================

        // Called từ Animation Event - Tool Impact
        // Function name trong Animation window: "OnToolImpact"
        public void OnToolImpact()
        {
            if (!isInitialized) return;

            AnimationEventData eventData = CreateEventData(AnimationEventType.ToolImpact);

            // Add tool-specific data
            eventData.toolType = GetCurrentToolType();
            eventData.impactPoint = GetImpactPoint();
            eventData.impactNormal = GetImpactNormal();

            // Notify receivers
            NotifyReceivers(eventData);

            // Fire C# event
            OnToolImpactEvent?.Invoke(eventData);

            LogEvent("ToolImpact", eventData);
        }

        // Called từ Animation Event - Footstep
        // Function name: "OnFootstep"
        public void OnFootstep()
        {
            if (!isInitialized) return;

            AnimationEventData eventData = CreateEventData(AnimationEventType.Footstep);
            eventData.soundName = "footstep";
            eventData.soundVolume = footstepVolume;

            // Fire C# event
            OnFootstepEvent?.Invoke(eventData);

            // Play sound (mock)
            PlaySound(eventData);

            LogEvent("Footstep", eventData);
        }

        // Called từ Animation Event - Play Sound với tên cụ thể
        // Function name: "PlaySoundEffect"
        // Parameter: string soundName
        public void PlaySoundEffect(string soundName)
        {
            if (!isInitialized) return;

            AnimationEventData eventData = CreateEventData(AnimationEventType.PlaySound);
            eventData.soundName = soundName;
            eventData.soundVolume = toolSoundVolume;

            // Fire C# event
            OnSoundEvent?.Invoke(eventData);

            // Play sound (mock)
            PlaySound(eventData);

            LogEvent($"PlaySound: {soundName}", eventData);
        }

        // Called từ Animation Event - Animation Start
        // Function name: "OnAnimationStart"
        public void OnAnimationStart()
        {
            if (!isInitialized) return;

            AnimationEventData eventData = CreateEventData(AnimationEventType.AnimationStart);
            OnGenericEvent?.Invoke(eventData);

            LogEvent("AnimationStart", eventData);
        }

        // Called từ Animation Event - Animation Complete
        // Function name: "OnAnimationComplete"
        public void OnAnimationComplete()
        {
            if (!isInitialized) return;

            AnimationEventData eventData = CreateEventData(AnimationEventType.AnimationComplete);

            // Notify receivers
            foreach (var receiver in eventReceivers)
            {
                receiver.OnAnimationComplete(eventData);
            }

            // Fire C# event
            OnAnimationCompleteEvent?.Invoke(eventData);

            LogEvent("AnimationComplete", eventData);
        }

        // ==========================================
        // EVENT DATA CREATION
        // ==========================================

        private AnimationEventData CreateEventData(AnimationEventType eventType)
        {
            // Get current animator state
            if (animator != null && animator.isActiveAndEnabled)
            {
                currentStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            }

            var eventData = new AnimationEventData
            {
                eventType = eventType,
                eventName = eventType.ToString(),
                timestamp = Time.time,

                // Animation state
                animationState = animController.CurrentState,
                direction = animController.CurrentDirection,
                normalizedTime = currentStateInfo.normalizedTime,

                // Transform
                position = transform.position,
                forward = GetForwardVector(),
                rotation = transform.rotation
            };

            return eventData;
        }

        // ==========================================
        // HELPER METHODS
        // ==========================================

        private ToolType? GetCurrentToolType()
        {
            // Map AnimationState → ToolType
            return animController.CurrentState switch
            {
                AnimationState.Hoeing => ToolType.Hoe,
                AnimationState.Watering => ToolType.Watering,
                AnimationState.Sickle => ToolType.Sickle,
                AnimationState.PickUpIdle => ToolType.PickUpIdle,
                AnimationState.PickUpRun => ToolType.PickUpRun,
                _ => null
            };
        }

        private Vector3 GetImpactPoint()
        {
            // Return impact point based on direction
            Transform impactTransform = animController.CurrentDirection switch
            {
                Direction.Down => impactPointDown,
                Direction.Up => impactPointUp,
                Direction.Side => impactPointSide,
                _ => impactPointDown
            };

            if (impactTransform != null)
            {
                // Flip side impact point if facing left
                if (animController.CurrentDirection == Direction.Side && animController.IsFacingLeft)
                {
                    Vector3 pos = impactTransform.position;
                    pos.x = transform.position.x - impactPointOffset;
                    return pos;
                }

                return impactTransform.position;
            }

            // Fallback: calculate from direction
            return transform.position + GetForwardVector() * impactPointOffset;
        }

        private Vector3 GetImpactNormal()
        {
            // Normal vector pointing away from impact point
            return -GetForwardVector();
        }

        private Vector3 GetForwardVector()
        {
            return animController.CurrentDirection switch
            {
                Direction.Down => Vector3.down,
                Direction.Up => Vector3.up,
                Direction.Side => animController.IsFacingLeft ? Vector3.left : Vector3.right,
                _ => Vector3.down
            };
        }

        // ==========================================
        // NOTIFICATION SYSTEM
        // ==========================================

        private void NotifyReceivers(AnimationEventData eventData)
        {
            // Notify all registered receivers
            foreach (var receiver in eventReceivers)
            {
                try
                {
                    if (eventData.eventType == AnimationEventType.ToolImpact)
                    {
                        receiver.OnToolImpact(eventData);
                    }
                    else
                    {
                        receiver.OnAnimationEvent(eventData);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[AnimationEventHandler] Error notifying receiver: {ex.Message}");
                }
            }
        }

        // ==========================================
        // SOUND SYSTEM (Mock/Placeholder)
        // ==========================================

        private void PlaySound(AnimationEventData eventData)
        {
            if (!enableSounds)
                return;

            // TODO: Integrate với Sound Manager thực tế
            // SoundManager.Instance.PlaySound(eventData.soundName, eventData.soundVolume);

            LogDebug($"[SOUND] Playing: {eventData.soundName} at volume {eventData.soundVolume}");
        }

        // ==========================================
        // PUBLIC API
        // ==========================================

        // Manually trigger event từ code (for testing)
        public void TriggerEvent(AnimationEventType eventType)
        {
            switch (eventType)
            {
                case AnimationEventType.ToolImpact:
                    OnToolImpact();
                    break;
                case AnimationEventType.Footstep:
                    OnFootstep();
                    break;
                case AnimationEventType.AnimationComplete:
                    OnAnimationComplete();
                    break;
                default:
                    Debug.LogWarning($"[AnimationEventHandler] Event type {eventType} not implemented for manual trigger");
                    break;
            }
        }

        // Get số lượng receivers đã register
        public int GetReceiverCount()
        {
            return eventReceivers.Count;
        }

        // ==========================================
        // DEBUG
        // ==========================================

        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[AnimEventHandler] {message}");
            }
        }

        private void LogEvent(string eventName, AnimationEventData data)
        {
            if (logAllEvents)
            {
                Debug.Log($"[AnimEvent] {eventName}: {data}");
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !debugMode)
                return;

            // Draw impact points
            if (impactPointDown != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(impactPointDown.position, 0.1f);
            }

            if (impactPointUp != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(impactPointUp.position, 0.1f);
            }

            if (impactPointSide != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(impactPointSide.position, 0.1f);
            }

            // Draw current impact point
            Vector3 currentImpact = GetImpactPoint();
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(currentImpact, 0.15f);
            Gizmos.DrawLine(transform.position, currentImpact);
        }

        [ContextMenu("Test - Trigger Tool Impact")]
        private void TestToolImpact()
        {
            OnToolImpact();
        }

        [ContextMenu("Test - Trigger Footstep")]
        private void TestFootstep()
        {
            OnFootstep();
        }

        [ContextMenu("Test - Play Test Sound")]
        private void TestPlaySound()
        {
            PlaySoundEffect("test_sound");
        }

        [ContextMenu("Debug - Log Receivers")]
        private void DebugLogReceivers()
        {
            Debug.Log($"=== REGISTERED RECEIVERS ({eventReceivers.Count}) ===");
            foreach (var receiver in eventReceivers)
            {
                Debug.Log($"- {receiver.GetType().Name}");
            }
        }
#endif
    }
}

