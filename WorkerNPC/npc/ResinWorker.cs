using Jotunn;
using System.Collections.Generic;
using UnityEngine;

namespace WorkerNPC
{
    internal class ResinWorker : WorkerNPCBase
    {
        static string customName = ResinWorkerConfig.customName;

        internal override GameObject GetBaseNPC()
        {
            GameObject clonedNPC = base.GetBaseNPC();
            clonedNPC.name = customName;
            clonedNPC.AddComponent<ResinWorkerBehaviour>();
            Jotunn.Logger.LogInfo("Added ResinWorkerBehaviour to NPC prefab.");
            return clonedNPC;
        }
    }

    internal class ResinWorkerBehaviour : NPCBehaviour
    {
        // Inventory
        int maxInventorySize = ResinWorkerConfig.maxInventorySize;
        string inventoryItem = ResinWorkerConfig.inventoryItem;
        float searchRadius = ResinWorkerConfig.searchRadius;

        internal override void DoJob()
        {
            base.DoJob();
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
                MoveTo(targetChest.transform.position);

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

                    MoveTo(torch.transform.position);
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
    }
}
