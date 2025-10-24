using System.Collections;
using System.Collections.Generic;
using TinyFarm.Items;
using UnityEngine;

namespace TinyFarm.Animation
{
    // Detailed data structure cho animation events
    // Chứa tất cả thông tin cần thiết cho gameplay logic
    public class AnimationEventData
    {
        // Event info
        public AnimationEventType eventType;
        public string eventName;
        public float timestamp;

        // Animation state
        public AnimationState animationState;
        public Direction direction;
        public float normalizedTime; // 0-1

        // Transform info
        public Vector3 position;
        public Vector3 forward;
        public Quaternion rotation;

        // Tool info (nếu là tool event)
        public ToolType? toolType;
        public Vector3? impactPoint;
        public Vector3? impactNormal;

        // Sound info (nếu là sound event)
        public string soundName;
        public float soundVolume;

        // Additional data
        public object customData;

        public AnimationEventData()
        {
            timestamp = Time.time;
            normalizedTime = 0f;
            soundVolume = 1f;
        }

        public override string ToString()
        {
            return $"[{eventType}] State={animationState}, Dir={direction}, Time={normalizedTime:F2}";
        }
    }
}

