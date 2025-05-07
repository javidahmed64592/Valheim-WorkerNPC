using Jotunn.Managers;
using Jotunn.Configs;
using Jotunn.Entities;
using UnityEngine;
using static Piece;

namespace WorkerNPC
{
    public static class WorkerChest
    {
        public static string displayName = "Worker Chest";
        public static string prefabName = "piece_chest_wood";
        public static string buildCategory = PieceCategory.Misc.ToString();

        public static void RegisterWorkerChest()
        {
            Main.logger.LogInfo($"Attempting to register {displayName}...");

            PrefabManager.OnPrefabsRegistered += () =>
            {
                GameObject chest = GetBaseChest();

                if (chest == null)
                {
                    Main.logger.LogError("Base chest prefab not found!");
                    return;
                }

                CustomPiece workerChest = new CustomPiece(chest, true, new PieceConfig
                {
                    Name = displayName,
                    PieceTable = "_HammerPieceTable",
                    Category = buildCategory
                });

                PieceManager.Instance.AddPiece(workerChest);
                Main.logger.LogInfo($"Registered {displayName} under {buildCategory} category.");
            };
        }

        private static GameObject GetBaseChest()
        {
            return PrefabManager.Instance.GetPrefab(prefabName);
        }
    }
}
