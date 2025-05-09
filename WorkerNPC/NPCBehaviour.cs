using UnityEngine;

namespace WorkerNPC
{
    public class NPCBehaviour : MonoBehaviour
    {
        public ZNetView zNetView;
        public string inventoryKey = "worker_npc_inventory";
        public int maxInventorySize = 50;
        public string inventoryItem = "Resin";

        private void Awake()
        {
            zNetView = GetComponent<ZNetView>();
            if (zNetView == null)
            {
                Jotunn.Logger.LogError("Failed to get ZNetView component.");
                return;
            }
        }

        private void AddItemToInventory(string itemName, int amount)
        {
            if (zNetView == null || !zNetView.IsValid())
            {
                Jotunn.Logger.LogError("ZNetView missing — cannot modify Worker NPC inventory.");
                return;
            }

            string inventoryData = zNetView.GetZDO().GetString(inventoryKey, $"{itemName}:0");
            int currentAmount = int.Parse(inventoryData.Split(':')[1]);

            int newAmount = currentAmount + amount;
            zNetView.GetZDO().Set("worker_npc_inventory", $"{itemName}:{newAmount}");

            Jotunn.Logger.LogInfo($"Added {amount} {itemName} to Worker NPC's inventory (Total: {newAmount}).");
        }

        private bool UseItemFromInventory(string itemName, int amount)
        {
            if (zNetView == null || !zNetView.IsValid())
            {
                Jotunn.Logger.LogError("ZNetView missing — cannot modify Worker NPC inventory.");
                return false;
            }

            string inventoryData = zNetView.GetZDO().GetString(inventoryKey, $"{itemName}:0");
            int currentAmount = int.Parse(inventoryData.Split(':')[1]);

            if (currentAmount < amount)
            {
                Jotunn.Logger.LogWarning($"Worker NPC does not have enough {itemName} (Has: {currentAmount}, Needs: {amount}).");
                return false;
            }

            int newAmount = currentAmount - amount;
            zNetView.GetZDO().Set("worker_npc_inventory", $"{itemName}:{newAmount}");

            Jotunn.Logger.LogInfo($"Used {amount} {itemName} from Worker NPC's inventory (Remaining: {newAmount}).");
            return true;
        }
    }
}
