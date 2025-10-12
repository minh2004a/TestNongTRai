using System;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    [Serializable]
    public class StackableProperty
    {
        [SerializeField] private bool isStackable = true;
        [SerializeField] private int maxStackSize = 999;
        [SerializeField] private int currentStack = 1;

        //Properties
        public bool IsStackable => isStackable;
        public int MaxStackSize => maxStackSize;
        public int CurrentStack => currentStack;

        public bool IsFull => currentStack >= maxStackSize;
        public int RemainingSpace => maxStackSize - currentStack;

        // Events
        public event Action<int> OnStackChanged;

        //Constructor
        public StackableProperty(bool isStackabale = true, int maxStackSize = 999, int initialStack = 1)
        {
            this.isStackable = isStackabale;
            this.maxStackSize = Mathf.Max(1, maxStackSize);
            this.currentStack = Mathf.Clamp(initialStack, 1 , this.maxStackSize);
        }

        // Thêm số lượng vào stack
        public int AddToStack(int amount)
        {
            if (!isStackable || amount <= 0) return amount;

            int oldStack = currentStack;
            int space = RemainingSpace;
            int toAdd = Mathf.Min(amount, space);

            currentStack += toAdd;

            if (currentStack != oldStack)
            {
                OnStackChanged?.Invoke(currentStack);
            }

            return amount - toAdd;
        }

        // Lấy số lượng từ stack
        public int RemoveFromStack(int amount)
        {
            if (amount <= 0) return 0;

            int oldStack = currentStack;
            int toRemove = Mathf.Min(amount, currentStack);

            currentStack -= toRemove;

            if (currentStack != oldStack)
            {
                OnStackChanged?.Invoke(currentStack);
            }

            return toRemove;
        }

        // Set stack trực tiếp
        public void SetStack(int amount)
        {
            int oldStack = currentStack;
            currentStack = Mathf.Clamp(amount, 0, maxStackSize);

            if (currentStack != oldStack)
            {
                OnStackChanged?.Invoke(currentStack);
            }
        }

        // Kiểm tra có thể merge với stack khác không
        public bool CanMergeWith(StackableProperty other)
        {
            if (!isStackable || !other.isStackable) return false;
            if (IsFull) return false;
            return maxStackSize == other.maxStackSize;
        }

        // Merge với stack khác
        public int MergeWith(StackableProperty other)
        {
            if (!CanMergeWith(other)) return other.currentStack;

            int transferred = AddToStack(other.currentStack);
            other.RemoveFromStack(other.currentStack - transferred);

            return transferred;
        }

        // Split stack thành 2 phần
        public StackableProperty Split(int amount)
        {
            if (!isStackable || amount <= 0 || amount >= currentStack) return null;

            int toSplit = Mathf.Min(amount, currentStack);
            RemoveFromStack(toSplit);

            return new StackableProperty(isStackable, maxStackSize, toSplit);
        }

        // Clone
        public StackableProperty Clone()
        {
            return new StackableProperty(isStackable,maxStackSize, currentStack);
        }

    }
}

