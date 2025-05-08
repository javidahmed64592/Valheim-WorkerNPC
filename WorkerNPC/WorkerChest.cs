using Jotunn.Managers;
using Jotunn.Configs;
using Jotunn.Entities;
using UnityEngine;

namespace WorkerNPC
{
    public static class WorkerChest
    {
        public static string displayName = "Worker Chest";
        public static string description = "A chest for storing items. Can be used by workers.";
        public static string pieceTable = PieceTables.Hammer;
        public static string prefabName = "piece_chest_wood";
        public static string customName = "worker_chest";
        public static string buildCategory = "Workers";

        public static void RegisterWorkerChest()
        {
            Jotunn.Logger.LogInfo($"Attempting to register {displayName}...");

            void CreateChest()
            {
                GameObject chestObject = GetBaseChest();
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
}