using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    public static class ItemDrop
    {
        // Default prefab cho pickup
        private static GameObject pickupPrefab;

        // Set default pickup prefab
        public static void SetPickupPrefab(GameObject prefab)
        {
            pickupPrefab = prefab;
        }

        // Drop item tại vị trí
        public static ItemPickup DropItem(Item item, Vector3 position)
        {
            return DropItem(item, position, Vector3.zero);
        }

        // Drop item với velocity (throw)
        public static ItemPickup DropItem(Item item, Vector3 position, Vector3 velocity)
        {
            if (item == null)
            {
                Debug.LogError("Cannot drop null item!");
                return null;
            }

            // Tạo pickup GameObject
            GameObject pickupGO = CreatePickupGameObject(item, position);

            // Add rigidbody nếu có velocity
            if (velocity != Vector3.zero)
            {
                Rigidbody2D rb = pickupGO.GetComponent<Rigidbody2D>();
                if (rb == null)
                {
                    rb = pickupGO.AddComponent<Rigidbody2D>();
                }
                rb.velocity = velocity;
                rb.drag = 2f; // Giảm tốc
            }

            ItemPickup pickup = pickupGO.GetComponent<ItemPickup>();
            return pickup;
        }

        // Drop ItemStack
        public static ItemPickup DropStack(ItemStack stack, Vector3 position)
        {
            if (stack == null || stack.IsEmpty)
            {
                Debug.LogError("Cannot drop empty stack!");
                return null;
            }

            return DropItem(stack.Item, position);
        }

        // Drop nhiều items (scatter)
        public static List<ItemPickup> DropItems(List<Item> items, Vector3 position, float scatterRadius = 1f)
        {
            var pickups = new List<ItemPickup>();

            foreach (var item in items)
            {
                // Random position trong radius
                Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * scatterRadius;
                Vector3 dropPosition = position + new Vector3(randomOffset.x, randomOffset.y, 0);

                // Random velocity (throw effect)
                Vector3 randomVelocity = new Vector3(
                    UnityEngine.Random.Range(-2f, 2f),
                    UnityEngine.Random.Range(2f, 4f),
                    0
                );

                ItemPickup pickup = DropItem(item, dropPosition, randomVelocity);
                if (pickup != null)
                {
                    pickups.Add(pickup);
                }
            }

            return pickups;
        }

        // Tạo pickup GameObject
        private static GameObject CreatePickupGameObject(Item item, Vector3 position)
        {
            GameObject pickupGO;

            if (pickupPrefab != null)
            {
                // Use prefab
                pickupGO = GameObject.Instantiate(pickupPrefab, position, Quaternion.identity);
            }
            else
            {
                // Create simple pickup
                pickupGO = new GameObject($"Pickup_{item.Name}");
                pickupGO.transform.position = position;

                // Add SpriteRenderer
                var spriteRenderer = pickupGO.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = item.Icon;
                spriteRenderer.sortingOrder = 10;

                // Add Collider
                var collider = pickupGO.AddComponent<CircleCollider2D>();
                collider.radius = 0.3f;
                collider.isTrigger = true;

                // Add ItemPickup component
                pickupGO.AddComponent<ItemPickup>();
            }

            // Set item
            var pickup = pickupGO.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                pickup.SetItem(item);
            }

            return pickupGO;
        }
    }

}
