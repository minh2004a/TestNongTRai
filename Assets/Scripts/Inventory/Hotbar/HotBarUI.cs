using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace TinyFarm.Items.UI
{
    public class HotBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private GameObject hotBarSlotPrefab;

        [Header("Settings")]
        [SerializeField] private int hotBarSize = 10;
        [SerializeField] private bool autoCreateSlots = true;

        private List<HotbarSlotUI> slotUIs = new List<HotbarSlotUI>();
        private int selectedSlotIndex = 0;

        public event Action<int> OnSlotSelected;

        public int SelectedSlotIndex => selectedSlotIndex;
        public InventorySlot SelectedInventorySlot =>
            inventoryManager.GetHotbarSlot(selectedSlotIndex);

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (inventoryManager == null)
            {
                Debug.LogError("[HotBarUI] Missing InventoryManager!");
                return;
            }

            if (autoCreateSlots) CreateSlotUIs();

            SelectSlot(0);
        }

        public List<HotbarSlotUI> GetAllSlotUIs()
        {
            return slotUIs;
        }
        private void CreateSlotUIs()
        {
            slotUIs.Clear();
            foreach (Transform child in slotsContainer)
                Destroy(child.gameObject);

            for (int i = 0; i < hotBarSize; i++)
            {
                InventorySlot slot = inventoryManager.GetHotbarSlot(i);
                CreateSlotUI(slot, i);
            }
        }

        private void CreateSlotUI(InventorySlot slot, int index)
        {
            GameObject slotGO = Instantiate(hotBarSlotPrefab, slotsContainer);
            HotbarSlotUI slotUI = slotGO.GetComponent<HotbarSlotUI>();
            
            slotUI.Initialize(slot, index);
            slotUI.OnSlotClicked += OnSlotUIClicked;

            // ✅ Gán inventoryManager cho DragDropHandler
            var dragHandler = slotGO.GetComponent<DragDropHandler>();
            if (dragHandler != null)
                dragHandler.inventoryManager = inventoryManager;

            slotUIs.Add(slotUI);
        }

        public void SelectSlot(int index)
        {
            if (index < 0 || index >= slotUIs.Count) return;

            slotUIs[selectedSlotIndex].Deselect();

            selectedSlotIndex = index;
            slotUIs[selectedSlotIndex].Select();

            OnSlotSelected?.Invoke(selectedSlotIndex);
        }

        public void UpdateUI()
        {
            for (int i = 0; i < slotUIs.Count; i++)
                slotUIs[i].UpdateUI();
        }

        private void OnSlotUIClicked(HotbarSlotUI ui)
        {
            int idx = slotUIs.IndexOf(ui);
            SelectSlot(idx);
        }

        private void OnDestroy()
        {
            foreach (var slotUI in slotUIs)
                if (slotUI != null)
                    slotUI.OnSlotClicked -= OnSlotUIClicked;
        }
    }
}

