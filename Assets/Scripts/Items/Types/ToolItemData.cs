using UnityEngine;

namespace TinyFarm.Items
{
    [CreateAssetMenu(fileName = "New Tool", menuName = "Game/Item/Tool Item Data")]
    public class ToolItemData : ItemData
    {
        public string toolID;

        [Header("Tool Specific")]
        [Tooltip("Loại công cụ")]
        public ToolType toolType;

        [Tooltip("Độ bền hiện tại (runtime)")]
        [SerializeField] private int currentDurability = 100;

        [Tooltip("Độ bền của công cụ (-1 = không giới hạn)")]
        public int durability = -1;

        [Tooltip("Sức mạnh của công cụ (dùng cho damage, efficiency, etc.)")]
        [Range(1, 10)]
        public int efficiency = 1;

        [Header("Animation & Sound")]
        [Tooltip("Animation clip khi sử dụng công cụ")]
        public  AnimationClip useAnimation;

        [Tooltip("Sound effect khi sử dụng")]
        public AudioClip useSound;


        public override ItemType GetItemType()
        {
            return ItemType.Tool;
        }

        protected override void ValidateItemData()
        {
            base.ValidateItemData();
            // Tool không thể stack
            isStackable = false;
            maxStackSize = 1;

            // Tool không thể ăn
            isUsable = false;

            // Đảm bảo currentDurability không vượt quá max
            if (currentDurability > maxDurability)
            {
                currentDurability = maxDurability;
            }
        }

        // Lấy độ bền hiện tại
        public int GetCurrentDurability()
        {
            return currentDurability;
        }

        // Set độ bền
        public void SetDurability(int value)
        {
            currentDurability = Mathf.Clamp(value, 0, maxDurability);
        }

        // Giảm độ bền khi dùng
        public void ReduceDurability(int amount = 1)
        {
            currentDurability = Mathf.Max(currentDurability, -amount);
        }

        // Sửa chữa tool
        public void Repair(int amount)
        {
            currentDurability = Mathf.Min(maxDurability,currentDurability + amount);
        }

        // Tool có bị hỏng không?
        public bool isBroken()
        {
            return currentDurability <= 0;
        }

        // Phần trăm độ bền còn lại
        public float GetDurabilityPercent()
        {
            return (float)currentDurability / maxDurability;
        }
    }
}

