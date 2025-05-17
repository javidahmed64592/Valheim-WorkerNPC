using Jotunn.Managers;
using System.Collections.Generic;
using System.Linq;
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
        internal ZDO zdo;
        float jobTimer = 0f;
        float jobInterval = 10f;

        // Inventory
        int maxInventorySize = 50;

        internal void Start()
        {
            zNetView = transform.parent.GetComponent<ZNetView>();
            if (zNetView == null)
            {
                Jotunn.Logger.LogError("Failed to get ZNetView component.");
                return;
            }

            zdo = zNetView.GetZDO();
            if (zdo == null)
            {
                Jotunn.Logger.LogError("Failed to get ZDO.");
            }
        }

        internal int CheckInventory(string itemName)
        {
            if (zNetView == null || !zNetView.IsValid())
            {
                Jotunn.Logger.LogError("ZNetView missing — cannot check Worker NPC inventory.");
                return 0;
            }

            return zdo.GetInt(itemName, 0);
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
            zdo.Set(itemName, newAmount);
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
            zdo.Set(itemName, newAmount);
            return amountToUse;
        }

        internal T[] FindNearbyResource<T>(float searchRadius) where T : MonoBehaviour
        {
            List<T> resources = new List<T>();

            if (transform.parent == null)
            {
                Jotunn.Logger.LogWarning("NPC has no parent object — cannot find nearby resources.");
                return resources.ToArray();
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

            return resources.ToArray();
        }

        internal WorkerSupplyChestBehaviour[] FindNearbySupplyChests(float searchRadius)
        {
            if (transform.parent == null)
            {
                Jotunn.Logger.LogWarning("NPC has no parent object — cannot find nearby Worker Chests.");
                return null;
            }

            return FindNearbyResource<WorkerSupplyChestBehaviour>(searchRadius);
        }

        internal WorkerDepositChestBehaviour[] FindNearbyDepositChests(float searchRadius)
        {
            if (transform.parent == null)
            {
                Jotunn.Logger.LogWarning("NPC has no parent object — cannot find nearby Worker Chests.");
                return null;
            }

            return FindNearbyResource<WorkerDepositChestBehaviour>(searchRadius);
        }

        internal void MoveTo(Vector3 targetPosition)
        {
            // TODO: Move to target position
            Jotunn.Logger.LogInfo($"Moving NPC to {targetPosition}...");
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
