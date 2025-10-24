using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Animation
{
    public enum AnimationEventType
    {
        None = 0,

        // Tool Events
        ToolImpact = 10,        // Tool chạm target (trigger gameplay logic)
        ToolSwingStart = 11,    // Bắt đầu vung tool
        ToolSwingEnd = 12,      // Kết thúc vung tool

        // Movement Events
        Footstep = 20,          // Chân chạm đất (play sound)

        // Sound Events
        PlaySound = 30,         // Play sound effect tại frame cụ thể

        // Animation Lifecycle
        AnimationStart = 40,    // Animation bắt đầu
        AnimationComplete = 41, // Animation kết thúc
        AnimationLoop = 42      // Animation loop (nếu có)
    }
}

