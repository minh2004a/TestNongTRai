using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TinyFarm.Items.UI
{
    public class InventoryTabController : MonoBehaviour
    {
        [Header("Tab Panels")]
        [SerializeField] private GameObject inventoryMainPanel;
        [SerializeField] private GameObject inventoryCropPanel;
        [SerializeField] private GameObject inventoryToolPanel;

        [Header("Tab Buttons")]
        [SerializeField] private Button buttonAll;
        [SerializeField] private Button buttonCrop;
        [SerializeField] private Button buttonTool;

        [Header("Button Visual Settings")]
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        // Optional: Nếu muốn scale button khi active
        [SerializeField] private float activeScale = 1.1f;
        [SerializeField] private float inactiveScale = 1f;

        [Header("Tab Content (Optional)")]
        [Tooltip("Nếu có FilteredInventoryUI, gán vào đây để auto refresh")]
        [SerializeField] private FilteredInventoryUI allInventoryUI;
        [SerializeField] private FilteredInventoryUI seedInventoryUI;
        [SerializeField] private FilteredInventoryUI toolInventoryUI;

        private Dictionary<Button, GameObject> buttonPanelMap;
        private Button currentActiveButton;

        private class TabData
        {
            public GameObject panel;
            public FilteredInventoryUI inventoryUI;

            public TabData(GameObject panel, FilteredInventoryUI ui)
            {
                this.panel = panel;
                this.inventoryUI = ui;
            }
        }

        private void Awake()
        {
            InitializeTabSystem();
        }

        private void Start()
        {
            // Mở tab All mặc định
            SwitchTab(buttonAll);
            //SwitchTab(buttonCrop);
            //SwitchTab(buttonTool);
        }

        private void InitializeTabSystem()
        {
            // Map buttons với panels tương ứng
            buttonPanelMap = new Dictionary<Button, GameObject>
            {
                { buttonAll, inventoryMainPanel },
                { buttonCrop, inventoryCropPanel },
                { buttonTool, inventoryToolPanel }
            };

            // Subscribe button events
            if (buttonAll != null)
                buttonAll.onClick.AddListener(() => SwitchTab(buttonAll));

            if (buttonCrop != null)
                buttonCrop.onClick.AddListener(() => SwitchTab(buttonCrop));

            if (buttonTool != null)
                buttonTool.onClick.AddListener(() => SwitchTab(buttonTool));

            Debug.Log("[InventoryTabController] Initialized with " + buttonPanelMap.Count + " tabs");
        }

        public void SwitchTab(Button targetButton)
        {
            if (!buttonPanelMap.ContainsKey(targetButton))
            {
                Debug.LogWarning("[InventoryTabController] Button not found in map!");
                return;
            }

            // Deactivate tất cả panels
            foreach (var panel in buttonPanelMap.Values)
            {
                if (panel != null)
                    panel.SetActive(false);
            }

            // Reset tất cả buttons về inactive state
            foreach (var button in buttonPanelMap.Keys)
            {
                SetButtonVisual(button, false);
            }

            // Activate panel và button được chọn
            GameObject targetPanel = buttonPanelMap[targetButton];
            if (targetPanel != null)
            {
                targetPanel.SetActive(true);
                SetButtonVisual(targetButton, true);
                currentActiveButton = targetButton;

                Debug.Log($"[InventoryTabController] Switched to {targetPanel.name}");
            }
        }

        private void SetButtonVisual(Button button, bool isActive)
        {
            if (button == null) return;

            // Đổi màu
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = isActive ? activeColor : inactiveColor;
            }

            // Đổi scale (optional)
            button.transform.localScale = Vector3.one * (isActive ? activeScale : inactiveScale);

            // Có thể thêm animation khác nếu muốn
            // Ví dụ: đổi sprite, glow effect, etc.
        }

        // Public methods để switch tab từ code khác
        public void SwitchToAll() => SwitchTab(buttonAll);
        public void SwitchToSeed() => SwitchTab(buttonCrop);
        public void SwitchToTool() => SwitchTab(buttonTool);

        // Keyboard shortcuts (optional)
        

        private void OnDestroy()
        {
            // Cleanup
            if (buttonAll != null)
                buttonAll.onClick.RemoveAllListeners();

            if (buttonCrop != null)
                buttonCrop.onClick.RemoveAllListeners();

            if (buttonTool != null)
                buttonTool.onClick.RemoveAllListeners();
        }
    }
}

