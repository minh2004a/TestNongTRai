using System.Collections;
using System.Collections.Generic;
using TinyFarm.Items;
using UnityEngine;


public class PlayerHarvestScript : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float harvestRange = 2f;
    [SerializeField] private KeyCode harvestKey = KeyCode.E;

    private ToolItem equippedTool;

    void Update()
    {
        if (Input.GetKeyDown(harvestKey))
        {
            TryHarvest();
        }
    }

    void TryHarvest()
    {
        // Find nearest harvestable
        var harvestable = ResourceManager.Instance.GetNearestHarvestable(
            transform.position,
            harvestRange
        );

        if (harvestable != null)
        {
            // Get equipped tool (từ ToolManager hoặc Inventory)
            equippedTool = ToolManager.Instance?.EquippedTool;

            if (harvestable.TryHarvest(equippedTool))
            {
                Debug.Log($"Harvesting {harvestable.name}...");
            }
        }
        else
        {
            Debug.Log("No harvestable nearby");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, harvestRange);
    }
}
