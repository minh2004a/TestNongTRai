using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Animation
{
    public interface IAnimationEventReceiver
    {
        // Called khi tool impact (áp dụng gameplay effect)
        void OnToolImpact(AnimationEventData eventData);

        // Called khi animation complete (cleanup, transitions)
        void OnAnimationComplete(AnimationEventData eventData);

        // Called cho generic animation events
        void OnAnimationEvent(AnimationEventData eventData);
    }
}

