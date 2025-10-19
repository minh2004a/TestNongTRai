using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    public class ResourceManager : MonoBehaviour
    {
        private static ResourceManager instance;
        public static ResourceManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ResourceManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("ResourceManager");
                        instance = go.AddComponent<ResourceManager>();
                    }
                }
                return instance;
            }
        }

        [Header("Settings")]
        [Tooltip("Track tất cả harvestable objects")]
        [SerializeField] private bool trackAllObjects = true;

        // Tracking
        private List<HarvestableObject> allHarvestables = new List<HarvestableObject>();
        private Dictionary<ResourcesType, int> totalResourcesHarvested = new Dictionary<ResourcesType, int>();

        // Events
        //public event Action<HarvestableObject, ResourceItem> OnResourceHarvested;

        // Properties
        public int TotalObjects => allHarvestables.Count;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (trackAllObjects)
            {
                RegisterAllHarvestables();
            }
        }

        // Register harvestable object
        public void RegisterHarvestable(HarvestableObject harvestable)
        {
            if (harvestable == null || allHarvestables.Contains(harvestable))
                return;

            allHarvestables.Add(harvestable);

            // Subscribe events
            harvestable.OnDestroyed += OnHarvestableDestroyed;
            harvestable.OnRespawned += OnHarvestableRespawned;
        }

        // Unregister harvestable
        public void UnregisterHarvestable(HarvestableObject harvestable)
        {
            if (harvestable == null) return;

            allHarvestables.Remove(harvestable);

            harvestable.OnDestroyed -= OnHarvestableDestroyed;
            harvestable.OnRespawned -= OnHarvestableRespawned;
        }

        // Register tất cả harvestables trong scene
        private void RegisterAllHarvestables()
        {
            var harvestables = FindObjectsOfType<HarvestableObject>();
            foreach (var h in harvestables)
            {
                RegisterHarvestable(h);
            }

            Debug.Log($"Registered {harvestables.Length} harvestable objects");
        }

        // Record resource harvested
        public void RecordHarvest(ResourcesType resourceType, int amount)
        {
            if (!totalResourcesHarvested.ContainsKey(resourceType))
            {
                totalResourcesHarvested[resourceType] = 0;
            }

            totalResourcesHarvested[resourceType] += amount;
        }

        // Get total harvested của resource type
        public int GetTotalHarvested(ResourcesType resourceType)
        {
            return totalResourcesHarvested.TryGetValue(resourceType, out int amount) ? amount : 0;
        }

        // Get statistics
        public string GetStatistics()
        {
            string stats = "=== Resource Statistics ===\n";
            stats += $"Total Harvestable Objects: {allHarvestables.Count}\n\n";
            stats += "Total Harvested:\n";

            foreach (var kvp in totalResourcesHarvested)
            {
                stats += $"  {kvp.Key}: {kvp.Value}\n";
            }

            return stats;
        }

        private void OnHarvestableDestroyed(HarvestableObject harvestable)
        {
            if (harvestable.ResourceData != null)
            {
                RecordHarvest(harvestable.ResourceData.resourceType, 1);
            }
        }

        private void OnHarvestableRespawned(HarvestableObject harvestable)
        {
            Debug.Log($"{harvestable.name} respawned");
        }

        // Get all harvestables of type
        public List<T> GetHarvestablesOfType<T>() where T : HarvestableObject
        {
            var result = new List<T>();
            foreach (var h in allHarvestables)
            {
                if (h is T typed)
                {
                    result.Add(typed);
                }
            }
            return result;
        }

        // Get nearest harvestable
        public HarvestableObject GetNearestHarvestable(Vector3 position, float maxDistance = 10f)
        {
            HarvestableObject nearest = null;
            float nearestDist = maxDistance;

            foreach (var h in allHarvestables)
            {
                if (h.IsDestroyed) continue;

                float dist = Vector3.Distance(position, h.transform.position);
                if (dist < nearestDist)
                {
                    nearest = h;
                    nearestDist = dist;
                }
            }

            return nearest;
        }

        [ContextMenu("Print Statistics")]
        public void PrintStatistics()
        {
            Debug.Log(GetStatistics());
        }
    }
}

