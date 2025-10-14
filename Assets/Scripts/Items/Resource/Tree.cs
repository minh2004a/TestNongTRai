using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    public class Tree : HarvestableObject
    {
        protected override void Awake()
        {
            base.Awake();

            // Rock yêu cầu Pickaxe
            if (requiredTool == ToolType.NoType)
            {
                requiredTool = ToolType.Hoe;
            }
        }
    }
}

