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
            iconImage = transform.Find("Icon").GetComponent<Image>();

        if (quantityText == null)
            quantityText = transform.Find("Quantity").GetComponent<TextMeshProUGUI>();
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
            iconImage.enabled = false;
            quantityText.text = "";
        }
        else
        {
            iconImage.enabled = true;
            iconImage.sprite = currentSlot.itemData.icon;

            if (currentSlot.quantity > 1)
            {
                quantityText.text = currentSlot.quantity.ToString();
            }
            else
            {
                quantityText.text = "";
            }
        }
    }

    public void ClearSlot()
    {
        currentSlot = null;
        iconImage.enabled = false;
        quantityText.text = "";
    }
}
