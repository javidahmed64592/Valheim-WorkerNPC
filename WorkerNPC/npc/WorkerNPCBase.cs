﻿using Jotunn.Entities;
using Jotunn.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace WorkerNPC
{
    internal class WorkerNPCBase
    {
        internal static string displayName;
        internal static string customName;
        static string prefabName = PrefabNamesConfig.npcPrefabName;

        public void RegisterWorkerNPC()
        {
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
        internal Character character;
        internal ZNetView zNetView;
        internal ZDO zdo;
        internal WorkerBedBehaviour parentBed;
        internal Vector3 parentBedPosition;
        internal float jobInterval;
        float jobTimer = 0f;

        // Inventory
        internal Dictionary<string, int> inventory = new Dictionary<string, int>();
        int maxInventorySize = 50;

        internal void Start()
        {
            if (transform.parent == null)
            {
                ZNetScene.instance.Destroy(gameObject);
                return;
            }

            parentBed = transform.parent.GetComponent<WorkerBedBehaviour>();
            parentBedPosition = transform.parent.position;

            character = GetComponent<Character>();
            if (character == null)
            {
                Jotunn.Logger.LogError("Failed to get Character component.");
                ZNetScene.instance.Destroy(gameObject);
                return;
            }

            character.m_onDeath += OnNPCDeath;

            zNetView = transform.parent.GetComponent<ZNetView>();
            if (zNetView == null)
            {
                Jotunn.Logger.LogError("Failed to get ZNetView component.");
                ZNetScene.instance.Destroy(gameObject);
                return;
            }

            zdo = zNetView.GetZDO();
            if (zdo == null)
            {
                Jotunn.Logger.LogError("Failed to get ZDO.");
                ZNetScene.instance.Destroy(gameObject);
            }
        }

        internal virtual void OnNPCDeath()
        {
            parentBed.OnNPCDeath();
        }

        internal void OnDestroy()
        {
            DropItems();
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
            inventory[itemName] = newAmount;
            zdo.Set(itemName, inventory[itemName]);
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
            inventory[itemName] = newAmount;
            zdo.Set(itemName, inventory[itemName]);
            return amountToUse;
        }

        public void DropItem(string itemName, int quantity, Vector3 position)
        {
            GameObject itemPrefab = PrefabManager.Instance.GetPrefab(itemName);

            if (itemPrefab != null)
            {
                GameObject itemDrop = Instantiate(itemPrefab, position, Quaternion.identity);
                ItemDrop itemDropComponent = itemDrop.GetComponent<ItemDrop>();

                if (itemDropComponent != null)
                {
                    itemDropComponent.SetStack(quantity);
                }
                else
                {
                    Jotunn.Logger.LogWarning($"Failed to find ItemDrop component for {itemName}.");
                }
            }
            else
            {
                Jotunn.Logger.LogWarning($"Failed to find prefab for {itemName}.");
            }
        }

        internal void DropItems()
        {
            CustomLocalization localization = LocalizationManager.Instance.GetLocalization();

            foreach (KeyValuePair<string, int> item in inventory)
            {
                string itemName = localization.TryTranslate(item.Key).Trim();
                int amount = item.Value;
                if (amount > 0)
                {
                    DropItem(itemName, amount, parentBedPosition);
                }
            }
            inventory.Clear();
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

        internal void MoveTo(GameObject target)
        {
            // TODO: Move to target position
            Jotunn.Logger.LogInfo($"Moving NPC to {target.transform.position}...");
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
