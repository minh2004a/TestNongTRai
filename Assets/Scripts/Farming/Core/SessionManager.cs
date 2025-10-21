using System;
using System.Collections;
using System.Collections.Generic;
using TinyFarm.Items;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;

namespace TinyFarm.Farming
{
    // Quản lý session game (save/load data)
    public class SessionManager : MonoBehaviour
    {
        // Singleton pattern
        public static SessionManager Instance { get; private set; }

        [Header("Session Data")]
        [SerializeField] private Session currentSession;

        // Session configuration
        [SerializeField] private int daysInSeason = 28;
        [SerializeField] private float sessionLength = 600f; // 10 phút mặc định

        // Events
        public event Action<Session> OnSessionLoaded;
        public event Action<Session> OnSessionSaved;
        public event Action OnSessionChanged;

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitializeSession();
        }

        /// Khởi tạo session mới hoặc load session cũ
        private void InitializeSession()
        {
            if (currentSession == null)
            {
                CreateNewSession();
            }
            else
            {
                LoadSession();
            }
        }

        /// Tạo session mới
        public void CreateNewSession()
        {
            currentSession = new Session
            {
                sessionId = Guid.NewGuid().ToString(),
                createdTime = DateTime.Now,
                lastPlayedTime = DateTime.Now,
                totalPlayTime = 0f,
                currentDay = 1,
                currentSeason = Season.Spring
            };

            Debug.Log($"Created new session: {currentSession.sessionId}");
            OnSessionChanged?.Invoke();
        }

        /// Load session từ PlayerPrefs hoặc file
        public void LoadSession()
        {
            // TODO: Implement actual save/load logic
            // Tạm thời dùng PlayerPrefs hoặc JSON file
            Debug.Log("Loading session...");
            OnSessionLoaded?.Invoke(currentSession);
        }

        /// Lưu session hiện tại
        public void SaveSession()
        {
            if (currentSession != null)
            {
                currentSession.lastPlayedTime = DateTime.Now;
                // TODO: Implement actual save logic
                Debug.Log($"Saving session: {currentSession.sessionId}");
                OnSessionSaved?.Invoke(currentSession);
            }
        }

        /// Lấy session hiện tại
        public Session GetCurrentSession()
        {
            return currentSession;
        }

        /// Cập nhật thời gian chơi
        public void IncrementPlayTime(float deltaTime)
        {
            if (currentSession != null)
            {
                currentSession.totalPlayTime += deltaTime;
            }
        }

        /// Thay đổi mùa
        public void ChangeSeason(Season newSeason)
        {
            if (currentSession != null)
            {
                currentSession.currentSeason = newSeason;
                currentSession.currentDay = 1;
                OnSessionChanged?.Invoke();
            }
        }

        /// Tăng ngày
        public void IncrementDay()
        {
            if (currentSession != null)
            {
                currentSession.currentDay++;

                // Check nếu hết mùa
                if (currentSession.currentDay > daysInSeason)
                {
                    Season nextSeason = GetNextSeason(currentSession.currentSeason);
                    ChangeSeason(nextSeason);
                }

                OnSessionChanged?.Invoke();
            }
        }

        /// Lấy mùa tiếp theo
        private Season GetNextSeason(Season current)
        {
            return current switch
            {
                Season.Spring => Season.Summer,
                Season.Summer => Season.Fall,
                Season.Fall => Season.Winter,
                Season.Winter => Season.Spring,
                _ => Season.Spring
            };
        }

        private void OnApplicationQuit()
        {
            SaveSession();
        }
    }

}
