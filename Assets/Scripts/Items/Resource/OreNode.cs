using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    public class OreNode : HarvestableObject
    {
        protected override void Awake()
        {
            base.Awake();

            // Ore yêu cầu Hoe
            if (requiredTool == ToolType.NoType)
            {
                requiredTool = ToolType.Hoe;
            }
        }
    }
}

