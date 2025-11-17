using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using TinyFarm.Items;

namespace TinyFarm.NPC
{
    // ShopItemSlotUI - Slot cho mỗi item trong shop list
    // Structure: SlotShop/Icon_Item, Text_Product, Text_Price, Icon_Gold
    public class ShopItemSlotUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI References")]
        [SerializeField] private Image itemIcon;              // Icon_Item
        [SerializeField] private TextMeshProUGUI productNameText; // Text_Product
        [SerializeField] private TextMeshProUGUI priceText;   // Text_Price
        [SerializeField] private Image goldIcon;              // Icon_Gold
        [SerializeField] private Image background;            // Background/Image
        
        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(0.9f, 0.85f, 0.75f);
        [SerializeField] private Color selectedColor = new Color(1f, 1f, 0.8f);
        
        private ShopItemEntry shopEntry;
        private int displayPrice;
        private bool isSelected = false;
        
        public System.Action OnClicked;

        private void Awake()
        {
            // Auto-find components if not assigned
            if (itemIcon == null)
                itemIcon = transform.Find("Icon_Item")?.GetComponent<Image>();
                
            if (productNameText == null)
                productNameText = transform.Find("Text_Product")?.GetComponent<TextMeshProUGUI>();
                
            if (priceText == null)
                priceText = transform.Find("Text_Price")?.GetComponent<TextMeshProUGUI>();
                
            if (goldIcon == null)
                goldIcon = transform.Find("Icon_Gold")?.GetComponent<Image>();
                
            if (background == null)
                background = GetComponent<Image>();
        }

        // Setup shop item slot
        public void Setup(ShopItemEntry entry, int price)
        {
            shopEntry = entry;
            displayPrice = price;
            
            if (entry?.itemData == null)
            {
                Debug.LogWarning("[ShopItemSlot] Setup called with null entry or itemData");
                return;
            }
            
            UpdateDisplay();
        }
        
        private void UpdateDisplay()
        {
            if (shopEntry?.itemData == null) return;
            
            ItemData itemData = shopEntry.itemData;
            
            // Update icon
            if (itemIcon != null)
            {
                itemIcon.sprite = itemData.icon;
                itemIcon.enabled = itemData.icon != null;
            }
            
            // Update product name
            if (productNameText != null)
            {
                productNameText.text = itemData.itemName;
            }
            
            // Update price
            if (priceText != null)
            {
                priceText.text = displayPrice.ToString();
            }
            
            // Update background color
            if (background != null)
            {
                background.color = normalColor;
            }
            
            // Show/hide based on stock
            bool hasStock = shopEntry.HasStock();
            gameObject.SetActive(hasStock);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClicked?.Invoke();
            SetSelected(true);
        }
        
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            
            if (background != null)
            {
                background.color = selected ? selectedColor : normalColor;
            }
        }
        
        private void OnDisable()
        {
            // Reset selection when disabled
            SetSelected(false);
        }
    }
}

