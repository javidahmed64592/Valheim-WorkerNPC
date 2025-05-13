using Jotunn;
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

        public static void RegisterWorkerNPC()
        {
            Jotunn.Logger.LogInfo($"Attempting to register {displayName}...");

            void CreateNPC()
            {
                GameObject npcObject = GetBaseNPC();

                Jotunn.Logger.LogInfo($"Registered {displayName}.");
                PrefabManager.OnVanillaPrefabsAvailable -= CreateNPC;
            }

            PrefabManager.OnVanillaPrefabsAvailable += CreateNPC;
        }

        private static GameObject GetBaseNPC()
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
        ZNetView zNetView;
        float jobTimer = 0f;

        // Inventory
        string inventoryKey = ResinWorkerConfig.inventoryKey;
        int maxInventorySize = ResinWorkerConfig.maxInventorySize;
        string inventoryItem = ResinWorkerConfig.inventoryItem;

        // Searching for resources
        float searchRadius = ResinWorkerConfig.searchRadius;
        float searchInterval = ResinWorkerConfig.searchInterval;

        private void Start()
        {
            zNetView = transform.parent.GetComponent<ZNetView>();
            if (zNetView == null)
            {
                Jotunn.Logger.LogError("Failed to get ZNetView component.");
                return;
            }
        }

        private int CheckInventory(string itemName)
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

        private int AddItemToInventory(string itemName, int amountRequested)
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

        private int UseItemFromInventory(string itemName, int amountRequested)
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

        private List<WorkerSupplyChestBehaviour> FindNearbySupplyChests(float searchRadius, string requiredItem)
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

        private List<T> FindNearbyResource<T>(float searchRadius) where T : MonoBehaviour
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

        private void DoJob()
        {
            // If NPC has nothing in inventory, check for nearby chests
            int currentStock = CheckInventory(inventoryItem);
            Jotunn.Logger.LogInfo($"NPC inventory has {currentStock} {inventoryItem}.");
            if (currentStock == 0)
            {
                Jotunn.Logger.LogInfo("NPC is searching for nearby chests...");
                List<WorkerSupplyChestBehaviour> nearbyChests = FindNearbySupplyChests(searchRadius, inventoryItem);

                // If there are no chests with available stock, job is done
                if (nearbyChests.Count == 0)
                {
                    Jotunn.Logger.LogInfo($"NPC could not find any chests with {inventoryItem}, ending job.");
                    return;
                }

                // Otherwise go to first available chest and take as much as possible
                Jotunn.Logger.LogInfo($"NPC found {nearbyChests.Count} nearby chests. Going to first available one...");

                WorkerSupplyChestBehaviour targetChest = nearbyChests[0];

                int requestedAmount = maxInventorySize - currentStock;
                int amountTakenFromChest = targetChest.TakeItem(inventoryItem, requestedAmount);
                Jotunn.Logger.LogInfo($"NPC took {amountTakenFromChest} {inventoryItem} from chest.");
                int amountAddedToInv = AddItemToInventory(inventoryItem, amountTakenFromChest);
                Jotunn.Logger.LogInfo($"NPC added {amountAddedToInv} {inventoryItem} to inventory.");
            }

            // While NPC has items in inventory, do the job!
            List<Fireplace> nearbyTorches = FindNearbyResource<Fireplace>(searchRadius);
            Jotunn.Logger.LogInfo($"NPC scanned for torches and found {nearbyTorches.Count} in range.");

            foreach (Fireplace torch in nearbyTorches)
            {
                if (torch.m_fuelItem.TokenName() == inventoryItem)
                {
                    ZNetView torchView = torch.GetComponent<ZNetView>();
                    int currentFuel = Mathf.CeilToInt(torchView.GetZDO().GetFloat(ZDOVars.s_fuel));
                    int maxFuel = (int)torch.m_maxFuel;

                    int fuelNeeded = maxFuel - currentFuel;
                    if (fuelNeeded <= 0)
                    {
                        Jotunn.Logger.LogInfo($"Torch {torch.name} is already full.");
                        continue;
                    }

                    int amountToUse = UseItemFromInventory(inventoryItem, fuelNeeded);
                    if (amountToUse > 0)
                    {
                        torch.AddFuel(amountToUse);
                        Jotunn.Logger.LogInfo($"NPC refueled {torch.name} with {amountToUse} {inventoryItem}.");
                    }
                    else
                    {
                        Jotunn.Logger.LogInfo($"NPC has no {inventoryItem} left in inventory.");
                        break;
                    }
                }
            }
        }
        
        private void Update()
        {
            jobTimer += Time.deltaTime;

            if (jobTimer >= searchInterval)
            {
                Jotunn.Logger.LogInfo("NPC STARTING JOB...");
                jobTimer = 0f;

                DoJob();

                Jotunn.Logger.LogInfo("NPC FINISHING JOB...");
            }
        }
    }
}
