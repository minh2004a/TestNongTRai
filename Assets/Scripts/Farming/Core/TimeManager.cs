using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Farming
{
    public class TimeManager : MonoBehaviour
    {
        // Singleton pattern
        public static TimeManager Instance { get; private set; }

        [Header("Time Settings")]
        [SerializeField] private float dayDuration = 60f; // 1 ngày game = 60 giây thực
        [SerializeField] private float currentDayTime = 0f; // 0-1 (0 = midnight, 0.5 = noon)

        [Header("Day/Night Settings")]
        [SerializeField] private float dayTimeStart = 0.25f;   // 6:00 AM
        [SerializeField] private float dayTimeEnd = 0.75f;     // 6:00 PM

        // Time tracking
        private float timeElapsed = 0f;
        private int currentDay = 1;

        // Events
        public event Action OnNewDay;
        public event Action OnDayStart;
        public event Action OnNightStart;
        public event Action<float> OnTimeChanged; // Gửi currentDayTime (0-1)

        // Properties
        public float CurrentDayTime => currentDayTime;
        public int CurrentDay => currentDay;
        public bool IsDaytime => currentDayTime >= dayTimeStart && currentDayTime <= dayTimeEnd;
        public float DayProgress => currentDayTime;

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Sync với SessionManager nếu có
            if (SessionManager.Instance != null)
            {
                currentDay = SessionManager.Instance.GetCurrentSession().currentDay;
            }
        }

        private void Update()
        {
            UpdateTime(Time.deltaTime);
        }

        /// Cập nhật thời gian
        private void UpdateTime(float deltaTime)
        {
            timeElapsed += deltaTime;

            // Tính thời gian trong ngày (0-1)
            float previousDayTime = currentDayTime;
            currentDayTime = (timeElapsed % dayDuration) / dayDuration;

            // Check chuyển ngày
            if (currentDayTime < previousDayTime)
            {
                OnDayComplete();
            }

            // Check chuyển day/night
            CheckDayNightTransition(previousDayTime, currentDayTime);

            // Trigger event
            OnTimeChanged?.Invoke(currentDayTime);
        }

        /// Xử lý khi hết ngày
        private void OnDayComplete()
        {
            currentDay++;

            // Sync với SessionManager
            if (SessionManager.Instance != null)
            {
                SessionManager.Instance.IncrementDay();
            }

            OnNewDay?.Invoke();
            Debug.Log($"New Day: {currentDay}");
        }

        /// Check chuyển đổi ngày/đêm
        private void CheckDayNightTransition(float previousTime, float currentTime)
        {
            // Chuyển sang ban ngày
            if (previousTime < dayTimeStart && currentTime >= dayTimeStart)
            {
                OnDayStart?.Invoke();
                Debug.Log("Day started");
            }

            // Chuyển sang ban đêm
            if (previousTime < dayTimeEnd && currentTime >= dayTimeEnd)
            {
                OnNightStart?.Invoke();
                Debug.Log("Night started");
            }
        }

        /// Lấy thời gian hiện tại dạng string (HH:MM)
        public string GetTimeString()
        {
            int totalMinutes = Mathf.FloorToInt(currentDayTime * 1440); // 1440 minutes in a day
            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;
            return $"{hours:00}:{minutes:00}";
        }

        /// Skip đến thời gian cụ thể
        public void SkipToTime(float targetTime)
        {
            if (targetTime >= 0 && targetTime <= 1)
            {
                currentDayTime = targetTime;
                timeElapsed = currentDayTime * dayDuration;
                OnTimeChanged?.Invoke(currentDayTime);
            }
        }

        /// Tăng tốc thời gian
        public void SetTimeScale(float scale)
        {
            Time.timeScale = Mathf.Clamp(scale, 0f, 10f);
        }

        /// Pause thời gian
        public void PauseTime()
        {
            Time.timeScale = 0f;
        }

        /// Resume thời gian
        public void ResumeTime()
        {
            Time.timeScale = 1f;
        }
    }
}

