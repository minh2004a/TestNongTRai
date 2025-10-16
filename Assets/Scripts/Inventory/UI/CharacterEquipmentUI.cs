using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TinyFarm.Items.UI
{
    public class CharacterEquipmentUI : MonoBehaviour
    {
        [Header("Equipment Slots")]
        public EquipmentSlotUI helmetSlot;
        public EquipmentSlotUI armorSlot;
        public EquipmentSlotUI glovesSlot;
        public EquipmentSlotUI pantsSlot;
        public EquipmentSlotUI bootsSlot;

        [Header("Character Display")]
        public Image characterImage;

        private void Start()
        {
            InitializeSlots();
        }

        private void InitializeSlots()
        {
            helmetSlot.Setup(EquipmentSlotType.Helmet);
            armorSlot.Setup(EquipmentSlotType.Armor);
            glovesSlot.Setup(EquipmentSlotType.Gloves);
            pantsSlot.Setup(EquipmentSlotType.Pants);
            bootsSlot.Setup(EquipmentSlotType.Boots);
        }

        public void EquipItem(ItemData item, EquipmentSlotType type)
        {
            switch (type)
            {
                case EquipmentSlotType.Helmet:
                    helmetSlot.SetItem(item);
                    break;
                case EquipmentSlotType.Armor:
                    armorSlot.SetItem(item);
                    break;
                case EquipmentSlotType.Gloves:
                    glovesSlot.SetItem(item);
                    break;
                case EquipmentSlotType.Pants:
                    pantsSlot.SetItem(item);
                    break;
                case EquipmentSlotType.Boots:
                    bootsSlot.SetItem(item);
                    break;
            }

            UpdateCharacterAppearance();
        }

        private void UpdateCharacterAppearance()
        {
            // Logic cập nhật sprite character khi equip
            // VD: thay đổi màu, thêm layer equipment lên character
        }
    }
}

