using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    public abstract class HarvestableObject : MonoBehaviour
    {
        [Header("Resource Info")]
        [Tooltip("Resource sẽ drop")]
        [SerializeField] protected ResourcesItemData resourceData;

        [Header("Harvest Settings")]
        [Tooltip("HP của object (số lần hit để phá)")]
        [SerializeField] protected int maxHealth = 3;

        [Tooltip("Tool type cần để harvest")]
        [SerializeField] protected ToolType requiredTool = ToolType.NoType;

        [Tooltip("Min tool level cần")]
        [SerializeField] protected int minToolLevel = 1;

        [Header("Visual Feedback")]
        [Tooltip("Particle effect khi bị hit")]
        [SerializeField] protected GameObject hitEffect;

        [Tooltip("Particle effect khi bị phá hủy")]
        [SerializeField] protected GameObject destroyEffect;

        [Tooltip("Sprite khi còn full health")]
        [SerializeField] protected Sprite healthySprite;

        [Tooltip("Sprite khi damaged")]
        [SerializeField] protected Sprite damagedSprite;

        [Tooltip("Sprite khi almost destroyed")]
        [SerializeField] protected Sprite almostDestroyedSprite;

        [Header("Respawn")]
        [Tooltip("Có respawn sau khi bị phá không?")]
        [SerializeField] protected bool canRespawn = true;

        [Tooltip("Thời gian respawn (giây)")]
        [SerializeField] protected float respawnTime = 60f;

        // State
        protected int currentHealth;
        protected bool isDestroyed = false;
        protected SpriteRenderer spriteRenderer;

        // Events
        public event Action<HarvestableObject, int> OnHarvested;
        public event Action<HarvestableObject> OnDestroyed;
        public event Action<HarvestableObject> OnRespawned;

        // Properties
        public ResourcesItemData ResourceData => resourceData;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public float HealthPercent => (float)currentHealth / maxHealth * 100f;
        public bool IsDestroyed => isDestroyed;
        public ToolType RequiredTool => requiredTool;
        public int MinToolLevel => minToolLevel;

        protected virtual void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            currentHealth = maxHealth;
            UpdateVisual();
        }

        // Attempt to harvest với tool
        public virtual bool TryHarvest(ToolItem tool)
        {
            if (isDestroyed)
            {
                Debug.Log($"{gameObject.name} is already destroyed!");
                return false;
            }

            // Validate tool
            if (!CanHarvestWith(tool))
            {
                Debug.Log($"Cannot harvest {gameObject.name} with {tool?.Name ?? "no tool"}!");
                ShowInvalidToolFeedback();
                return false;
            }

            // Harvest
            Harvest(tool);
            return true;
        }

        // Check xem có thể harvest với tool này không
        public virtual bool CanHarvestWith(ToolItem tool)
        {
            if (requiredTool == ToolType.NoType) return true;
            if (tool == null) return false;
            if (tool.ToolType != requiredTool) return false;

            return true;
        }

        // Perform harvest
        protected virtual void Harvest(ToolItem tool)
        {
            // Giảm health
            int damage = CalculateDamage(tool);
            currentHealth -= damage;

            // Visual feedback
            PlayHitEffect();
            UpdateVisual();
            ShakeObject();

            OnHarvested?.Invoke(this, damage);

            // Check destroyed
            if (currentHealth <= 0)
            {
                DestroyObject();
            }
        }

        // Tính damage dựa vào tool
        protected virtual int CalculateDamage(ToolItem tool)
        {
            if (tool == null) return 1;

            int baseDamage = 1;

            return Mathf.Max(1, baseDamage);
        }

        // Destroy object và drop resources
        protected virtual void DestroyObject()
        {
            if (isDestroyed) return;

            isDestroyed = true;

            // Drop resources
            DropResources();

            // Visual effects
            PlayDestroyEffect();

            OnDestroyed?.Invoke(this);

            // Respawn hoặc destroy GameObject
            if (canRespawn)
            {
                StartCoroutine(RespawnCoroutine());
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Drop resources vào world
        protected virtual void DropResources()
        {
            if (resourceData == null)
            {
                Debug.LogWarning($"{gameObject.name} has no resource data!");
                return;
            }

            // Calculate drop amount
            int dropAmount = resourceData.CalculateDropAmount();

            // Create resource item
            ResourceItem resource = new ResourceItem(resourceData);
            resource.Stackable.SetStack(dropAmount);

            // Drop vào world
            Vector3 dropPosition = transform.position + Vector3.up * 0.5f;
            ItemDrop.DropItem(resource, dropPosition);

            Debug.Log($"Dropped {dropAmount}x {resourceData.itemName}");
        }

        protected virtual IEnumerator RespawnCoroutine()
        {
            // Hide object
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }

            // Disable collider
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            // Wait
            yield return new WaitForSeconds(respawnTime);

            // Respawn
            Respawn();
        }

        // Respawn object
        protected virtual void Respawn()
        {
            currentHealth = maxHealth;
            isDestroyed = false;

            // Show object
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }

            // Enable collider
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = true;
            }

            UpdateVisual();
            OnRespawned?.Invoke(this);

            Debug.Log($"{gameObject.name} respawned!");
        }

        // Update sprite dựa vào health
        protected virtual void UpdateVisual()
        {
            if (spriteRenderer == null) return;

            float healthPercent = HealthPercent;

            if (healthPercent > 66f && healthySprite != null)
            {
                spriteRenderer.sprite = healthySprite;
            }
            else if (healthPercent > 33f && damagedSprite != null)
            {
                spriteRenderer.sprite = damagedSprite;
            }
            else if (almostDestroyedSprite != null)
            {
                spriteRenderer.sprite = almostDestroyedSprite;
            }
        }

        // Play hit effect
        protected virtual void PlayHitEffect()
        {
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }

            // Play sound
            // AudioManager.Instance?.PlaySFX("hit_resource");
        }

        // Play destroy effect
        protected virtual void PlayDestroyEffect()
        {
            if (destroyEffect != null)
            {
                Instantiate(destroyEffect, transform.position, Quaternion.identity);
            }

            // Play sound
            // AudioManager.Instance?.PlaySFX("destroy_resource");
        }

        // Shake effect khi bị hit
        protected virtual void ShakeObject()
        {
            StartCoroutine(ShakeCoroutine());
        }

        protected virtual IEnumerator ShakeCoroutine()
        {
            Vector3 originalPos = transform.position;
            float elapsed = 0f;
            float duration = 0.1f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = UnityEngine.Random.Range(-0.05f, 0.05f);
                transform.position = originalPos + new Vector3(x, 0, 0);
                yield return null;
            }

            transform.position = originalPos;
        }

        // Show feedback khi dùng sai tool
        protected virtual void ShowInvalidToolFeedback()
        {
            // Show text popup: "Need [Tool] Level [X]"
            // FloatingTextManager.Instance?.Show($"Need {requiredTool} Lv.{minToolLevel}+", transform.position);

            Debug.Log($"Need {requiredTool} Level {minToolLevel} or higher!");
        }


        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }

}

