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

    //[Header("Settings")]
    //[Tooltip("Auto-repair tools khi durability = 0?")]
    //[SerializeField] private bool autoRepair = false;

    //[Tooltip("Warn khi tool durability thấp")]
    //[SerializeField] private bool warnLowDurability = true;

    //[SerializeField] private float lowDurabilityWarningThreshold = 25f;

    // Tool tracking
    //private Dictionary<string, ToolDurability> toolDurabilities = new Dictionary<string, ToolDurability>();
    private ToolItem equippedTool;

    // Events
    public event Action<ToolItem> OnToolEquipped;
    public event Action<ToolItem> OnToolUnequipped;
    //public event Action<ToolItem> OnToolBroken;
    //public event Action<ToolItem> OnToolRepaired;

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

    // Register tool vào manager
    //public void RegisterTool(ToolItem tool)
    //{
    //    if (tool == null) return;

    //    string id = tool.InstanceID;

    //    // Create durability component
    //    if (!toolDurabilities.ContainsKey(id))
    //    {
    //        ToolDurability durability = new ToolDurability(tool);
    //        durability.OnToolBroken += () => HandleToolBroken(tool);
    //        durability.OnToolRepaired += () => HandleToolRepaired(tool);
    //        durability.OnLowDurability += (percent) => HandleLowDurability(tool, percent);

    //        toolDurabilities.Add(id, durability);
    //    }

        
    //}

    // Unregister tool
    //public void UnregisterTool(ToolItem tool)
    //{
    //    if (tool == null) return;

    //    string id = tool.InstanceID;
    //    toolDurabilities.Remove(id);
    //}

    // Get durability component cho tool
    //public ToolDurability GetToolDurability(ToolItem tool)
    //{
    //    if (tool == null) return null;

    //    if (!toolDurabilities.ContainsKey(tool.InstanceID))
    //    {
    //        RegisterTool(tool);
    //    }

    //    return toolDurabilities[tool.InstanceID];
    //}

    /// Equip tool
    public bool EquipTool(ToolItem tool)
    {
        if (tool == null) return false;

        // Unequip current tool
        if (equippedTool != null)
        {
            UnequipTool();
        }

        //// Register tool if not registered
        //RegisterTool(tool);

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

    /// Use equipped tool
    //public bool UseEquippedTool()
    //{
    //    if (!HasEquippedTool) return false;

    //    ToolDurability durability = GetToolDurability(equippedTool);
    //    if (durability == null) return false;

    //    return durability.UseTool();
    //}

    /// Repair tool
    //public bool RepairTool(ToolItem tool, float amount)
    //{
    //    ToolDurability durability = GetToolDurability(tool);
    //    if (durability == null) return false;

    //    return durability.Repair(inventoryManager, amount);
    //}

    /// Repair equipped tool
    //public bool RepairEquippedTool(float amount)
    //{
    //    if (!HasEquippedTool) return false;
    //    return RepairTool(equippedTool, amount);
    //}

    /// Repair tool hoàn toàn
    //public bool RepairToolFull(ToolItem tool)
    //{
    //    ToolDurability durability = GetToolDurability(tool);
    //    if (durability == null) return false;

    //    return durability.RepairFull(inventoryManager);
    //}

    //private void HandleToolBroken(ToolItem tool)
    //{
    //    OnToolBroken?.Invoke(tool);
    //    Debug.LogWarning($"Tool broken: {tool.Name}");

    //    if (autoRepair && inventoryManager != null)
    //    {
    //        RepairToolFull(tool);
    //    }
    //    else if (tool == equippedTool)
    //    {
    //        // Auto unequip broken tool
    //        UnequipTool();
    //    }
    //}

    //private void HandleToolRepaired(ToolItem tool)
    //{
    //    OnToolRepaired?.Invoke(tool);
    //    Debug.Log($"Tool repaired: {tool.Name}");
    //}

    //private void HandleLowDurability(ToolItem tool, float percent)
    //{
    //    if (warnLowDurability && percent <= lowDurabilityWarningThreshold)
    //    {
    //        Debug.LogWarning($"Low durability: {tool.Name} ({percent:F0}%)");
    //    }
    //}

    //private void HandleToolUpgraded(ToolItem tool, int level)
    //{
    //    OnToolUpgraded?.Invoke(tool, level);
    //    Debug.Log($"Tool upgraded: {tool.Name} -> Level {level}");
    //}

    /// Get all registered tools
    public List<ToolItem> GetAllTools()
    {
        var tools = new List<ToolItem>();

        //foreach (var durability in toolDurabilities.Values)
        //{
        //    if (durability.Tool != null)
        //    {
        //        tools.Add(durability.Tool);
        //    }
        //}

        return tools;
    }

    /// Get broken tools
    //public List<ToolItem> GetBrokenTools()
    //{
    //    var tools = new List<ToolItem>();

    //    foreach (var durability in toolDurabilities.Values)
    //    {
    //        if (durability.IsBroken)
    //        {
    //            tools.Add(durability.Tool);
    //        }
    //    }

    //    return tools;
    //}

    /// Repair all broken tools
    //public int RepairAllBrokenTools()
    //{
    //    var brokenTools = GetBrokenTools();
    //    int repairedCount = 0;

    //    foreach (var tool in brokenTools)
    //    {
    //        if (RepairToolFull(tool))
    //        {
    //            repairedCount++;
    //        }
    //    }

    //    return repairedCount;
    //}
}
