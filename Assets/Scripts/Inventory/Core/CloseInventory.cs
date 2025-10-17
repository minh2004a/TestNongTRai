using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items.UI
{
    public class CloseInventory : MonoBehaviour
    {
        public GameObject closeInventory;

        private void Update()
        {
            this.CLoseInventory();
        }
        private void CLoseInventory()
        {
            closeInventory.SetActive(false);
        }
    }
}

