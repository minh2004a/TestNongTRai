using System;
using TinyFarm.Farming;
using TinyFarm.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TinyFarm.Items.UI
{
    /// UI hiển thị thời gian và mùa theo style Stardew Valley
    /// Thiết kế: Bảng gỗ với icon mùa, ngày và tiền
    public class TimeSeasonUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private Image backgroundPanel; // Bảng gỗ nền
        [SerializeField] private Sprite woodPanelSprite;

        [Header("Season Icon")]
        [SerializeField] private Image seasonIcon;
        [SerializeField] private Sprite springIcon;
        [SerializeField] private Sprite summerIcon;
        [SerializeField] private Sprite fallIcon;
        [SerializeField] private Sprite winterIcon;

        [Header("Day Display")]
        [SerializeField] private TextMeshProUGUI dayNumberText; // Số ngày lớn
        [SerializeField] private Image dayBackground; // Background cho số ngày
        [SerializeField] private Color dayTextColor = new Color(0.2f, 0.1f, 0f); // Màu nâu đậm

        [Header("Money Display")]
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private Image moneyIcon; // Icon coin
        [SerializeField] private Sprite coinSprite;
        [SerializeField] private Color moneyTextColor = new Color(1f, 0.85f, 0f); // Màu vàng

        [Header("Time Display")]
        [SerializeField] private TextMeshProUGUI timeText; // HH:MM
        [SerializeField] private Image clockIcon;
        [SerializeField] private Sprite clockSprite;

        [Header("Layout Settings")]
        [SerializeField] private float iconSize = 80f;
        [SerializeField] private float spacing = 10f;
        [SerializeField] private Vector2 panelPadding = new Vector2(15f, 10f);

        [Header("Animation")]
        [SerializeField] private float bounceAmount = 5f;
        [SerializeField] private float bounceSpeed = 2f;
        [SerializeField] private bool enableDayChangeAnimation = true;

        // Cache
        private Season currentSeason;
        private int currentDay = -1;
        private int currentMoney = 0;
        private RectTransform seasonIconRect;
        private RectTransform dayBackgroundRect;
        private Vector3 originalDayBackgroundPos;

        private void Awake()
        {
            InitializeUI();

            if (seasonIcon != null)
            {
                seasonIconRect = seasonIcon.GetComponent<RectTransform>();
            }

            if (dayBackground != null)
            {
                dayBackgroundRect = dayBackground.GetComponent<RectTransform>();
                originalDayBackgroundPos = dayBackgroundRect.localPosition;
            }
        }

        private void Start()
        {
            SetupInitialStyle();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
            ForceUpdate();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// Khởi tạo UI components
        /// </summary>
        private void InitializeUI()
        {
            // Setup background panel
            if (backgroundPanel != null && woodPanelSprite != null)
            {
                backgroundPanel.sprite = woodPanelSprite;
                backgroundPanel.type = Image.Type.Sliced;
            }

            // Setup day text style
            if (dayNumberText != null)
            {
                dayNumberText.color = dayTextColor;
                dayNumberText.fontSize = 48;
                dayNumberText.fontStyle = FontStyles.Bold;
                dayNumberText.alignment = TextAlignmentOptions.Center;
            }

            // Setup money text style
            if (moneyText != null)
            {
                moneyText.color = moneyTextColor;
                moneyText.fontSize = 28;
                moneyText.fontStyle = FontStyles.Bold;
                moneyText.alignment = TextAlignmentOptions.Left;
            }

            // Setup time text style
            if (timeText != null)
            {
                timeText.color = new Color(0.9f, 0.9f, 0.9f);
                timeText.fontSize = 24;
                timeText.alignment = TextAlignmentOptions.Center;
            }

            // Setup icons
            if (moneyIcon != null && coinSprite != null)
            {
                moneyIcon.sprite = coinSprite;
            }

            if (clockIcon != null && clockSprite != null)
            {
                clockIcon.sprite = clockSprite;
            }
        }

        /// <summary>
        /// Setup style ban đầu
        /// </summary>
        private void SetupInitialStyle()
        {
            // Set icon sizes
            if (seasonIconRect != null)
            {
                seasonIconRect.sizeDelta = new Vector2(iconSize, iconSize);
            }
        }

        /// <summary>
        /// Subscribe to manager events
        /// </summary>
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

        /// <summary>
        /// Unsubscribe from events
        /// </summary>
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

        /// <summary>
        /// Update time display
        /// </summary>
        private void UpdateTime(float dayTime)
        {
            if (timeText != null && TimeManager.Instance != null)
            {
                timeText.text = TimeManager.Instance.GetTimeString();
            }
        }

        /// <summary>
        /// Update season icon and day number
        /// </summary>
        private void UpdateSeasonAndDay()
        {
            if (SessionManager.Instance == null) return;

            Session session = SessionManager.Instance.GetCurrentSession();
            if (session == null) return;

            // Update season icon
            Season newSeason = session.currentSeason;
            bool seasonChanged = newSeason != currentSeason;
            currentSeason = newSeason;

            if (seasonIcon != null)
            {
                Sprite newIcon = GetSeasonIcon(currentSeason);

                if (seasonChanged && enableDayChangeAnimation)
                {
                    StartCoroutine(AnimateSeasonChange(newIcon));
                }
                else
                {
                    seasonIcon.sprite = newIcon;
                }
            }

            // Update day number
            int newDay = session.currentDay;
            bool dayChanged = newDay != currentDay;
            currentDay = newDay;

            if (dayNumberText != null)
            {
                dayNumberText.text = currentDay.ToString();

                if (dayChanged && enableDayChangeAnimation)
                {
                    StartCoroutine(AnimateDayChange());
                }
            }
        }

        /// <summary>
        /// Update money display
        /// </summary>
        public void UpdateMoney(int amount)
        {
            currentMoney = amount;

            if (moneyText != null)
            {
                moneyText.text = FormatMoney(currentMoney);
            }
        }

        /// <summary>
        /// Format số tiền với dấu phẩy
        /// </summary>
        private string FormatMoney(int amount)
        {
            return amount.ToString("N0").Replace(",", " "); // 3500 -> "3 500"
        }

        /// <summary>
        /// Get season icon sprite
        /// </summary>
        private Sprite GetSeasonIcon(Season season)
        {
            return season switch
            {
                Season.Spring => springIcon,
                Season.Summer => summerIcon,
                Season.Fall => fallIcon,
                Season.Winter => winterIcon,
                _ => springIcon
            };
        }

        /// <summary>
        /// Animation khi đổi mùa (rotate và scale)
        /// </summary>
        private System.Collections.IEnumerator AnimateSeasonChange(Sprite newIcon)
        {
            if (seasonIconRect == null) yield break;

            float duration = 0.4f;
            float elapsed = 0f;

            Vector3 originalScale = seasonIconRect.localScale;
            Vector3 originalRotation = seasonIconRect.localEulerAngles;

            // Rotate and scale down
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                seasonIconRect.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
                seasonIconRect.localEulerAngles = Vector3.Lerp(originalRotation, new Vector3(0, 0, 180), t);

                yield return null;
            }

            // Change sprite
            seasonIcon.sprite = newIcon;

            // Rotate back and scale up
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                seasonIconRect.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
                seasonIconRect.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, 180), new Vector3(0, 0, 360), t);

                yield return null;
            }

            seasonIconRect.localScale = originalScale;
            seasonIconRect.localEulerAngles = originalRotation;
        }

        /// <summary>
        /// Animation khi đổi ngày (bounce effect)
        /// </summary>
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

        /// <summary>
        /// Handle new day event
        /// </summary>
        private void OnNewDay()
        {
            UpdateSeasonAndDay();
        }

        /// <summary>
        /// Force update all displays
        /// </summary>
        public void ForceUpdate()
        {
            UpdateTime(TimeManager.Instance?.CurrentDayTime ?? 0f);
            UpdateSeasonAndDay();

            // Update money if you have player data
            // UpdateMoney(PlayerData.Instance?.Money ?? 0);
        }

        /// <summary>
        /// Set money amount (call this from inventory/shop systems)
        /// </summary>
        public void SetMoney(int amount)
        {
            int oldMoney = currentMoney;
            currentMoney = amount;

            if (moneyText != null)
            {
                moneyText.text = FormatMoney(currentMoney);

                // Optional: animate money change
                if (amount != oldMoney)
                {
                    StartCoroutine(AnimateMoneyChange(oldMoney, amount));
                }
            }
        }

        /// <summary>
        /// Animate money change with counter effect
        /// </summary>
        private System.Collections.IEnumerator AnimateMoneyChange(int from, int to)
        {
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                int current = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
                moneyText.text = FormatMoney(current);

                yield return null;
            }

            moneyText.text = FormatMoney(to);
        }

#if UNITY_EDITOR
        [ContextMenu("Test Day Change")]
        private void TestDayChange()
        {
            if (SessionManager.Instance != null)
            {
                SessionManager.Instance.IncrementDay();
            }
        }

        [ContextMenu("Test Add Money")]
        private void TestAddMoney()
        {
            SetMoney(currentMoney + 500);
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
#endif
    }
}
