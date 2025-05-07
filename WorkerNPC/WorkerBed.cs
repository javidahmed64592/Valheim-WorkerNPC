using Jotunn.Managers;
using Jotunn.Configs;
using Jotunn.Entities;
using UnityEngine;
using static Piece;

namespace WorkerNPC
{
    public static class WorkerBed
    {
        public static string displayName = "Worker Bed";
        public static string prefabName = "bed";
        public static string buildCategory = PieceCategory.Misc.ToString();

        public static void RegisterWorkerBed()
        {
            Main.logger.LogInfo($"Attempting to register {displayName}...");

            PrefabManager.OnPrefabsRegistered += () =>
            {
                GameObject bed = GetBaseBed();

                if (bed == null)
                {
                    Main.logger.LogError("Base bed prefab not found!");
                    return;
                }

                CustomPiece workerBed = new CustomPiece(bed, true, new PieceConfig
                {
                    Name = displayName,
                    PieceTable = "_HammerPieceTable",
                    Category = buildCategory
                });

                PieceManager.Instance.AddPiece(workerBed);
                Main.logger.LogInfo($"Registered {displayName} under {buildCategory} category.");
            };
        }

        private static GameObject GetBaseBed()
        {
            return PrefabManager.Instance.GetPrefab(prefabName);
        }
    }
}
