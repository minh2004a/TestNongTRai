using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI quantityText;

    private InventorySlot currentSlot;

    private void Awake()
    {
        if (iconImage == null)
            iconImage = transform.Find("Icon").GetComponentInChildren<Image>();

        if (quantityText == null)
            quantityText = transform.Find("Quantity").GetComponentInChildren<TextMeshProUGUI>();

        if (quantityText == null)
            Debug.LogError($"[{gameObject.name}] Không tìm thấy Quantity Text!", this);
    }

    

    public void SetSlot(InventorySlot slot)
    {
        currentSlot = slot;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (currentSlot == null || currentSlot.IsEmpty())
        {
            // Slot trống
            if (iconImage != null)
                iconImage.sprite = null;
                iconImage.enabled = false;

            if (quantityText != null)
                quantityText.text = "";
        }
        else
        {
            if (iconImage != null)
            {
                iconImage.enabled = true;
                iconImage.sprite = currentSlot.itemData.icon; // ⭐ Dùng itemData
            }
            // hien thi so luong
            if (quantityText != null)
            {
                quantityText.text = currentSlot.quantity > 1 ? currentSlot.quantity.ToString() : "";
            }
        }
    }

    public void ClearSlot()
    {
        currentSlot = null;

        if (iconImage != null)
            iconImage.enabled = false;

        if (quantityText != null)
            quantityText.text = "";
    }
}
