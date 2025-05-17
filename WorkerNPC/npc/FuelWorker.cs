using Jotunn;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WorkerNPC
{
    internal class FuelWorker : WorkerNPCBase
    {
        internal override GameObject GetBaseNPC()
        {
            displayName = FuelWorkerConfig.displayName;
            customName = FuelWorkerConfig.customName;

            GameObject clonedNPC = base.GetBaseNPC();
            clonedNPC.name = customName;
            clonedNPC.AddComponent<FuelWorkerBehaviour>();
            return clonedNPC;
        }
    }

    internal class FuelWorkerBehaviour : NPCBehaviour
    {
        int maxInventorySize = FuelWorkerConfig.maxInventorySize;
        float searchRadius = FuelWorkerConfig.searchRadius;

        internal new void Start()
        {
            base.Start();
            jobInterval = FuelWorkerConfig.jobInterval;
        }

        private int GetFireplaceRequiredFuel(Fireplace fireplace)
        {
            ZNetView fireplaceView = fireplace.GetComponent<ZNetView>();
            int currentFuel = Mathf.CeilToInt(fireplaceView.GetZDO().GetFloat(ZDOVars.s_fuel));
            int maxFuel = (int)fireplace.m_maxFuel;
            return maxFuel - currentFuel;
        }

        internal override void DoJob()
        {
            base.DoJob();

            // Find nearby fireplaces and check if they need refueling
            Fireplace[] fireplaces = FindNearbyResource<Fireplace>(searchRadius);
            Jotunn.Logger.LogInfo($"NPC scanned for fireplaces and found {fireplaces.Length} in range.");

            List<Fireplace> fireplacesToRefuel = new List<Fireplace>();

            Dictionary<string, int> fuelItems = new Dictionary<string, int>();
            foreach (Fireplace fireplace in fireplaces)
            {
                int fuelNeeded = GetFireplaceRequiredFuel(fireplace);
                if (fuelNeeded <= 0)
                {
                    continue;
                }

                fireplacesToRefuel.Add(fireplace);

                string fuelItem = fireplace.m_fuelItem.TokenName();

                if (fuelItems.ContainsKey(fuelItem))
                {
                    fuelItems[fuelItem] += fuelNeeded;
                }
                else
                {
                    fuelItems.Add(fuelItem, fuelNeeded);
                }
            }
            fireplaces = fireplacesToRefuel.ToArray();

            if (fireplaces.Length == 0)
            {
                Jotunn.Logger.LogInfo("No fireplaces need refuelling.");
                return;
            }

            // Check required fuel
            Jotunn.Logger.LogInfo($"NPC found {fireplaces.Length} fireplaces to refuel.");
            Dictionary<string, int> fuelItemsNeeded = new Dictionary<string, int>();
            foreach (KeyValuePair<string, int> fuelItem in fuelItems)
            {
                string itemName = fuelItem.Key;
                int amountNeeded = fuelItem.Value;
                int currentAmount = CheckInventory(itemName);
                int amountToTake = Mathf.Min(amountNeeded - currentAmount, maxInventorySize - currentAmount);
                if (amountToTake > 0)
                {
                    fuelItemsNeeded.Add(itemName, amountToTake);
                }
            }

            // Retrieve all required fuel
            if (fuelItemsNeeded.Count > 0)
            {
                Jotunn.Logger.LogInfo($"NPC needs to take {fuelItemsNeeded.Count} items from chests.");
                WorkerSupplyChestBehaviour[] nearbyChests = FindNearbySupplyChests(searchRadius);
                if (nearbyChests.Length == 0)
                {
                    Jotunn.Logger.LogInfo("NPC could not find any nearby chests.");
                    return;
                }

                Jotunn.Logger.LogInfo($"NPC found {nearbyChests.Length} nearby chests.");

                int chestIndex = 0;
                while (fuelItemsNeeded.Count > 0)
                {
                    if (chestIndex >= nearbyChests.Length)
                    {
                        Jotunn.Logger.LogInfo("NPC checked all chests without getting all required fuel.");
                        break;
                    }

                    WorkerSupplyChestBehaviour targetChest = nearbyChests[chestIndex];

                    foreach (KeyValuePair<string, int> fuelItem in fuelItemsNeeded.ToList())
                    {
                        if (targetChest.CountItems(fuelItem.Key) == 0)
                        {
                            Jotunn.Logger.LogInfo($"Chest has no {fuelItem.Key} left.");
                            continue;
                        }

                        if (Vector3.Distance(transform.position, targetChest.transform.position) > 2f)
                        {
                            MoveTo(targetChest.gameObject);
                        }

                        string itemName = fuelItem.Key;
                        int amountToTake = fuelItem.Value;
                        int amountTakenFromChest = targetChest.TakeItem(itemName, amountToTake);
                        Jotunn.Logger.LogInfo($"NPC took {amountTakenFromChest} {itemName} from {targetChest.name}.");

                        int amountAddedToInv = AddItemToInventory(itemName, amountTakenFromChest);
                        Jotunn.Logger.LogInfo($"NPC added {amountAddedToInv} {itemName} to inventory.");

                        fuelItemsNeeded[itemName] -= amountAddedToInv;
                        if (fuelItemsNeeded[itemName] <= 0)
                        {
                            fuelItemsNeeded.Remove(itemName);
                        }
                    }

                    chestIndex++;
                }
            }

            // Refuel fireplaces
            foreach (Fireplace fireplace in fireplaces)
            {
                string fuelItem = fireplace.m_fuelItem.TokenName();
                int fuelNeeded = GetFireplaceRequiredFuel(fireplace);
                ZNetView fireplaceView = fireplace.GetComponent<ZNetView>();

                if (fuelNeeded <= 0)
                {
                    continue;
                }

                int amountToUse = UseItemFromInventory(fuelItem, fuelNeeded);
                if (amountToUse > 0)
                {
                    MoveTo(fireplace.gameObject);
                    fireplace.AddFuel(amountToUse);
                    Jotunn.Logger.LogInfo($"NPC refueled {fireplace.name} with {amountToUse} {fuelItem}.");
                }
                else
                {
                    Jotunn.Logger.LogInfo($"NPC has no {fuelItem} left in inventory.");
                    break;
                }
            }
        }
    }
}
