using System;
using TinyFarm.Farming;
using TinyFarm.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TinyFarm.Items.UI
{
    /// UI hiển thị thời gian và mùa theo style Stardew Valley
    /// Sử dụng sprite sheet với multiple frames cho Season và Day/Night
    public class TimeSeasonUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private Image backgroundPanel; // Bảng gỗ nền

        [Header("Season & Day/Night Icon (Sprite Sheet)")]
        [SerializeField] private Image seasonDayNightIcon;
        [SerializeField] private Sprite[] seasonDayNightSprites; // Array 15 sprites (hoặc 20)

        [Header("Sprite Sheet Layout")]
        [Tooltip("Số sprites trong 1 hàng (thường là 5)")]
        [SerializeField] private int spritesPerRow = 5;

        [Header("Day Display")]
        [SerializeField] private TextMeshProUGUI dayNumberText; // Số ngày lớn
        [SerializeField] private Image dayBackground; // Background cho số ngày

        [Header("Money Display")]
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private Image moneyIcon; // Icon coin

        [Header("Time Display")]
        [SerializeField] private TextMeshProUGUI timeText; // HH:MM

        [Header("Day/Night Settings")]
        [Tooltip("Giờ bắt đầu ban ngày (0-24)")]
        [SerializeField] private float dayStartHour = 6f;
        [Tooltip("Giờ bắt đầu buổi tối (0-24)")]
        [SerializeField] private float eveningStartHour = 18f;
        [Tooltip("Giờ bắt đầu ban đêm (0-24)")]
        [SerializeField] private float nightStartHour = 20f;

        [Header("Animation")]
        [SerializeField] private float bounceAmount = 5f;
        [SerializeField] private bool enableDayChangeAnimation = true;

        [Header("References")]
        [SerializeField] private PlayerWallet playerWallet;

        // Cache
        private Season currentSeason = Season.Spring;
        private int currentDay = -1;
        private TimeOfDay currentTimeOfDay = TimeOfDay.Morning;
        private RectTransform seasonIconRect;
        private RectTransform dayBackgroundRect;
        private Vector3 originalDayBackgroundPos;

        private readonly string[] weekdays =
        {
            "Sun", "Mon", "Tue", "Wed", "Thur", "Fri", "Sat"
        };

        /// Enum cho thời gian trong ngày (dựa vào sprite sheet)
        private enum TimeOfDay
        {
            Morning = 0,    // 6:00 - 12:00
            Afternoon = 1,  // 12:00 - 18:00
            Evening = 2,    // 18:00 - 20:00
            Night = 3,      // 20:00 - 6:00
            Midnight = 4    // Optional: deep night
        }

        private void Awake()
        {
            InitializeUI();

            if (seasonDayNightIcon != null)
            {
                seasonIconRect = seasonDayNightIcon.GetComponent<RectTransform>();
            }

            if (dayBackground != null)
            {
                dayBackgroundRect = dayBackground.GetComponent<RectTransform>();
                originalDayBackgroundPos = dayBackgroundRect.localPosition;
            }

            playerWallet = FindObjectOfType<PlayerWallet>();
        }

        private void Start()
        {
            ValidateSpriteSheet();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
            ForceUpdate();

            
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();

            if (playerWallet != null)
                playerWallet.OnGoldChanged -= UpdateMoney;
        }

        /// Validate sprite sheet có đủ sprites không
        private void ValidateSpriteSheet()
        {
            if (seasonDayNightSprites == null || seasonDayNightSprites.Length == 0)
            {
                Debug.LogError("TimeSeasonUI: Sprite sheet is empty! Please assign sprites.");
                return;
            }

            int expectedCount = 4 * spritesPerRow; // 4 seasons * 5 times = 20 sprites
            if (seasonDayNightSprites.Length < expectedCount)
            {
            }
        }

        /// Khởi tạo UI components
        private void InitializeUI()
        {
            // Setup background panel
            if (backgroundPanel != null)
            {
                backgroundPanel.type = Image.Type.Sliced;
            }

            if (playerWallet != null)
            {
                playerWallet.OnGoldChanged += UpdateMoney;

                // Update gold display
                UpdateMoney(playerWallet.CurrentGold);
            }
        }

        /// Subscribe to manager events
        private void SubscribeToEvents()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTimeChanged += UpdateTime;
                TimeManager.Instance.OnNewDay += OnNewDay;
            }

            if (SessionManager.Instance != null)
            {
                SessionManager.Instance.OnSessionChanged += UpdateSeasonAndDay;
            }
        }

        /// Unsubscribe from events
        private void UnsubscribeFromEvents()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTimeChanged -= UpdateTime;
                TimeManager.Instance.OnNewDay -= OnNewDay;
            }

            if (SessionManager.Instance != null)
            {
                SessionManager.Instance.OnSessionChanged -= UpdateSeasonAndDay;
            }
        }

        /// Update time display và icon dựa trên giờ
        private void UpdateTime(float dayTime)
        {
            if (TimeManager.Instance == null) return;

            // Update time text
            if (timeText != null)
            {
                timeText.text = TimeManager.Instance.GetTimeString();
            }

            // Tính giờ hiện tại (0-24)
            float currentHour = dayTime * 24f;

            // Determine time of day
            TimeOfDay newTimeOfDay = GetTimeOfDay(currentHour);

            // Update icon nếu time of day thay đổi
            if (newTimeOfDay != currentTimeOfDay)
            {
                currentTimeOfDay = newTimeOfDay;
                UpdateSeasonDayNightIcon();
            }
        }

        // Xác định thời gian trong ngày dựa trên giờ
        private TimeOfDay GetTimeOfDay(float hour)
        {
            if (hour >= nightStartHour || hour < dayStartHour)
                return TimeOfDay.Night;
            else if (hour >= eveningStartHour)
                return TimeOfDay.Evening;
            else if (hour >= 12f)
                return TimeOfDay.Afternoon;
            else
                return TimeOfDay.Morning;
        }

        /// Update season icon và day number
        private void UpdateSeasonAndDay()
        {
            if (SessionManager.Instance == null) return;

            Session session = SessionManager.Instance.GetCurrentSession();
            if (session == null) return;

            // Update season
            Season newSeason = session.currentSeason;
            bool seasonChanged = newSeason != currentSeason;
            currentSeason = newSeason;

            // Update day number
            int newDay = session.currentDay;
            bool dayChanged = newDay != currentDay;
            currentDay = newDay;

            if (dayNumberText != null)
            {
                // Xác định thứ trong tuần
                int weekdayIndex = (currentDay - 1) % 7;
                string weekdayName = weekdays[weekdayIndex];

                // Hiển thị "Thứ X - Ngày Y"
                dayNumberText.text = $"{weekdayName}, {currentDay}";

                if (dayChanged && enableDayChangeAnimation)
                {
                    StartCoroutine(AnimateDayChange());
                }
            }

            // Update icon
            if (seasonChanged || dayChanged)
            {
                UpdateSeasonDayNightIcon(seasonChanged);
            }
        }

        /// Update sprite icon dựa trên Season và TimeOfDay
        /// Layout sprite sheet: 
        /// Row 0: Spring (Morning, Afternoon, Evening, Night, Midnight)
        /// Row 1: Summer (Morning, Afternoon, Evening, Night, Midnight)
        /// Row 2: Fall (Morning, Afternoon, Evening, Night, Midnight)
        /// Row 3: Winter (Morning, Afternoon, Evening, Night, Midnight)
        private void UpdateSeasonDayNightIcon(bool animated = false)
        {
            if (seasonDayNightIcon == null || seasonDayNightSprites == null || seasonDayNightSprites.Length == 0)
                return;

            // Calculate sprite index
            int seasonIndex = (int)currentSeason; // 0-3
            int timeIndex = (int)currentTimeOfDay; // 0-4
            int spriteIndex = seasonIndex * spritesPerRow + timeIndex;

            // Clamp to valid range
            spriteIndex = Mathf.Clamp(spriteIndex, 0, seasonDayNightSprites.Length - 1);

            Sprite newSprite = seasonDayNightSprites[spriteIndex];

            if (newSprite == null)
            {
                Debug.LogWarning($"TimeSeasonUI: Sprite at index {spriteIndex} is null!");
                return;
            }

            if (animated && enableDayChangeAnimation)
            {
                StartCoroutine(AnimateIconChange(newSprite));
            }
            else
            {
                seasonDayNightIcon.sprite = newSprite;
            }
        }

        /// Animation khi đổi icon (fade hoặc scale)
        private System.Collections.IEnumerator AnimateIconChange(Sprite newSprite)
        {
            if (seasonIconRect == null) yield break;

            float duration = 0.3f;
            float elapsed = 0f;

            Vector3 originalScale = seasonIconRect.localScale;

            // Scale down
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                seasonIconRect.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
                yield return null;
            }

            // Change sprite
            seasonDayNightIcon.sprite = newSprite;

            // Scale up
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                seasonIconRect.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
                yield return null;
            }

            seasonIconRect.localScale = originalScale;
        }

        /// Animation khi đổi ngày (bounce effect)
        private System.Collections.IEnumerator AnimateDayChange()
        {
            if (dayBackgroundRect == null) yield break;

            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Bounce curve
                float bounce = Mathf.Sin(t * Mathf.PI * 2) * bounceAmount * (1 - t);
                Vector3 newPos = originalDayBackgroundPos + new Vector3(0, bounce, 0);

                dayBackgroundRect.localPosition = newPos;

                // Scale pulse
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.15f;
                dayBackgroundRect.localScale = Vector3.one * scale;

                yield return null;
            }

            dayBackgroundRect.localPosition = originalDayBackgroundPos;
            dayBackgroundRect.localScale = Vector3.one;
        }

        /// Handle new day event
        private void OnNewDay()
        {
            UpdateSeasonAndDay();
        }

        /// Update money display
        private void UpdateMoney(int amount)
        {
            if (moneyText != null)
            {
                moneyText.text = amount.ToString();
            }
        }

        /// Force update all displays
        public void ForceUpdate()
        {
            UpdateTime(TimeManager.Instance?.CurrentDayTime ?? 0f);
            UpdateSeasonAndDay();
        }

        // /// Set money amount với animation
        // public void SetMoney(int amount)
        // {
        //     int oldMoney = currentMoney;
        //     currentMoney = amount;

        //     if (moneyText != null)
        //     {
        //         if (amount != oldMoney)
        //         {
        //             StartCoroutine(AnimateMoneyChange(oldMoney, amount));
        //         }
        //     }
        // }

        /// Animate money change với counter effect
        // private System.Collections.IEnumerator AnimateMoneyChange(int from, int to)
        // {
        //     float duration = 0.5f;
        //     float elapsed = 0f;

        //     while (elapsed < duration)
        //     {
        //         elapsed += Time.deltaTime;
        //         float t = elapsed / duration;

        //         int current = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
        //         yield return null;
        //     }
        // }

#if UNITY_EDITOR
        [ContextMenu("Test Day Change")]
        private void TestDayChange()
        {
            if (SessionManager.Instance != null)
            {
                SessionManager.Instance.IncrementDay();
            }
        }

        [ContextMenu("Test Change Season")]
        private void TestChangeSeason()
        {
            if (SessionManager.Instance != null)
            {
                Session session = SessionManager.Instance.GetCurrentSession();
                Season nextSeason = (Season)(((int)session.currentSeason + 1) % 4);
                SessionManager.Instance.ChangeSeason(nextSeason);
            }
        }

        [ContextMenu("Preview All Sprites")]
        private void PreviewAllSprites()
        {
            if (seasonDayNightSprites == null || seasonDayNightSprites.Length == 0)
            {
                Debug.LogError("No sprites assigned!");
                return;
            }

            Debug.Log($"Total sprites: {seasonDayNightSprites.Length}");
            for (int i = 0; i < seasonDayNightSprites.Length; i++)
            {
                int season = i / spritesPerRow;
                int time = i % spritesPerRow;
                Debug.Log($"Index {i}: Season={season}, Time={time}, Sprite={(seasonDayNightSprites[i] != null ? seasonDayNightSprites[i].name : "NULL")}");
            }
        }
#endif
    }
}

