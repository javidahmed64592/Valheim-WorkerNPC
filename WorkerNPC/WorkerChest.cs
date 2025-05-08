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

            PrefabManager.OnVanillaPrefabsAvailable += () =>
            {
                GameObject chestObject = GetBaseChest();
                PieceConfig chest = new PieceConfig();
                chest.Name = displayName;
                chest.Description = description;
                chest.PieceTable = pieceTable;
                chest.Category = buildCategory;
                PieceManager.Instance.AddPiece(new CustomPiece(chestObject, false, chest));
                Jotunn.Logger.LogInfo($"Registered {displayName} under {buildCategory} category.");
            };
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