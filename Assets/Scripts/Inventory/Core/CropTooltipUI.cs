using System.Collections;
using System.Collections.Generic;
using TinyFarm.Crops;
using TinyFarm.Farming;
using TMPro;
using UnityEngine;

namespace TinyFarm.UI
{
    public class CropTooltipUI : MonoBehaviour
    {
        public static CropTooltipUI Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Canvas canvas;
        [SerializeField] private Camera mainCamera;

        [Header("Text Fields")]
        [SerializeField] private TextMeshProUGUI cropNameText;
        [SerializeField] private TextMeshProUGUI growthText;
        [SerializeField] private TextMeshProUGUI fertilizerText;

        [Header("Settings")]
        [SerializeField] private Vector2 offset = new(200f, 0f);
        [SerializeField] private float fadeSpeed = 40f;

        private CanvasGroup canvasGroup;
        private CropInstance currentCrop;
        private bool isVisible;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();

            if (mainCamera == null)
                mainCamera = Camera.main;

            if (panel != null)
            {
                canvasGroup = panel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = panel.AddComponent<CanvasGroup>();
            }

            Hide();
        }

        private void Update()
        {
            if (!isVisible) return;

            UpdatePosition();
            if (currentCrop != null)
                UpdateContent();

            float targetAlpha = isVisible ? 1f : 0f;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        }

        public void Show(CropInstance crop)
        {
            if (crop == null || panel == null) return;

            currentCrop = crop;

            panel.SetActive(true); // 🔥 Bắt buộc phải có

            if (canvasGroup == null)
                canvasGroup = panel.GetComponent<CanvasGroup>();

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            isVisible = true;
            UpdateContent();
        }

        private void UpdateContent()
        {
            if (currentCrop == null || currentCrop.Data == null)
            {
                Hide();
                return;
            }

            var data = currentCrop.Data;
            string name = data.cropName;
            string growth;

            if (currentCrop.IsHarvestable)
                growth = "Sẵn sàng thu hoạch!";
            else
                growth = $"Giai đoạn {currentCrop.CurrentStage + 1}/{data.growthStages}";

            string fert = currentCrop.fertilizer == FertilizerType.None
                ? "Chưa bón phân"
                : $"Phân bón: {currentCrop.fertilizer}";

            cropNameText.text = $"<b>{name}</b>";
            growthText.text = growth;
            fertilizerText.text = fert;
        }

        private void UpdatePosition()
        {
            if (rectTransform == null || canvas == null) return;

            Vector2 mousePos = Input.mousePosition;
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                mousePos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                out localPoint
            );

            rectTransform.anchoredPosition = Vector2.Lerp(
                rectTransform.anchoredPosition,
                localPoint + offset,
                Time.deltaTime * fadeSpeed
            );
        }

         public void Hide()
        {
            if (panel == null) return;

            if (canvasGroup == null)
                canvasGroup = panel.GetComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            panel.SetActive(false);
        }
    }
}

