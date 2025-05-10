using Jotunn.Managers;
using Jotunn.Configs;
using Jotunn.Entities;
using UnityEngine;

namespace WorkerNPC
{
    internal static class WorkerChest
    {
        static string displayName = "Worker Chest";
        static string description = "A chest for storing items. Can be used by workers.";
        static string prefabName = "piece_chest_wood";
        static string customName = "worker_chest";
        static string pieceTable = GlobalConfig.pieceTable;
        static string buildCategory = GlobalConfig.buildCategory;

        public static void RegisterWorkerChest()
        {
            Jotunn.Logger.LogInfo($"Attempting to register {displayName}...");

            void CreateChest()
            {
                GameObject chestObject = GetBaseChest();
                chestObject.AddComponent<WorkerChestBehaviour>();
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

    internal class WorkerChestBehaviour : MonoBehaviour
    {

    }
}