using Jotunn.Managers;
using Jotunn.Configs;
using Jotunn.Entities;
using UnityEngine;

namespace WorkerNPC
{
    internal static class WorkerDepositChest
    {
        static string displayName = DepositChestConfig.displayName;
        static string description = DepositChestConfig.description;
        static string prefabName = DepositChestConfig.prefabName;
        static string customName = DepositChestConfig.customName;
        static string pieceTable = BuildMenuConfig.pieceTable;
        static string buildCategory = BuildMenuConfig.buildCategory;

        public static void RegisterWorkerChest()
        {
            void CreateChest()
            {
                GameObject chestObject = GetBaseChest();
                chestObject.AddComponent<WorkerDepositChestBehaviour>();
                PieceConfig chest = new PieceConfig
                {
                    Name = displayName,
                    Description = description,
                    PieceTable = pieceTable,
                    Category = buildCategory
                };
                PieceManager.Instance.AddPiece(new CustomPiece(chestObject, false, chest));
                Jotunn.Logger.LogInfo($"Registered {displayName} under {buildCategory} category.");
                PrefabManager.OnVanillaPrefabsAvailable -= CreateChest;
            }

            PrefabManager.OnVanillaPrefabsAvailable += CreateChest;
        }

        private static GameObject GetBaseChest()
        {
            GameObject clonedChest = PrefabManager.Instance.CreateClonedPrefab(customName, prefabName);

            if (clonedChest == null)
            {
                Jotunn.Logger.LogError("Base chest prefab not found!");
                return null;
            }

            return clonedChest;
        }
    }

    internal class WorkerDepositChestBehaviour : MonoBehaviour
    {
        Container chestContainer;

        private void Start()
        {
            chestContainer = GetComponent<Container>();
            if (chestContainer == null)
            {
                Jotunn.Logger.LogError("Failed to get Container component.");
                return;
            }
        }

        public int CountItems(string itemName)
        {
            Inventory chestInventory = chestContainer.GetInventory();
            if (chestInventory == null)
            {
                Jotunn.Logger.LogError("Chest inventory is missing.");
                return 0;
            }

            return chestInventory.CountItems(itemName);
        }

        public int DepositItem(string itemName, int amount)
        {
            Inventory chestInventory = chestContainer.GetInventory();
            int itemCount = CountItems(itemName);

            int availableAmount = Mathf.Min(itemCount, amount);
            //chestInventory.AddItem(itemName, availableAmount);
            return availableAmount;
        }
    }
}