using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.PlayerInput
{
    // Struct để lưu buffered input
    public struct BufferedInput
    {
        public InputAction action;
        public float timestamp;
        public object data;

        public BufferedInput(InputAction action, float timestamp, object data = null)
        {
            this.action = action;
            this.timestamp = timestamp;
            this.data = data;
        }

        public bool IsExpired(float currentTime, float duration)
        {
            return (currentTime - timestamp) > duration;
        }
    }
}


