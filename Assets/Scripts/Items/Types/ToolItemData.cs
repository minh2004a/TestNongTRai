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
        }
    }
}

