using System.Collections.Generic;
using UnityEngine;

namespace WorkerNPC
{
    internal class NPCBehaviour : MonoBehaviour
    {
        ZNetView zNetView;

        // Inventory
        string inventoryKey = "worker_npc_inventory";
        int maxInventorySize = 50;
        string inventoryItem = "$item_resin";

        // Searching for resources
        float searchRadius = 10f;
        float searchTimer = 0f;
        float searchInterval = 10f;

        private void Start()
        {
            zNetView = transform.parent.GetComponent<ZNetView>();
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
            zNetView.GetZDO().Set(inventoryKey, $"{itemName}:{newAmount}");

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
            zNetView.GetZDO().Set(inventoryKey, $"{itemName}:{newAmount}");

            Jotunn.Logger.LogInfo($"Used {amount} {itemName} from Worker NPC's inventory (Remaining: {newAmount}).");
            return true;
        }

        private List<GameObject> FindNearbyWorkerChests(float searchRadius, string requiredItem)
        {
            List<GameObject> workerChests = new List<GameObject>();

            if (transform.parent == null)
            {
                Jotunn.Logger.LogWarning("NPC has no parent object — cannot find nearby Worker Chests.");
                return workerChests;
            }

            Vector3 bedPosition = transform.parent.position;

            WorkerChestBehaviour[] allChests = FindObjectsOfType<WorkerChestBehaviour>();

            foreach (WorkerChestBehaviour chest in allChests)
            {
                float distance = Vector3.Distance(chest.transform.position, bedPosition);
                if (distance > searchRadius) continue;

                if (chest.CountItems(requiredItem) > 0)
                {
                    workerChests.Add(chest.gameObject);
                }
            }

            return workerChests;
        }

        private List<GameObject> FindNearbyResource<T>(float searchRadius) where T : MonoBehaviour
        {
            List<GameObject> resources = new List<GameObject>();

            if (transform.parent == null)
            {
                Jotunn.Logger.LogWarning("NPC has no parent object — cannot find nearby resources.");
                return resources;
            }

            Vector3 bedPosition = transform.parent.position;

            T[] allResources = FindObjectsOfType<T>();

            foreach (T resource in allResources)
            {
                float distance = Vector3.Distance(resource.transform.position, bedPosition);
                if (distance <= searchRadius)
                {
                    resources.Add(resource.gameObject);
                }
            }

            return resources;
        }

        private void Update()
        {
            searchTimer += Time.deltaTime;

            if (searchTimer >= searchInterval)
            {
                searchTimer = 0f;

                List<GameObject> nearbyChests = FindNearbyWorkerChests(searchRadius, inventoryItem);
                Jotunn.Logger.LogInfo($"NPC scanned for chests and found {nearbyChests.Count} in range.");

                List<GameObject> nearbyTorches = FindNearbyResource<Fireplace>(searchRadius);
                Jotunn.Logger.LogInfo($"NPC scanned for torches and found {nearbyTorches.Count} in range.");
            }
        }

    }
}
