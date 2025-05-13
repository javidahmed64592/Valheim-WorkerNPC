using Jotunn.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace WorkerNPC
{
    internal class WorkerNPCBase
    {
        static string displayName = WorkerNPCConfig.displayName;
        static string prefabName = WorkerNPCConfig.prefabName;
        static string customName = WorkerNPCConfig.customName;

        public void RegisterWorkerNPC()
        {
            Jotunn.Logger.LogInfo($"Attempting to register {displayName}...");

            void CreateNPC()
            {
                GameObject npcObject = GetBaseNPC();
                PrefabManager.Instance.AddPrefab(npcObject);

                Jotunn.Logger.LogInfo($"Registered {displayName}.");
                PrefabManager.OnVanillaPrefabsAvailable -= CreateNPC;
            }

            PrefabManager.OnVanillaPrefabsAvailable += CreateNPC;
        }

        internal virtual GameObject GetBaseNPC()
        {
            GameObject clonedNPC = PrefabManager.Instance.CreateClonedPrefab(customName, prefabName);

            if (clonedNPC == null)
            {
                Jotunn.Logger.LogError("Dvergr NPC prefab not found!");
                return null;
            }

            return clonedNPC;
        }
    }

    internal class NPCBehaviour : MonoBehaviour
    {
        internal ZNetView zNetView;
        float jobTimer = 0f;
        float jobInterval = 10f;

        // Inventory
        string inventoryKey = WorkerNPCConfig.inventoryKey;
        int maxInventorySize = 50;

        internal void Start()
        {
            zNetView = transform.parent.GetComponent<ZNetView>();
            if (zNetView == null)
            {
                Jotunn.Logger.LogError("Failed to get ZNetView component.");
                return;
            }
        }

        internal int CheckInventory(string itemName)
        {
            if (zNetView == null || !zNetView.IsValid())
            {
                Jotunn.Logger.LogError("ZNetView missing — cannot check Worker NPC inventory.");
                return 0;
            }

            string inventoryData = zNetView.GetZDO().GetString(inventoryKey, $"{itemName}:0");
            int currentAmount = int.Parse(inventoryData.Split(':')[1]);

            return currentAmount;
        }

        internal int AddItemToInventory(string itemName, int amountRequested)
        {
            if (zNetView == null || !zNetView.IsValid())
            {
                Jotunn.Logger.LogError("ZNetView missing — cannot modify Worker NPC inventory.");
                return 0;
            }

            int currentAmount = CheckInventory(itemName);
            int amountCanAdd = maxInventorySize - currentAmount;
            int amountToAdd = Mathf.Min(amountRequested, amountCanAdd);

            int newAmount = currentAmount + amountToAdd;
            zNetView.GetZDO().Set(inventoryKey, $"{itemName}:{newAmount}");
            return amountToAdd;
        }

        internal int UseItemFromInventory(string itemName, int amountRequested)
        {
            if (zNetView == null || !zNetView.IsValid())
            {
                Jotunn.Logger.LogError("ZNetView missing — cannot modify Worker NPC inventory.");
                return 0;
            }

            int currentAmount = CheckInventory(itemName);
            int amountToUse = Mathf.Min(amountRequested, currentAmount);

            int newAmount = currentAmount - amountToUse;
            zNetView.GetZDO().Set(inventoryKey, $"{itemName}:{newAmount}");

            return amountToUse;
        }

        internal List<WorkerSupplyChestBehaviour> FindNearbySupplyChests(float searchRadius, string requiredItem)
        {
            List<WorkerSupplyChestBehaviour> workerChests = new List<WorkerSupplyChestBehaviour>();

            if (transform.parent == null)
            {
                Jotunn.Logger.LogWarning("NPC has no parent object — cannot find nearby Worker Chests.");
                return workerChests;
            }

            Vector3 bedPosition = transform.parent.position;

            WorkerSupplyChestBehaviour[] allChests = FindObjectsOfType<WorkerSupplyChestBehaviour>();

            foreach (WorkerSupplyChestBehaviour chest in allChests)
            {
                float distance = Vector3.Distance(chest.transform.position, bedPosition);
                if (distance > searchRadius) continue;

                if (chest.CountItems(requiredItem) > 0)
                {
                    workerChests.Add(chest);
                }
            }

            return workerChests;
        }

        internal List<T> FindNearbyResource<T>(float searchRadius) where T : MonoBehaviour
        {
            List<T> resources = new List<T>();

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
                    resources.Add(resource);
                }
            }

            return resources;
        }

        internal virtual void DoJob()
        {
            Jotunn.Logger.LogInfo("NPC DOING JOB...");
        }
        
        private void Update()
        {
            jobTimer += Time.deltaTime;

            if (jobTimer >= jobInterval)
            {
                Jotunn.Logger.LogInfo("NPC STARTING JOB...");
                jobTimer = 0f;

                DoJob();

                Jotunn.Logger.LogInfo("NPC FINISHING JOB...");
            }
        }
    }
}
