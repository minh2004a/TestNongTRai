using System;
using System.Collections;
using System.Collections.Generic;
using TinyFarm.Items;
using UnityEngine;

public class ToolManager : MonoBehaviour
{
    private static ToolManager instance;
    public static ToolManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ToolManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ToolManager");
                    instance = go.AddComponent<ToolManager>();
                }
            }
            return instance;
        }
    }

    [Header("References")]
    [SerializeField] private InventoryManager inventoryManager;

    private ToolItem equippedTool;

    // Events
    public event Action<ToolItem> OnToolEquipped;
    public event Action<ToolItem> OnToolUnequipped;

    // Properties
    public ToolItem EquippedTool => equippedTool;
    public bool HasEquippedTool => equippedTool != null;
    public InventoryManager Inventory => inventoryManager;

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
        if (inventoryManager == null)
        {
            inventoryManager = FindObjectOfType<InventoryManager>();
            if (inventoryManager == null)
            {
                Debug.LogError("ToolManager: No InventoryManager found!");
            }
        }
    }


    /// Equip tool
    public bool EquipTool(ToolItem tool)
    {
        if (tool == null) return false;

        // Unequip current tool
        if (equippedTool != null)
        {
            UnequipTool();
        }

        // Equip
        equippedTool = tool;
        OnToolEquipped?.Invoke(tool);

        Debug.Log($"Equipped tool: {tool.Name}");
        return true;
    }

    /// Unequip current tool
    public void UnequipTool()
    {
        if (equippedTool == null) return;

        ToolItem tool = equippedTool;
        equippedTool = null;

        OnToolUnequipped?.Invoke(tool);
        Debug.Log($"Unequipped tool: {tool.Name}");
    }


    /// Get all registered tools
    public List<ToolItem> GetAllTools()
    {
        var tools = new List<ToolItem>();

        return tools;
    }
}
