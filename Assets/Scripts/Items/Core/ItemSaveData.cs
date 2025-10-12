using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemSaveData
{
    public string instanceID;
    public string itemDataID;
    public int currentStack;
    public float currentDurability;
    public Dictionary<string, object> customData = new Dictionary<string, object>();
}
