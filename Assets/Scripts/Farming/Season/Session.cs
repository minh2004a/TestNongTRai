using System;
using System.Collections;
using System.Collections.Generic;
using TinyFarm.Items;
using UnityEngine;

namespace TinyFarm.Farming
{
    [Serializable]
    public class Session
    {
        public string sessionId;
        public DateTime createdTime;
        public DateTime lastPlayedTime;
        public float totalPlayTime;
        public int currentDay;
        public Season currentSeason;
    }
}

