using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TinyFarm.Items
{
    /// <summary>
    /// ITEM SPAWNER - Spawn items vào thế giới
    /// </summary>
    // Spawner để tạo items trong world
    // Có thể dùng cho loot drops, resource nodes, etc.
    public class ItemSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [Tooltip("Item IDs có thể spawn")]
        [SerializeField] private List<SpawnEntry> spawnTable = new List<SpawnEntry>();

        [Tooltip("Spawn tự động khi Start?")]
        [SerializeField] private bool spawnOnStart = false;

        [Tooltip("Spawn interval (giây, 0 = không repeat)")]
        [SerializeField] private float spawnInterval = 0f;

        [Tooltip("Max items có thể spawn cùng lúc")]
        [SerializeField] private int maxSpawnedItems = 5;

        [Header("Spawn Area")]
        [Tooltip("Spawn mode")]
        [SerializeField] private SpawnMode spawnMode = SpawnMode.Point;

        [Tooltip("Radius cho Circle spawn")]
        [SerializeField] private float spawnRadius = 2f;

        [Tooltip("Size cho Box spawn")]
        [SerializeField] private Vector2 spawnBoxSize = new Vector2(4f, 4f);

        [Header("Item Settings")]
        [Tooltip("Pickup prefab")]
        [SerializeField] private GameObject pickupPrefab;

        // State
        private List<ItemPickup> spawnedItems = new List<ItemPickup>();
        private float lastSpawnTime;

        // Events
        public event Action<ItemPickup> OnItemSpawned;

        private void Start()
        {
            if (spawnOnStart)
            {
                SpawnItem();
            }

            if (spawnInterval > 0)
            {
                InvokeRepeating(nameof(TrySpawn), spawnInterval, spawnInterval);
            }
        }

        // Spawn một item random từ spawn table
        public ItemPickup SpawnItem()
        {
            // Check max spawned
            CleanupDestroyedItems();
            if (spawnedItems.Count >= maxSpawnedItems)
            {
                Debug.Log("Max spawned items reached");
                return null;
            }

            // Roll item từ spawn table
            SpawnEntry entry = RollSpawnTable();
            if (entry == null || string.IsNullOrEmpty(entry.itemID))
            {
                Debug.LogWarning("No item to spawn");
                return null;
            }

            // Tạo item
            Item item = ItemManager.Instance.CreateItem(entry.itemID, entry.quantity);
            if (item == null)
            {
                Debug.LogError($"Failed to create item: {entry.itemID}");
                return null;
            }

            // Spawn position
            Vector3 spawnPos = GetSpawnPosition();

            // Tạo pickup
            ItemPickup pickup = CreatePickup(item, spawnPos);

            if (pickup != null)
            {
                spawnedItems.Add(pickup);
                lastSpawnTime = Time.time;

                // Subscribe to destroy event
                pickup.OnItemDestroyed += (p) => spawnedItems.Remove(p);
                pickup.OnItemPickedUp += (p, picker) => spawnedItems.Remove(p);

                OnItemSpawned?.Invoke(pickup);
            }

            return pickup;
        }

        // Spawn item cụ thể
        public ItemPickup SpawnSpecificItem(string itemID, int quantity = 1)
        {
            Item item = ItemManager.Instance.CreateItem(itemID, quantity);
            if (item == null) return null;

            Vector3 spawnPos = GetSpawnPosition();
            ItemPickup pickup = CreatePickup(item, spawnPos);

            if (pickup != null)
            {
                spawnedItems.Add(pickup);
                OnItemSpawned?.Invoke(pickup);
            }

            return pickup;
        }

        // Spawn nhiều items
        public List<ItemPickup> SpawnMultiple(int count)
        {
            var pickups = new List<ItemPickup>();

            for (int i = 0; i < count; i++)
            {
                ItemPickup pickup = SpawnItem();
                if (pickup != null)
                {
                    pickups.Add(pickup);
                }
            }

            return pickups;
        }

        // Try spawn (check conditions)
        private void TrySpawn()
        {
            if (Time.time < lastSpawnTime + spawnInterval) return;
            if (spawnedItems.Count >= maxSpawnedItems) return;

            SpawnItem();
        }

        // Roll spawn table để chọn item
        private SpawnEntry RollSpawnTable()
        {
            if (spawnTable.Count == 0) return null;

            // Calculate total weight
            float totalWeight = 0f;
            foreach (var entry in spawnTable)
            {
                if (entry.enabled)
                    totalWeight += entry.spawnChance;
            }

            // Roll
            float roll = UnityEngine.Random.Range(0f, totalWeight);
            float current = 0f;

            foreach (var entry in spawnTable)
            {
                if (!entry.enabled) continue;

                current += entry.spawnChance;
                if (roll <= current)
                {
                    return entry;
                }
            }

            return spawnTable[0]; // Fallback
        }

        // Lấy spawn position dựa vào mode
        private Vector3 GetSpawnPosition()
        {
            switch (spawnMode)
            {
                case SpawnMode.Point:
                    return transform.position;

                case SpawnMode.Circle:
                    Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * spawnRadius;
                    return transform.position + new Vector3(randomCircle.x, randomCircle.y, 0);

                case SpawnMode.Box:
                    float x = UnityEngine.Random.Range(-spawnBoxSize.x / 2, spawnBoxSize.x / 2);
                    float y = UnityEngine.Random.Range(-spawnBoxSize.y / 2, spawnBoxSize.y / 2);
                    return transform.position + new Vector3(x, y, 0);

                default:
                    return transform.position;
            }
        }

        // Tạo pickup GameObject
        private ItemPickup CreatePickup(Item item, Vector3 position)
        {
            GameObject pickupGO;

            if (pickupPrefab != null)
            {
                pickupGO = Instantiate(pickupPrefab, position, Quaternion.identity);
            }
            else
            {
                // Use ItemDrop utility
                ItemDrop.SetPickupPrefab(pickupPrefab);
                return ItemDrop.DropItem(item, position);
            }

            var pickup = pickupGO.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                pickup.SetItem(item);
            }

            return pickup;
        }

        // Remove destroyed items từ list
        private void CleanupDestroyedItems()
        {
            spawnedItems.RemoveAll(item => item == null);
        }

        // Clear tất cả spawned items
        public void ClearAllSpawnedItems()
        {
            foreach (var item in spawnedItems.ToArray())
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }

            spawnedItems.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;

            switch (spawnMode)
            {
                case SpawnMode.Point:
                    Gizmos.DrawWireSphere(transform.position, 0.3f);
                    break;

                case SpawnMode.Circle:
                    DrawCircle(transform.position, spawnRadius, 32);
                    break;

                case SpawnMode.Box:
                    Gizmos.DrawWireCube(transform.position, new Vector3(spawnBoxSize.x, spawnBoxSize.y, 0));
                    break;
            }
        }

        private void DrawCircle(Vector3 center, float radius, int segments)
        {
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }
    }

    [Serializable]
    public class SpawnEntry
    {
        public bool enabled = true;
        public string itemID;

        [Range(1, 999)]
        public int quantity = 1;

        [Range(0, 100)]
        [Tooltip("Chance to spawn (weight)")]
        public float spawnChance = 10f;
    }

    public enum SpawnMode
    {
        Point,      // Spawn tại vị trí spawner
        Circle,     // Random trong circle
        Box         // Random trong box
    }
}

