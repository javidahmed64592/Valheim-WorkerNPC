using Jotunn.Managers;
using Jotunn.Configs;
using Jotunn.Entities;
using UnityEngine;

namespace WorkerNPC
{
    public static class WorkerBed
    {
        public static string displayName = "Worker Bed";
        public static string description = "A bed for workers to rest in. Spawns a worker NPC when placed.";
        public static string pieceTable = PieceTables.Hammer;
        public static string prefabName = "bed";
        public static string buildCategory = "Workers";

        public static void RegisterWorkerBed()
        {
            Jotunn.Logger.LogInfo($"Attempting to register {displayName}...");

            PrefabManager.OnVanillaPrefabsAvailable += () =>
            {
                GameObject bedObject = GetBaseBed();
                PieceConfig bed = new PieceConfig();
                bed.Name = displayName;
                bed.Description = description;
                bed.PieceTable = pieceTable;
                bed.Category = buildCategory;
                PieceManager.Instance.AddPiece(new CustomPiece(bedObject, false, bed));
                Jotunn.Logger.LogInfo($"Registered {displayName} under {buildCategory} category.");
            };
        }

        private static GameObject GetBaseBed()
        {
            return PrefabManager.Instance.GetPrefab(prefabName);
        }
    }
}
